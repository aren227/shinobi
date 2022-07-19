using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    const float speed = 600f;

    const float lifetime = 10;

    float spawnTimestamp;

    CapsuleCollider capsuleCollider;

    void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();

        spawnTimestamp = Time.time;
    }

    void Update() {
        Vector3 velocity = transform.forward * speed;

        Vector3 delta = velocity * Time.deltaTime;

        RaycastHit hit;
        if (Physics.CapsuleCast(
            transform.position + capsuleCollider.center - (capsuleCollider.height/2 + capsuleCollider.radius) * transform.forward,
            transform.position + capsuleCollider.center + (capsuleCollider.height/2 - capsuleCollider.radius) * transform.forward,
            capsuleCollider.radius, delta.normalized, out hit, delta.magnitude
        )) {
            Destroy(gameObject);
            FindObjectOfType<ParticleManager>().CreateBulletImpact(hit.point, hit.normal);

            // @Todo: Generalize to damagable.

            Damagable damagable = hit.collider.GetComponent<Damagable>();

            if (damagable) {
                damagable.Hit(hit.point, hit.normal, 10);
            }

            // @Todo: We can search mech by collider.
            Mech mech = hit.collider.GetComponentInParent<Mech>();
            if (mech) {
                Part part = mech.skeleton.GetPartByCollider(hit.collider);
                if (part) {
                    part.Hit(hit.point, hit.normal, 10);
                }
            }

            Thruster thruster = hit.collider.GetComponent<Thruster>();
            if (thruster) thruster.Hit(10);

            return;
        }

        transform.position = transform.position + delta;
    }
}