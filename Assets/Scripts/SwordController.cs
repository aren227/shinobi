using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    Mech mech;

    const float maxAngle = 60;

    SwordCanvas swordCanvas;

    Vector2 current;

    public Vector2 dir => current.normalized;

    const float sensitivity = 0.3f;
    const float maxRadius = 1.0f;

    public Mech target;

    void Awake() {
        mech = GetComponent<Mech>();
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

        SwordHitShape hitShape;
        if (target != null) {
            hitShape = mech.GetSwordHitShape(target);
            hitShape.offset = Mathf.Lerp(-1, 1, Mathf.PerlinNoise(0, Time.time)) * 60;
        }
        else {
            hitShape = new SwordHitShape();
            for (int i = 0; i < 6; i++) hitShape.proportions[i] = 0;
        }

        swordCanvas.SetSwordHitShape(hitShape);
    }
}
