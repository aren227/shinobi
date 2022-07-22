using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Camera cam;

    public float pitch = 0, yaw = 0, roll = 0;

    float rollVel;

    public Transform cameraArm;
    public Transform cameraTarget;

    Vector2 prevMouse;
    public Vector2 mouseSensitivity = Vector2.one;

    Vector3 posVel;

    public Transform locked;

    Vector3 offset;

    const float shootXOffset = 4f;

    void Awake() {
        cam = Camera.main;
        prevMouse = Input.mousePosition;

        Cursor.lockState = CursorLockMode.Locked;

        // shootOffset = cam.transform.localPosition;
        // swordOffset = new Vector3(0, shootOffset.y, shootOffset.z);

        offset = new Vector3(0, 3.5f, -7f);

        cam.transform.parent = cameraTarget;
        cam.transform.localPosition = Vector3.zero;
        cam.transform.localRotation = Quaternion.identity;
    }

    void Update() {
        if (GameManager.Instance.isPaused) return;

        Mech mech = GameManager.Instance.player;

        if (!mech) return;

        const float velocityResponsiveness = 1 / 40f;

        Vector3 origin = mech.transform.position - mech.velocity * velocityResponsiveness;

        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        mouseDelta *= mouseSensitivity;

        // Do not update yaw, pitch in bullet time.
        if (!mech.isBulletTime) {
            if (locked) {
                Vector3 fromTo = locked.position - cameraArm.position;

                yaw = -Mathf.Atan2(fromTo.z, fromTo.x) * Mathf.Rad2Deg + 90;

                transform.localEulerAngles = new Vector3(0, yaw, 0);

                fromTo = locked.position - cameraTarget.position;

                Vector3 proj = new Vector3(fromTo.x, 0, fromTo.z);
                pitch = -Mathf.Atan2(fromTo.y, proj.magnitude) * Mathf.Rad2Deg;
            }
            else {
                yaw = (yaw + mouseDelta.x) % 360;
                pitch = Mathf.Clamp(pitch - mouseDelta.y, -60, 60);
            }
        }

        Quaternion yawRot = Quaternion.AngleAxis(yaw, Vector3.up);
        Quaternion pitchRot = Quaternion.AngleAxis(pitch, yawRot * Vector3.right);

        Quaternion rot = pitchRot * yawRot;

        float xShift = 0;
        if (!mech.isUsingSword) xShift = shootXOffset;

        Vector3 rotatedOffset = origin;

        Vector3 vec = yawRot * Vector3.right * xShift;

        const float camSphereRadius = 0.5f;

        RaycastHit hit;
        float dist = vec.magnitude;
        if (Physics.SphereCast(rotatedOffset, camSphereRadius, vec.normalized, out hit, vec.magnitude, LayerMask.GetMask("Ground", "Objective"))) {
            dist = Mathf.Max(hit.distance, 0);
        }

        rotatedOffset += vec.normalized * dist;

        vec = rot * offset;
        dist = vec.magnitude;
        if (Physics.SphereCast(rotatedOffset, camSphereRadius, vec.normalized, out hit, vec.magnitude, LayerMask.GetMask("Ground", "Objective"))) {
            dist = Mathf.Max(hit.distance, 0);
        }

        rotatedOffset += vec.normalized * dist;

        cameraTarget.position = rotatedOffset;
        cameraTarget.rotation = rot;

        Vector3 cameraSpaceSpeed = rot * mech.velocity;

        const float rollSensitivity = 0.1f;
        const float maxRoll = 2.5f;

        float x = cameraSpaceSpeed.x * rollSensitivity;
        float tanh = (Mathf.Exp(2 * x) - 1) / (Mathf.Exp(2 * x) + 1);

        float targetRoll = tanh * maxRoll;
        roll = Mathf.SmoothDamp(roll, targetRoll, ref rollVel, 0.1f);

        cam.transform.localEulerAngles = new Vector3(0, 0, roll);

        prevMouse = Input.mousePosition;
    }

    public Quaternion GetCameraRotation() {
        // Roll is ignored.
        return Quaternion.Euler(pitch, yaw, 0);
    }
}
