using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    const float maxAngle = 60;

    SwordCanvas swordCanvas;

    Vector2 current;

    public Vector2 dir => current.normalized;

    const float sensitivity = 0.3f;
    const float maxRadius = 1.0f;

    void Awake() {
        swordCanvas = FindObjectOfType<SwordCanvas>();
    }

    public void ResetOrigin() {
        current = Random.insideUnitCircle.normalized;
    }

    void Update() {
        current += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * sensitivity;

        // current = current.normalized;
        if (current.magnitude > maxRadius) current = current.normalized * maxRadius;

        swordCanvas.SetAim(dir);

        SwordHitShape hitShape = new SwordHitShape();
        hitShape.offset = Mathf.Lerp(-1, 1, Mathf.PerlinNoise(0, Time.time)) * 60;

        swordCanvas.SetSwordHitShape(hitShape);
    }
}
