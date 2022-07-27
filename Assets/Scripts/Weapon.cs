using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public WeaponType type;
    public int ammo;
    public const float delay = 0.05f;

    const float ammoLossPerDamage = 0.2f;

    public Transform point;

    float lastShootTime;

    List<TargetDelay> launchQueue = new List<TargetDelay>();

    public bool Shoot(Vector3 aimTarget, Transform targetTransform = null) {
        Item item = GetComponent<Item>();
        if (item == null || !item.isEquipped) return false;

        Mech mech = item.owner;

        if (ammo <= 0 || Time.time - lastShootTime < delay) return false;

        if (type == WeaponType.BULLET_WEAPON) {
            GameObject obj = PoolManager.Instance.Spawn("bullet");

            const float pushForward = 4f;

            Vector3 dir = (aimTarget - point.position).normalized;

            obj.transform.position = point.position + dir * pushForward;
            obj.transform.forward = dir;

            obj.GetComponent<Bullet>().owner = mech;

            SoundBank.Instance.PlaySound("bullet_shoot", transform.position, 0.3f);
        }
        else if (type == WeaponType.MISSLE_WEAPON) {
            GameObject obj = PoolManager.Instance.Spawn("missile");

            const float pushForward = 1f;

            obj.transform.position = point.position + point.forward * pushForward;
            obj.transform.forward = point.forward;

            Missile missile = obj.GetComponent<Missile>();

            missile.owner = mech;

            if (targetTransform != null) {
                missile.target = targetTransform;

                Mech victim = targetTransform.root.GetComponent<Mech>();
                if (victim) {
                    victim.AddTargetedMissile(missile);
                }
            }
            else {
                obj.transform.forward = (aimTarget - obj.transform.position).normalized;
            }
        }

        lastShootTime = Time.time;
        if (ammo > 0) ammo--;

        return true;
    }

    public void Hit(int damage) {
        int ammoLoss = Mathf.RoundToInt(damage * ammoLossPerDamage);
        ammo = Mathf.Max(ammo - ammoLoss, 0);
    }

    void Update() {
        for (int i = launchQueue.Count-1; i >= 0; i--) {
            if (launchQueue[i].delay <= 0) {
                Shoot(launchQueue[i].aimTarget, launchQueue[i].target);
                launchQueue.RemoveAt(i);
            }
            else {
                launchQueue[i] = new TargetDelay() {
                    target = launchQueue[i].target,
                    aimTarget = launchQueue[i].aimTarget,
                    delay = launchQueue[i].delay - Time.deltaTime,
                };
            }
        }
    }

    public void ScheduleLaunch(Vector3 aimTarget, Transform target, float delay) {
        launchQueue.Add(new TargetDelay() {
            target = target,
            aimTarget = aimTarget,
            delay = delay,
        });
    }
}

struct TargetDelay {
    public Transform target;
    public Vector3 aimTarget;
    public float delay;
}

public enum WeaponType {
    BULLET_WEAPON,
    MISSLE_WEAPON,
}