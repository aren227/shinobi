using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMechController : MonoBehaviour
{
    Mech mech;

    Vector3 targetPos;

    float lastShoot;
    float delay;

    void Awake() {
        mech = GetComponent<Mech>();

        targetPos = Random.insideUnitSphere * 100;
    }

    void Start() {
        foreach (Inventory.Slot slot in new Inventory.Slot[] {
            Inventory.Slot.LEFT_SHOULDER
        }) {
            GameObject missileWeapon = Instantiate(PrefabRegistry.Instance.missileWeapon);

            mech.Equip(missileWeapon.GetComponent<Item>(), slot);
        }
    }

    void Update() {
        // mech.Move((targetPos - mech.transform.position).normalized);

        if (!Mech.Player.isHided) mech.Aim(Mech.Player.transform.position);

        if (Vector3.Distance(mech.transform.position, targetPos) < 3) {
            targetPos = Random.insideUnitSphere * 100;
        }

        if (Time.time - lastShoot > delay) {
            // mech.LaunchMissiles();

            lastShoot = Time.time;
            delay = Random.Range(0f, 2f);
        }
    }
}
