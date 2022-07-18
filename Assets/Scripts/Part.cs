using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour
{
    public Skeleton skeleton;
    public Transform frameRoot;
    public Transform armorRoot;
    public List<Collider> frameColliders = new List<Collider>();
    public List<Collider> armorColliders = new List<Collider>();

    public int durability = 100;

    public int health { get; private set; }

    void Awake() {
        health = durability;
    }

    public void Hit(Vector3 pos, Vector3 normal, int damage) {
        health = Mathf.Max(health - damage, 0);

        if (health < durability/2) {
            armorRoot.gameObject.SetActive(false);
            foreach (Collider collider in armorColliders) {
                collider.enabled = false;
            }
        }
        if (health <= 0) {
            frameRoot.gameObject.SetActive(false);
            foreach (Collider collider in frameColliders) {
                collider.enabled = false;
            }
        }
    }

    void Update() {

    }
}
