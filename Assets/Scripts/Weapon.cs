using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponType type;
    public int ammo;
    public float delay = 0.1f;

    public Transform point;

    float lastShootTime;

    public bool Shoot(Vector3 aimTarget, Transform targetTransform = null) {
        Item item = GetComponent<Item>();
        if (item == null || !item.isEquipped) return false;

        Mech mech = item.owner;

        if (ammo <= 0 || Time.time - lastShootTime < delay) return false;

        if (type == WeaponType.BULLET_WEAPON) {
            GameObject obj = GameObject.Instantiate(GameObject.FindObjectOfType<ParticleManager>().bullet);

            const float pushForward = 8f;

            obj.transform.position = point.position + point.forward * pushForward;
            obj.transform.forward = (aimTarget - obj.transform.position).normalized;
        }
        else if (type == WeaponType.MISSLE_WEAPON) {
            GameObject obj = GameObject.Instantiate(PrefabRegistry.Instance.missile);

            const float pushForward = 1f;

            obj.transform.position = point.position + point.forward * pushForward;
            obj.transform.forward = point.forward;

            if (targetTransform != null) {
                obj.GetComponent<Missile>().target = targetTransform;
            }
            else {
                obj.transform.forward = (aimTarget - obj.transform.position).normalized;
            }
        }

        lastShootTime = Time.time;
        if (ammo > 0) ammo--;

        return true;
    }
}

public enum WeaponType {
    BULLET_WEAPON,
    MISSLE_WEAPON,
}