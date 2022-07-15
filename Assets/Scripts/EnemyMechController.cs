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
        foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
            GameObject missileWeapon = Instantiate(PrefabRegistry.Instance.missileWeapon);

            mech.Equip(missileWeapon.GetComponent<Item>(), slot);
        }
    }

    void Update() {
        mech.Move((targetPos - mech.transform.position).normalized);
        mech.Aim(Mech.Player.transform.position);

        if (Vector3.Distance(mech.transform.position, targetPos) < 3) {
            targetPos = Random.insideUnitSphere * 100;
        }

        if (Time.time - lastShoot > delay) {
            foreach (Item item in mech.inventory.GetItems()) {
                Weapon weapon = item.GetComponent<Weapon>();
                if (weapon) {
                    weapon.Shoot(Vector3.zero, Mech.Player.transform);
                }
            }

            lastShoot = Time.time;
            delay = Random.Range(0f, 3f);
        }
    }
}
