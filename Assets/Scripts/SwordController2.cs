using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SwordController2 : MonoBehaviour
{
    Mech mech;
    Mech target;

    Quaternion swingPivotRotation => mech.skeleton.swordSwingPivot.rotation;

    Vector3 swingPivot => mech.skeleton.swordSwingPivot.position;

    Vector3 swingNormal => swingPivotRotation * Vector3.up;

    public bool isRightHanded { get; private set; } = true;

    Vector3 targetAttackPos;


    public AnimationCurve swingAngleVelocityCurve;

    float swingTime = 0.2f;
    float blockBackSwingTime = 0.3f;
    float blockBackSwingAmount = 1f;

    float beginDepth = 1f;
    float endDepth = 4f;

    float _beginAngle = 90f;
    float beginAngle {
        get {
            if (isRightHanded) return _beginAngle;
            return _beginAngle * -1;
        }
    }

    float deathAngle = 75;
    float maxAngle = 120f;

    float swordSwingT = 0;

    float angleVel;
    float angleAcc; // for SmoothDamp
    float angle;
    Quaternion rot;

    public SwordSwingState state { get; private set; }

    Transform swordTransform;

    // MotionDriver motionDriver;
    SwordVelocitySolver swordVelocitySolver;

    GameObject sliceCube;

    void Awake() {
        mech = GetComponent<Mech>();
    }

    public void BeginAttack(Mech target) {
        if (!mech.isUsingSword) return;

        swordTransform = mech.inventory.GetItem(Inventory.Slot.SWORD).transform;

        state = SwordSwingState.PREPARE;
        angle = beginAngle;

        angleVel = 0;

        this.target = target;

        targetAttackPos = mech.GetMeleeAttackPos(target);

        mech.disableMovement = true;
        target.disableMovement = true;

        // motionDriver = new MotionDriver(mech, target);

        swordVelocitySolver = new SwordVelocitySolver(mech, target);

        swordVelocitySolver.Begin();

        sliceCube = Instantiate(PrefabRegistry.Instance.sliceEffectBox);
        sliceCube.transform.parent = target.transform;
        sliceCube.transform.localScale = Vector3.zero;

        target.skeleton.SetHoleCube(sliceCube.GetComponent<MeshFilter>());

        // swingPivotRotation = mech.skeleton.swordSwingPivot.rotation;
    }

    public void EndAttack() {
        swordTransform.localPosition = Vector3.zero;
        swordTransform.localRotation = Quaternion.identity;
        swordTransform = null;

        state = SwordSwingState.IDLE;

        mech.disableMovement = false;
        target.disableMovement = false;

        mech.skeleton.DisableHandIk(isRightHanded);

        swordVelocitySolver.Finish();

        target.skeleton.GetPart(PartName.BODY).Hit(10000);
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

        mech.skeleton.EnableHandIk(isRightHanded, true, 0.1f);

        swordVelocitySolver.Update();

        if (state == SwordSwingState.PREPARE) {
            if (swordVelocitySolver.approached) {
                state = SwordSwingState.SWING;
            }
        }
        else {
            float targetAngleVel = 0;
            float smoothTime = 0.15f;
            if (state == SwordSwingState.SWING) {
                if (Mathf.Abs(angle - beginAngle) >= maxAngle) targetAngleVel = 0;
                else targetAngleVel = -700f;

                smoothTime = 0.05f;
            }
            else if (state == SwordSwingState.BLOCKED) {
                smoothTime = 0.07f;
            }
            else if (state == SwordSwingState.HIT) {
                if (Input.GetMouseButton(0)) {
                    smoothTime = 2f;
                    targetAngleVel = -700f;
                }
                else {
                    smoothTime = 0.05f;
                }
            }

            if (!isRightHanded) targetAngleVel *= -1;

            angleVel = Mathf.SmoothDamp(angleVel, targetAngleVel, ref angleAcc, smoothTime);

            angle += angleVel * Time.deltaTime;

            rot = swingPivotRotation * Quaternion.AngleAxis(angle, Vector3.up);

            if (state == SwordSwingState.SWING) {
                Collider[] colliders = Physics.OverlapCapsule(
                    swingPivot + rot * Vector3.forward * beginDepth,
                    swingPivot + rot * Vector3.forward * endDepth,
                    0.2f,
                    LayerMask.GetMask("Frame", "Armor")
                );

                bool targetHit = false;

                foreach (Collider collider in colliders) {
                    if (collider.GetComponentInParent<Mech>() == target) {
                        targetHit = true;
                        break;
                    }
                }

                if (targetHit) {
                    Debug.Log("Begin Hit");

                    state = SwordSwingState.HIT;

                    // Reduce velocity
                    angleVel = 0;
                    angleAcc = 0;
                }
            }
            else if (state == SwordSwingState.HIT) {
                Collider[] colliders = Physics.OverlapCapsule(
                    swingPivot + rot * Vector3.forward * beginDepth,
                    swingPivot + rot * Vector3.forward * endDepth,
                    0.2f,
                    LayerMask.GetMask("Frame", "Armor")
                );

                bool targetHit = false;

                foreach (Collider collider in colliders) {
                    if (collider.GetComponentInParent<Mech>() == target) {
                        targetHit = true;
                        break;
                    }
                }

                if (!targetHit) {
                    Debug.Log("End Hit");

                    state = SwordSwingState.SWING;
                }
            }

            // if (Mathf.Abs(beginAngle - angle) > deathAngle) {
            //     target.Kill();
            // }

            swordTransform.position = swingPivot + rot * Vector3.forward * beginDepth;
            swordTransform.rotation = rot;

            Vector3 rotForward = rot * Vector3.forward;
            Vector3 rotRight = rot * Vector3.right;

            if (isRightHanded) {
                sliceCube.transform.position = swingPivot + rotForward * 10 + rotRight * 10;
            }
            else {
                sliceCube.transform.position = swingPivot + rotForward * 10 - rotRight * 10;
            }
            sliceCube.transform.rotation = rot;
            sliceCube.transform.localScale = new Vector3(20, 0.2f, 20);

            if (state != SwordSwingState.HIT && targetAngleVel == 0 && Mathf.Abs(angleVel) < 5f) {
                EndAttack();
            }
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