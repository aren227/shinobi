using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    const float speed = 50f;

    CapsuleCollider capsuleCollider;

    void Awake() {
        capsuleCollider = GetComponent<CapsuleCollider>();
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

            FindObjectOfType<ParticleManager>().CreateMissileExplosion(hit.point);
        }

        transform.position = transform.position + delta;
    }
}
