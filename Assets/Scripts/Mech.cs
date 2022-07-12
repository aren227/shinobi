using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mech : MonoBehaviour
{
    Rigidbody rigid;

    CapsuleParams kinematicCapsule;

    public CameraController cameraController;
    UiManager uiManager;

    VelocitySolver velocitySolver;

    public Transform leftWeaponPivot;
    public Transform rightWeaponPivot;

    public Vector3 velocity;

    Vector3 accumulatedDelta;

    bool boost = false;

    const float maxStemina = 500;
    float stemina = 0;

    const float steminaConsumRate = 10;
    const float steminaRestoreRate = 5;
    const float steminaRequiredToBoost = 10;

    public List<ThermalTarget> thermalTargets = new List<ThermalTarget>();

    public MechArmature mechArmature;

    void Awake() {
        cameraController = FindObjectOfType<CameraController>();
        uiManager = FindObjectOfType<UiManager>();

        rigid = GetComponent<Rigidbody>();

        velocitySolver = FindObjectOfType<AccelerationBasedVelocitySolver>();

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        kinematicCapsule = new CapsuleParams() { center = capsuleCollider.center, radius = capsuleCollider.radius, height = capsuleCollider.height };
        Destroy(capsuleCollider);

        stemina = maxStemina;
        uiManager.SetMaxStemina(maxStemina);

        GetComponentInChildren<BulletWeapon>().SetOwner(this);
        GetComponentInChildren<MissileWeapon>().SetOwner(this);
    }

    void Update() {
        Vector3[] dirs = new Vector3[] {
            Vector3.forward, Vector3.back, Vector3.left, Vector3.right, Vector3.up, Vector3.down
        };
        KeyCode[] keys = new KeyCode[] {
            KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Space, KeyCode.LeftControl
        };

        Vector3 moveDir = Vector3.zero;
        for (int i = 0; i < 6; i++) {
            if (Input.GetKey(keys[i])) {
                moveDir += dirs[i];
            }
        }
        moveDir.Normalize();

        moveDir = cameraController.GetCameraRotation() * moveDir;

        if (boost) {
            stemina = Mathf.Max(stemina - steminaConsumRate * Time.deltaTime, 0);

            bool boostInput = Input.GetKey(KeyCode.LeftShift) && moveDir.sqrMagnitude > 0;

            if (!boostInput || stemina <= 0) {
                boost = false;
            }
        }
        else {
            stemina = Mathf.Min(stemina + steminaRestoreRate * Time.deltaTime, maxStemina);
            if (Input.GetKeyDown(KeyCode.LeftShift) && moveDir.sqrMagnitude > 0 && stemina >= steminaRequiredToBoost) {
                stemina -= steminaRequiredToBoost;
                boost = true;
            }
        }

        velocity = velocitySolver.Update(moveDir, boost);

        accumulatedDelta += velocity * Time.deltaTime;

        leftWeaponPivot.forward = cameraController.cameraTarget.forward;
        rightWeaponPivot.forward = cameraController.cameraTarget.forward;

        thermalTargets = GetVisibleThermalTargets();

        uiManager.SetStemina(stemina);
        uiManager.SetSpeed(velocity.magnitude);
        uiManager.SetThermalTargets(thermalTargets, cameraController.cam);
    }

    void FixedUpdate() {
        Vector3 delta = accumulatedDelta;

        accumulatedDelta = Vector3.zero;

        const float pad = 0.1f;
        for (int i = 0; i < 3; i++) {
            RaycastHit hit;
            if (delta.sqrMagnitude > 1e-5f && Physics.CapsuleCast(
                rigid.position + kinematicCapsule.center - (kinematicCapsule.height/2 + kinematicCapsule.radius) * Vector3.up,
                rigid.position + kinematicCapsule.center + (kinematicCapsule.height/2 - kinematicCapsule.radius) * Vector3.up,
                kinematicCapsule.radius, delta.normalized, out hit, delta.magnitude + pad, LayerMask.GetMask("Kinematic"))) {

                Vector3 safe = delta.normalized * Mathf.Max(hit.distance - pad, 0);
                rigid.MovePosition(rigid.position + safe);

                delta = Vector3.ProjectOnPlane(delta, hit.normal);
            }
            else {
                rigid.MovePosition(rigid.position + delta);

                break;
            }
        }

        // if (moveDir.sqrMagnitude > 0) {
        rigid.MoveRotation(Quaternion.Euler(0, cameraController.cameraArm.eulerAngles.y, 0));
        // }
    }

    List<ThermalTarget> GetVisibleThermalTargets() {
        ThermalTarget[] targets = FindObjectsOfType<ThermalTarget>();
        List<ThermalTarget> result = new List<ThermalTarget>();

        Camera cam = cameraController.cam;

        RaycastHit[] hits = new RaycastHit[32];

        foreach (ThermalTarget target in targets) {
            Vector2 viewport = cam.WorldToViewportPoint(target.transform.position);
            if (0 <= viewport.x && viewport.x <= 1 && 0 <= viewport.y && viewport.y <= 1) {
                const float sphereRadius = 0.5f;
                Vector3 camToTarget = target.transform.position - cam.transform.position;

                int count = Physics.SphereCastNonAlloc(
                    cam.transform.position, sphereRadius, camToTarget.normalized, hits, camToTarget.magnitude, ~LayerMask.GetMask("Missile")
                );

                bool success = true;
                for (int i = 0; i < count; i++) {
                    if (hits[i].collider.transform.IsChildOf(transform)) continue;
                    if (hits[i].collider.gameObject == target.gameObject) continue;

                    success = false;
                    break;
                }

                if (success) {
                    result.Add(target);
                }
            }
        }

        return result;
    }
}

class CapsuleParams {
    public Vector3 center;
    public float radius;
    public float height;
}