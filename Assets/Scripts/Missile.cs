using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    const float speed = 50f;

    const float lifetime = 10;

    float spawnTimestamp;

    CapsuleCollider capsuleCollider;

    public Transform target;

    const float maxAngleDiff = 80;
    const float fov = 90;

    void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();

        spawnTimestamp = Time.time;
    }

    void Explode(Vector3 at) {
        transform.position = at;

        Destroy(gameObject);
        FindObjectOfType<ParticleManager>().CreateMissileExplosion(transform.position);
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

        if (Time.time - spawnTimestamp >= lifetime) {
            Explode(transform.position);
            return;
        }

        Vector3 velocity = transform.forward * speed;

        Vector3 delta = velocity * Time.deltaTime;

        RaycastHit hit;
        if (Physics.CapsuleCast(
            transform.position + capsuleCollider.center - (capsuleCollider.height/2 + capsuleCollider.radius) * transform.forward,
            transform.position + capsuleCollider.center + (capsuleCollider.height/2 - capsuleCollider.radius) * transform.forward,
            capsuleCollider.radius, delta.normalized, out hit, delta.magnitude, ~LayerMask.GetMask("Missile")
        )) {
            Explode(hit.point);
            return;
        }

        transform.position = transform.position + delta;
    }
}
