using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SwordController2 : MonoBehaviour
{
    Mech mech;
    Mech target;

    public bool isRightHanded { get; private set; } = true;

    const float depth = 6;
    const float radius = 1f;

    const float beginSweep = 0f;
    const float beginHit = 0.11f;
    const float endHit = 0.15f;
    const float endSweep = 0.33f;
    const float endAttack = 1f;

    float beginTimestamp;

    public SwordSwingState state { get; private set; }

    Transform swordTransform;

    RaycastHit[] hits = new RaycastHit[64];
    Collider[] colliders = new Collider[64];

    bool failed = false;

    SwordVelocitySolver swordVelocitySolver;

    const float sliceCubeSize = 10;

    void Awake() {
        mech = GetComponent<Mech>();
    }

    public void BeginAttack(Mech target) {
        if (!mech.isUsingSword) return;

        this.target = target;

        swordTransform = mech.inventory.GetItem(Inventory.Slot.SWORD).transform;

        state = SwordSwingState.PREPARE;

        // mech.disableMovement = true;

        beginTimestamp = Time.time;

        mech.skeleton.BeginMeleeAttackAnimation(isRightHanded);

        swordVelocitySolver = new SwordVelocitySolver(mech, target);

        failed = false;

        if (target != null) {
            if (target.boost) {
                target.EndBoost();
            }

            target.GetComponent<EnemyMechController>().OverrideTargetPos(
                (target.transform.position - mech.transform.position).normalized * 100 + target.transform.position,
                0.5f
            );

            if (target.BeginBoost()) {
                failed = true;
            }
        }
        else {
            failed = true;
        }

        if (!failed) {
            swordVelocitySolver.followVictim = true;
        }

        mech.AddVelocitySolver(swordVelocitySolver);
        if (!failed) {
            target.AddVelocitySolver(swordVelocitySolver);

            target.BeginSlice();
        }
    }

    // public void SyncSliceCubeToSword() {
    //     sliceCube.transform.position =
    //         swordTransform.position
    //             - swordTransform.rotation * Vector3.forward * sliceCubeSize / 2
    //             + swordTransform.rotation * Vector3.up * sliceCubeSize / 2;

    //     sliceCube.transform.rotation = swordTransform.rotation;
    // }

    public void EndAttack() {
        state = SwordSwingState.IDLE;

        // mech.skeleton.DisableHandIk(isRightHanded);

        mech.targetMetalCrackingVolume = 0;

        mech.RemoveVelocitySolver(swordVelocitySolver);

        if (!failed) {
            target.RemoveVelocitySolver(swordVelocitySolver);
            target.Kill();
        }

        Time.timeScale = 1f;

        // target.skeleton.GetPart(PartName.BODY).Hit(10000);
    }

    public void SetRightHand(bool rightHand) {
        isRightHanded = rightHand;

        Vector3 scale = mech.skeleton.swordSwingMirror.localScale;
        scale.x *= -1;
        mech.skeleton.swordSwingMirror.localScale = scale;

        // Update sword hand pivot.
        mech.UpdatePivots();
    }

    void Update() {
        if (state == SwordSwingState.IDLE) {
            return;
        }

        float t = Time.time - beginTimestamp;

        if (!failed) {
            mech.Aim(target.skeleton.cockpit.transform.position);
        }

        if (state == SwordSwingState.SWING) {
            // CapsuleCollider capsuleCollider = swordTransform.GetComponent<Sword>().coll;

            // Vector3 direction = new Vector3 {[capsuleCollider.direction] = 1};
            // float offset = capsuleCollider.height / 2 - capsuleCollider.radius;
            // Vector3 localPoint0 = capsuleCollider.center - direction * offset;
            // Vector3 localPoint1 = capsuleCollider.center + direction * offset;

            // Vector3 point0 = capsuleCollider.transform.TransformPoint(localPoint0);
            // Vector3 point1 = capsuleCollider.transform.TransformPoint(localPoint1);

            // int count = Physics.OverlapCapsuleNonAlloc(
            //     point0, point1, 0.2f, colliders, LayerMask.GetMask("SwordHit"), QueryTriggerInteraction.Collide
            // );

            // bool hitting = false;
            // for (int i = 0; i < count; i++) {
            //     Mech mech = colliders[i].transform.root.GetComponent<Mech>();
            //     if (mech == target) {
            //         hitting = true;
            //         break;
            //     }
            // }

            // Debug.Log(hitting);

            if (!failed) {
                bool hitting = beginHit <= t && t <= endHit;

                if (hitting) {
                    Time.timeScale = 0.04f;
                }
                else {
                    Time.timeScale = 1f;
                }
            }
        }

        if (state == SwordSwingState.PREPARE && t >= beginSweep) {
            state = SwordSwingState.SWING;
        }
        if (state == SwordSwingState.SWING && t >= endSweep) {
        }
        if (state == SwordSwingState.SWING && t >= endAttack) {
            EndAttack();
        }
    }
}

public enum SwordSwingState {
    IDLE,
    PREPARE,
    SWING,
    HIT,
    BLOCKED,
    RETURN,
}