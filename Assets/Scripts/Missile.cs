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
    const float explosionRadius = 1f;
    const int explosionDamage = 10;

    void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();

        spawnTimestamp = Time.time;
    }

    void Explode(Vector3 at) {
        transform.position = at;

        Destroy(gameObject);
        FindObjectOfType<ParticleManager>().CreateMissileExplosion(transform.position);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider collider in colliders) {
            Vector3 target = collider.ClosestPoint(at);
            Vector3 v = (target - at);

            RaycastHit hit;
            bool blocked = false;
            if (Physics.Raycast(at, v.normalized, out hit, v.magnitude)) {
                if (hit.distance < v.magnitude - 0.01f) blocked = true;
            }

            if (blocked) continue;

            Mech mech = collider.GetComponentInParent<Mech>();
            if (mech) {
                mech.GiveDamage(collider, explosionDamage);
            }
        }
    }

    void Update() {
        if (target != null) {
            Mech mech = target.GetComponentInParent<Mech>();
            if (mech == null || !mech.isHided) {
                Vector3 dir = (target.position - transform.position).normalized;

                float maxAngle = maxAngleDiff * Time.deltaTime;

                float angle = Vector3.Angle(dir, transform.forward);
                if (angle > 0.01f && angle < fov) {
                    Quaternion q = Quaternion.AngleAxis(-Mathf.Min(maxAngle, Vector3.Angle(dir, transform.forward)), Vector3.Cross(dir, transform.forward));
                    transform.forward = q * transform.forward;
                }
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
            Explode(hit.point + hit.normal * 0.1f);
            return;
        }

        transform.position = transform.position + delta;
    }
}
