using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    const float speed = 600f;
    const float lifetime = 10;
    const int damage = 5;

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
            capsuleCollider.radius, delta.normalized, hits, delta.magnitude, ~LayerMask.GetMask("Mech", "Projectile"), QueryTriggerInteraction.Collide
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
            ParticleManager.Instance.CreateBulletImpact(hit.point, hit.normal);

            Mech mech = hit.collider.GetComponentInParent<Mech>();
            bool metalHit = false;
            if (mech) {
                mech.GiveDamage(owner, hit.collider, damage);
                metalHit = true;
            }

            Spaceship spaceship = hit.collider.GetComponent<Spaceship>();
            if (spaceship) {
                spaceship.Hit(damage);
                metalHit = true;
            }

            if (metalHit) {
                SoundBank.Instance.PlaySound("bullet_hit", hit.point, 0.3f);
            }
            else {
                SoundBank.Instance.PlaySound("bullet_hit_ground", hit.point, 0.1f);
            }

            return;
        }

        transform.position = transform.position + delta;
    }
}