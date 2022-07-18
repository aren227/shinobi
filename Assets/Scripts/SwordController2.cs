using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController2 : MonoBehaviour
{
    Mech mech;

    Vector3 swingPivot => mech.skeleton.swordSwingPivot.position;

    Vector3 swingNormal => mech.skeleton.swordSwingPivot.rotation * Vector3.up;

    public bool isRightHanded { get; private set; } = true;


    public AnimationCurve swingAngleVelocityCurve;

    float swingTime = 0.2f;
    float blockBackSwingTime = 0.3f;
    float blockBackSwingAmount = 1f;

    float beginDepth = 1f;
    float endDepth = 4f;

    float _beginAngle = 80f;
    float beginAngle {
        get {
            if (isRightHanded) return _beginAngle;
            return _beginAngle * -1;
        }
    }
    
    float maxAngle = 80f;

    float swordSwingT = 0;

    float angleVel;
    float angleAcc; // for SmoothDamp
    float angle;
    Quaternion rot;

    public SwordSwingState state { get; private set; }

    Transform swordTransform;

    Mech targetMech;
    Part targetPart;

    void Awake() {
        mech = GetComponent<Mech>();
    }

    public void BeginSwing() {
        if (!mech.isUsingSword) return;

        swordTransform = mech.inventory.GetItem(Inventory.Slot.SWORD).transform;

        state = SwordSwingState.SWING;
        angle = beginAngle;
        
        angleVel = 0;

        targetMech = null;
        targetPart = null;
    }

    public void EndSwing() {
        swordTransform.localPosition = Vector3.zero;
        swordTransform.localRotation = Quaternion.identity;
        swordTransform = null;

        state = SwordSwingState.IDLE;
    }

    public void SwitchHand() {
        if (state != SwordSwingState.IDLE) return;

        isRightHanded = !isRightHanded;

        Vector3 scale = mech.skeleton.swordSwingMirror.localScale;
        scale.x *= -1;
        mech.skeleton.swordSwingMirror.localScale = scale;

        // Update sword hand pivot.
        mech.UpdatePivots();
    }

    void Update() {
        if (!swordTransform) return;

        float targetAngleVel = 0;
        float smoothTime = 0.15f;
        if (state == SwordSwingState.SWING) {
            if (Mathf.Abs(angle - beginAngle) >= maxAngle) targetAngleVel = 0;
            else targetAngleVel = -700f;

            smoothTime = 0.15f;
        }
        else if (state == SwordSwingState.BLOCKED) {
            smoothTime = 0.07f;
        }
        else if (state == SwordSwingState.HIT) {
            if (Input.GetMouseButton(0)) {
                smoothTime = 3f;
                targetAngleVel = -700f;
            }
            else {
                smoothTime = 0.05f;
            }
        }

        if (!isRightHanded) targetAngleVel *= -1;

        angleVel = Mathf.SmoothDamp(angleVel, targetAngleVel, ref angleAcc, smoothTime);

        angle += angleVel * Time.deltaTime;

        rot = Quaternion.AngleAxis(angle, swingNormal) * mech.transform.rotation;

        if (state == SwordSwingState.SWING) {
            // If we already destroyed the part, prevent to do it one more time.
            if (targetPart == null) {
                RaycastHit[] hits = Physics.CapsuleCastAll(
                    swingPivot + rot * Vector3.forward * beginDepth,
                    swingPivot + rot * Vector3.forward * endDepth,
                    0.1f,
                    rot * Vector3.left,
                    0.4f,
                    LayerMask.GetMask("Frame", "Armor")
                );

                float minDist = float.PositiveInfinity;
                RaycastHit hit = new RaycastHit();

                for (int i = 0; i < hits.Length; i++) {
                    // Ignore self
                    if (hits[i].collider.GetComponentInParent<Mech>() == mech) continue;

                    if (hits[i].distance < minDist) {
                        minDist = hits[i].distance;
                        hit = hits[i];
                    }
                }

                if (minDist != float.PositiveInfinity) {
                    if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Frame")) {
                        targetMech = hit.collider.GetComponentInParent<Mech>();
                        targetPart = targetMech.skeleton.GetPartByCollider(hit.collider);

                        if (targetPart) {
                            Debug.Log("Frame hit.");

                            state = SwordSwingState.HIT;

                            // Reduce velocity
                            angleVel = 0;
                            angleAcc = 0;
                        }
                        else {
                            Debug.Log("Part not found!");
                        }

                    }
                    else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Armor")) {
                        Debug.Log("Armor hit. Blocked.");

                        state = SwordSwingState.BLOCKED;

                        // Reflect
                        angleVel *= -1;
                    }
                }
            }
        }
        else if (state == SwordSwingState.HIT) {
            RaycastHit[] hits = Physics.CapsuleCastAll(
                swingPivot + rot * Vector3.forward * beginDepth,
                swingPivot + rot * Vector3.forward * endDepth,
                0.1f,
                rot * Vector3.left,
                0.4f,
                LayerMask.GetMask("Frame")
            );

            bool stopHit = true;
            foreach (RaycastHit hit in hits) {
                if (targetPart.frameColliders.IndexOf(hit.collider) != -1) {
                    stopHit = false;
                    break;
                }
            }

            if (stopHit) {
                Debug.Log("Hit end.");

                targetPart.Slice();

                state = SwordSwingState.SWING;
            }
        }

        swordTransform.position = swingPivot + rot * Vector3.forward * beginDepth;
        swordTransform.rotation = rot;

        if (state != SwordSwingState.HIT && targetAngleVel == 0 && Mathf.Abs(angleVel) < 5f) {
            EndSwing();
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