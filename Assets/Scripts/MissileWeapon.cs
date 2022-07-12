using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileWeapon : MonoBehaviour, Item
{
    public Transform point;

    public Mech owner { get; set; }

    public int ammo = 100;

    void Awake() {
        // @Temp
        ammo = Random.Range(50, 500);
    }

    public bool Launch(Transform target) {
        if (ammo > 0) {
            GameObject obj = GameObject.Instantiate(PrefabRegistry.Instance.missile);

            const float pushForward = 1f;

            obj.transform.position = point.position + point.forward * pushForward;
            obj.transform.forward = point.forward;

            obj.GetComponent<Missile>().target = target.transform;

            ammo--;
            return true;
        }
        return false;
    }
}
