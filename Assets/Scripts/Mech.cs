using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mech : MonoBehaviour
{
    public static Mech Player;

    Rigidbody rigid;

    public float yaw = 0;

    CapsuleParams kinematicCapsule;

    VelocitySolver velocitySolver;

    public Vector3 velocity;

    Vector3 accumulatedDelta;

    public bool boost = false;
    float boostSince;

    const float minimumBoostTime = 0.3f;

    public const float maxStemina = 100;
    public float stemina = 0;

    const float boostSteminaConsumRate = 10;
    const float hideSteminaConsumRate = 10;
    const float steminaRestoreRate = 5;

    public const float minSteminaRequiredToBoost = 10;
    public const float maxSteminaRequiredToBoost = 100;
    const float minSteminaRequiredToHide = 3;
    const float steminaPenaltyWhenHit = 40;

    public TargetType targetType { get; private set; } = TargetType.VITAL;

    public Vector3 aimTarget { get; private set; }

    public List<Target> targets { get; private set; } = new List<Target>();

    public MechArmature mechArmature;

    public Inventory inventory;

    public Skeleton skeleton;
    public SwordController2 swordController { get; private set; }

    public GameObject model;

    public bool isUsingSword { get; private set; }
    public bool isHided { get; private set; }

    public bool isKilled { get; private set; }

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        skeleton = GetComponent<Skeleton>();
        swordController = GetComponent<SwordController2>();

        velocitySolver = GetComponent<AccelerationBasedVelocitySolver>();

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        kinematicCapsule = new CapsuleParams() { center = capsuleCollider.center, radius = capsuleCollider.radius, height = capsuleCollider.height };
        Destroy(capsuleCollider);

        stemina = maxStemina;

        inventory = new Inventory(this);

        if (GetComponent<PlayerMechController>()) Player = this;

        Aim(transform.position + transform.forward * 10);
    }

    void Start() {
        // foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
        //     GameObject missileWeapon = Instantiate(PrefabRegistry.Instance.missileWeapon);

        //     Equip(missileWeapon.GetComponent<MissileWeapon>(), slot);
        // }
    }

    void Update() {
        if (isHided) {
            stemina = Mathf.Max(stemina - hideSteminaConsumRate * Time.deltaTime, 0);
            if (stemina <= 0) {
                EndHide();
            }
        }

        // Update targets
        // @Todo: Update after move to get up-to-date result?
        targets = GetVisibleTargets();
    }

    public void Move(Vector3 moveDir) {
        if (isKilled) return;

        if (boost) {
            stemina = Mathf.Max(stemina - boostSteminaConsumRate * Time.deltaTime, 0);
            if (stemina <= 0 && Time.time - boostSince > minimumBoostTime) {
                EndBoost();
            }
        }
        else {
            stemina = Mathf.Min(stemina + steminaRestoreRate * Time.deltaTime, maxStemina);
        }

        velocity = velocitySolver.Update(moveDir, boost);

        accumulatedDelta += velocity * Time.deltaTime;
    }

    public bool BeginBoost() {
        int requiredStemina = skeleton.thruster.GetSteminaRequiredToBoost();

        if (stemina < requiredStemina) return false;

        stemina -= requiredStemina;

        boost = true;
        boostSince = Time.time;

        return true;
    }

    public void EndBoost() {
        boost = false;
    }

    public void BeginHide() {
        if (isHided) return;

        if (stemina < minSteminaRequiredToHide) return;

        isHided = true;

        foreach (Part part in skeleton.GetParts()) {
            part.SetHide(true);
        }

        // @Temp: Fixme
        skeleton.hideEffect.transform.localScale = Vector3.one;

        skeleton.hideEffect.Play();
    }

    public void EndHide(bool attacked = false) {
        if (!isHided) return;

        isHided = false;

        foreach (Part part in skeleton.GetParts()) {
            part.SetHide(false);
        }

        skeleton.hideEffect.Play();

        if (attacked) {
            stemina = Mathf.Max(stemina - steminaPenaltyWhenHit, 0);
        }
    }

    public void Aim(Vector3 aimTarget) {
        this.aimTarget = aimTarget;

        skeleton.leftGunPivot.forward = (aimTarget - skeleton.leftGunPivot.position).normalized;
        skeleton.rightGunPivot.forward = (aimTarget - skeleton.rightGunPivot.position).normalized;

        skeleton.headBone.forward = (aimTarget - skeleton.headBone.position).normalized;

        Vector2 dir = new Vector2(aimTarget.x - transform.position.x, aimTarget.z - transform.position.z);
        yaw = -Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
    }

    public List<Target> GetVisibleTargets(int maxCount = 256) {
        List<Target> result = new List<Target>();

        if (targetType == TargetType.NONE) return result;

        Target[] targets = FindObjectsOfType<Target>();

        // @Todo: This is fair but sucks.
        // Should implement custom aim boundary or something.
        Camera cam = Camera.main;

        Vector3 prevPos = cam.transform.localPosition;
        Quaternion prevRot = cam.transform.localRotation;

        if (this != Mech.Player) {
            cam.transform.position = skeleton.headBone.position;
            cam.transform.rotation = skeleton.headBone.rotation;
        }

        RaycastHit[] hits = new RaycastHit[32];

        foreach (Target target in targets) {
            if (target.type != targetType) continue;
            // Ignore targets in myself.
            if (target.transform.IsChildOf(transform)) continue;

            Vector2 viewport = cam.WorldToViewportPoint(target.transform.position);
            if (Vector3.Dot(target.transform.position - cam.transform.position, cam.transform.forward) > 0
                && 0 <= viewport.x && viewport.x <= 1 && 0 <= viewport.y && viewport.y <= 1) {
                const float sphereRadius = 0.2f;
                Vector3 camToTarget = target.transform.position - cam.transform.position;

                int count = Physics.SphereCastNonAlloc(
                    cam.transform.position, sphereRadius, camToTarget.normalized, hits, camToTarget.magnitude, ~LayerMask.GetMask("Missile")
                );

                float distToTarget = float.PositiveInfinity;
                float minDist = float.PositiveInfinity;

                for (int i = 0; i < count; i++) {
                    // Ignore myself.
                    if (hits[i].collider.transform.IsChildOf(transform)) continue;

                    if (hits[i].collider.transform == target.transform || hits[i].collider.transform.IsChildOf(target.transform)) {
                        distToTarget = Mathf.Min(distToTarget, hits[i].distance);
                    }

                    // Vital target should always visible.
                    if (targetType == TargetType.VITAL && hits[i].collider.transform.IsChildOf(target.transform.root)) continue;

                    minDist = Mathf.Min(minDist, hits[i].distance);
                }

                if (distToTarget <= minDist) {
                    result.Add(target);
                }
            }
        }

        if (maxCount < result.Count) {
            List<KeyValuePair<Target, float>> distList = new List<KeyValuePair<Target, float>>();

            foreach (Target target in result) {
                Vector2 viewport = cam.WorldToViewportPoint(target.transform.position);
                distList.Add(new KeyValuePair<Target, float>(target, Vector2.Distance(Vector2.one * 0.5f, viewport)));
            }

            distList.Sort((x, y) => {
                float f = x.Value - y.Value;
                if (f == 0) return 0;
                if (f > 0) return 1;
                return -1;
            });

            result.Clear();
            for (int i = 0; i < maxCount; i++) {
                result.Add(distList[i].Key);
            }
        }

        // Restore camera transform.
        if (this != Mech.Player) {
            cam.transform.localPosition = prevPos;
            cam.transform.localRotation = prevRot;
        }

        return result;
    }

    public void UpdatePivots() {
        // @Todo: Does not consider animation.
        foreach (KeyValuePair<Inventory.Slot, Item> p in inventory.items) {
            p.Value.transform.SetParent(skeleton.GetPivot(p.Key), false);
        }
    }

    public void BeginSword() {
        Item sword = inventory.GetItem(Inventory.Slot.SWORD);

        if (!sword) return;

        isUsingSword = true;

        UpdatePivots();
    }

    public void EndSword() {
        isUsingSword = false;

        UpdatePivots();
    }

    public void BeginSwing() {
        if (isHided) EndHide();

        if (swordController.state == SwordSwingState.IDLE) {
            swordController.BeginSwing();
        }
    }

    public void SwitchHand() {
        swordController.SwitchHand();
    }

    public bool CanUseWeapon(Inventory.Slot slot) {
        Item item = inventory.GetItem(slot);
        if (!item) return false;

        Weapon weapon = item.GetComponent<Weapon>();
        if (!weapon) return false;

        Part part = skeleton.GetPartBySlot(slot);
        if (part.disabled) return false;

        return true;
    }

    public void UseWeapon(Inventory.Slot slot) {
        if (!CanUseWeapon(slot)) return;

        if (isHided) EndHide();

        inventory.GetItem(slot).GetComponent<Weapon>().Shoot(aimTarget, null);
    }

    public void LaunchMissiles() {
        // @Todo: Simple algorithm. Need to be refined.

        List<Weapon> weapons = new List<Weapon>();
        foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
            if (CanUseWeapon(slot)) {
                Weapon weapon = inventory.GetItem(slot).GetComponent<Weapon>();
                if (weapon && weapon.type == WeaponType.MISSLE_WEAPON) {
                    weapons.Add(weapon);
                }
            }
        }

        if (Mathf.Min(weapons.Count, targets.Count) > 0) {
            if (isHided) EndHide();

            weapons.Sort((x, y) => x.ammo - y.ammo);

            for (int i = 0; i < Mathf.Min(weapons.Count, targets.Count); i++) {
                weapons[i].Shoot(Vector3.zero, targets[i].transform);
            }
        }
    }

    public void GiveDamage(Collider collider, int damage) {
        Part part = skeleton.GetPartByCollider(collider);
        if (part) {
            if (isHided) {
                EndHide(attacked: true);
            }

            part.Hit(damage);
        }

        Damagable damagable = collider.GetComponent<Damagable>();
        if (damagable) {
            if (isHided) {
                EndHide(attacked: true);
            }

            damagable.Hit(damage);
        }
    }

    void FixedUpdate() {
        if (isKilled) return;

        Vector3 delta = accumulatedDelta;

        accumulatedDelta = Vector3.zero;

        const float pad = 0.1f;
        for (int i = 0; i < 3; i++) {
            RaycastHit hit;
            if (delta.sqrMagnitude > 1e-5f && Physics.CapsuleCast(
                rigid.position + kinematicCapsule.center - (kinematicCapsule.height/2 + kinematicCapsule.radius) * Vector3.up,
                rigid.position + kinematicCapsule.center + (kinematicCapsule.height/2 - kinematicCapsule.radius) * Vector3.up,
                kinematicCapsule.radius, delta.normalized, out hit, delta.magnitude + pad, LayerMask.GetMask("Kinematic"))) {

                Vector3 safe = delta.normalized * Mathf.Max(hit.distance - pad, 0);
                rigid.MovePosition(rigid.position + safe);

                delta = Vector3.ProjectOnPlane(delta, hit.normal);
            }
            else {
                rigid.MovePosition(rigid.position + delta);

                break;
            }
        }

        // if (moveDir.sqrMagnitude > 0) {
        rigid.MoveRotation(Quaternion.Euler(0, yaw, 0));
        // }
    }

    public bool Equip(Item item, Inventory.Slot slot) {
        if (inventory.SetItem(item, slot)) {
            Transform pivot = skeleton.GetPivot(slot);

            item.transform.parent = pivot;
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;

            return true;
        }
        return false;
    }

    public bool Unequip(Inventory.Slot slot) {
        Item item = inventory.GetItem(slot);
        if (item != null) {
            inventory.SetItem(null, slot);

            item.transform.parent = null;
            item.transform.localRotation = Quaternion.identity;

            return true;
        }
        return false;
    }

    float DistanceToProportion(Transform sliceTarget) {
        float dist = Vector3.Distance(skeleton.GetPart(PartName.BODY).transform.position, sliceTarget.position);

        const float minDist = 1;
        const float maxDist = 10;

        return 1-Mathf.InverseLerp(minDist, maxDist, dist);
    }

    public SwordHitShape GetSwordHitShape(Mech mech) {
        SwordHitShape shape = new SwordHitShape();

        shape.fillRate[0] = DistanceToProportion(mech.skeleton.rightArmSlicePivot) * shape.fillRate[0];
        shape.fillRate[1] = DistanceToProportion(mech.skeleton.rightBodySlicePivot) * shape.fillRate[1];
        shape.fillRate[2] = DistanceToProportion(mech.skeleton.rightLegSlicePivot) * shape.fillRate[2];
        shape.fillRate[3] = DistanceToProportion(mech.skeleton.leftLegSlicePivot) * shape.fillRate[3];
        shape.fillRate[4] = DistanceToProportion(mech.skeleton.leftBodySlicePivot) * shape.fillRate[4];
        shape.fillRate[5] = DistanceToProportion(mech.skeleton.leftArmSlicePivot) * shape.fillRate[5];

        // @Temp
        Vector3 localVelocity = skeleton.headBone.rotation * (mech.velocity - velocity);

        const float velocityOffsetScale = 3;
        shape.offset = localVelocity.x * velocityOffsetScale;

        return shape;
    }

    public void Kill() {
        if (isKilled) return;

        isKilled = true;
        rigid.isKinematic = false;

        Debug.Log("Mech killed.");
    }
}

class CapsuleParams {
    public Vector3 center;
    public float radius;
    public float height;
}