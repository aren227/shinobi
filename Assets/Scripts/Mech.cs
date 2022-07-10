using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mech : MonoBehaviour
{
    Rigidbody rigid;

    CapsuleParams kinematicCapsule;

    CameraController cameraController;
    UiManager uiManager;

    VelocitySolver velocitySolver;

    public Vector3 velocity;

    Vector3 accumulatedDelta;

    bool boost = false;

    const float maxStemina = 100;
    float stemina = 0;

    const float steminaConsumRate = 10;
    const float steminaRestoreRate = 5;
    const float steminaRequiredToBoost = 10;

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

        uiManager.SetStemina(stemina);
        uiManager.SetSpeed(velocity.magnitude);
    }

    void FixedUpdate() {
        Vector3 delta = accumulatedDelta;

        accumulatedDelta = Vector3.zero;

        RaycastHit hit;
        if (delta.sqrMagnitude > 0 && Physics.CapsuleCast(
            rigid.position - (kinematicCapsule.height/2 + kinematicCapsule.radius) * Vector3.up,
            rigid.position + (kinematicCapsule.height/2 - kinematicCapsule.radius) * Vector3.up, kinematicCapsule.radius, delta.normalized, out hit, delta.magnitude, LayerMask.GetMask("Kinematic"))) {
            delta = Vector3.zero;
        }

        rigid.MovePosition(rigid.position + delta);

        // if (moveDir.sqrMagnitude > 0) {
        rigid.MoveRotation(Quaternion.Euler(0, cameraController.cameraArm.eulerAngles.y, 0));
        // }
    }
}

class CapsuleParams {
    public Vector3 center;
    public float radius;
    public float height;
}