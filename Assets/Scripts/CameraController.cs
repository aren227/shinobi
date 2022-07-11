using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Camera cam;
    Mech mech;

    float pitch = 0, yaw = 0, roll = 0;

    float rollVel;

    public Transform cameraArm;
    public Transform cameraTarget;

    Vector2 prevMouse;
    public Vector2 mouseSensitivity = Vector2.one;

    Vector3 posVel;

    void Awake() {
        cam = Camera.main;
        prevMouse = Input.mousePosition;
        // @Temp
        mech = FindObjectOfType<Mech>();

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        const float velocityResponsiveness = 1 / 30f;

        transform.position = mech.transform.position - mech.velocity * velocityResponsiveness;
        // transform.position = Vector3.SmoothDamp(transform.position, mech.transform.position, ref posVel, 0.03f);

        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        mouseDelta *= mouseSensitivity;

        yaw = (yaw + mouseDelta.x) % 360;
        pitch = Mathf.Clamp(pitch - mouseDelta.y, -89, 89);

        transform.localEulerAngles = new Vector3(0, yaw, 0);
        cameraArm.localEulerAngles = new Vector3(pitch, 0, 0);

        // Vector3 targetLocalPos = cameraTarget.localPosition;
        // targetLocalPos.z = Mathf.LerpUnclamped(-6, -8, mech.boost ? 1 : 0);
        // cameraTarget.localPosition = targetLocalPos;

        Vector3 cameraSpaceSpeed = cameraTarget.rotation * mech.velocity;

        const float rollSensitivity = 0.1f;
        const float maxRoll = 2.5f;

        float x = cameraSpaceSpeed.x * rollSensitivity;
        float tanh = (Mathf.Exp(2 * x) - 1) / (Mathf.Exp(2 * x) + 1);

        float targetRoll = tanh * maxRoll;
        roll = Mathf.SmoothDamp(roll, targetRoll, ref rollVel, 0.1f);

        cameraTarget.localEulerAngles = new Vector3(0, 0, roll);

        cam.transform.position = cameraTarget.position;
        cam.transform.rotation = cameraTarget.rotation;

        prevMouse = Input.mousePosition;
    }

    public Quaternion GetCameraRotation() {
        // Roll is ignored.
        return Quaternion.Euler(pitch, yaw, 0);
    }
}
