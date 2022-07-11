using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    const float speed = 50f;

    CapsuleCollider capsuleCollider;

    public Transform target;

    const float maxAngleDiff = 90;
    const float fov = 90;

    void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();
    }

    void Update() {
        if (target != null) {
            Vector3 dir = (target.position - transform.position).normalized;

            float maxAngle = maxAngleDiff * Time.deltaTime;

            float angle = Vector3.Angle(dir, transform.forward);
            if (angle > 0.01f && angle < fov) {
                Quaternion q = Quaternion.AngleAxis(-Mathf.Min(maxAngle, Vector3.Angle(dir, transform.forward)), Vector3.Cross(dir, transform.forward));
                transform.forward = q * transform.forward;
            }
        }

        Vector3 velocity = transform.forward * speed;

        Vector3 delta = velocity * Time.deltaTime;

        RaycastHit hit;
        if (Physics.CapsuleCast(
            transform.position + capsuleCollider.center - (capsuleCollider.height/2 + capsuleCollider.radius) * transform.forward,
            transform.position + capsuleCollider.center + (capsuleCollider.height/2 - capsuleCollider.radius) * transform.forward,
            capsuleCollider.radius, delta.normalized, out hit, delta.magnitude
        )) {
            Destroy(gameObject);

            FindObjectOfType<ParticleManager>().CreateMissileExplosion(hit.point);
        }

        transform.position = transform.position + delta;
    }
}
