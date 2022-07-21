using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    const float speed = 600f;
    const float lifetime = 10;
    const int damage = 10;

    float spawnTimestamp;

    CapsuleCollider capsuleCollider;

    public Mech owner;

    RaycastHit[] hits = new RaycastHit[64];

    void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();

        spawnTimestamp = Time.time;
    }

    void Update() {
        Vector3 velocity = transform.forward * speed;

        Vector3 delta = velocity * Time.deltaTime;

        int count = Physics.CapsuleCastNonAlloc(
            transform.position + capsuleCollider.center - (capsuleCollider.height/2 + capsuleCollider.radius) * transform.forward,
            transform.position + capsuleCollider.center + (capsuleCollider.height/2 - capsuleCollider.radius) * transform.forward,
            capsuleCollider.radius, delta.normalized, hits, delta.magnitude, ~LayerMask.GetMask("Projectile")
        );

        int minIndex = -1;
        for (int i = 0; i < count; i++) {
            Mech mech = hits[i].collider.transform.root.GetComponent<Mech>();
            if (mech == owner) continue;

            if (minIndex == -1 || hits[minIndex].distance > hits[i].distance) {
                minIndex = i;
            }
        }

        // Hit
        if (minIndex != -1) {
            RaycastHit hit = hits[minIndex];

            Destroy(gameObject);
            FindObjectOfType<ParticleManager>().CreateBulletImpact(hit.point, hit.normal);

            Mech mech = hit.collider.GetComponentInParent<Mech>();
            if (mech) mech.GiveDamage(owner, hit.collider, damage);

            return;
        }

        transform.position = transform.position + delta;
    }
}