using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Mech : MonoBehaviour
{
    public Rigidbody rigid { get; private set; }

    public float yaw = 0;

    CapsuleParams kinematicCapsule;

    const float rigidbodyCastPad = 0.1f;

    List<VelocitySolver> velocitySolvers = new List<VelocitySolver>();

    public Vector3 velocity;

    public bool disableMovement = false;

    Vector3 accumulatedDelta;

    public bool boost { get; private set; } = false;
    float boostSince;

    public bool hitByPlayerFlag = false;

    const float minimumBoostTime = 0.3f;

    public const float maxStemina = 100;
    public float stemina = 0;

    const float boostSteminaConsumRate = 10;
    const float hideSteminaConsumRate = 20;
    const float bulletTimeSteminaConsumeRate = 20;
    const float steminaRestoreRate = 10;

    public const float minSteminaRequiredToBoost = 5;
    public const float maxSteminaRequiredToBoost = 50;
    const float minSteminaRequiredToHide = 3;
    const float steminaPenaltyWhenHit = 40;
    const float minSteminaRequiredToBulletTime = 3;


    const float maxMeleeAttackRange = 7;
    public const float meleeAttackDistance = 2.5f;

    public TargetType targetType { get; private set; } = TargetType.VITAL;

    public Vector3 aimTarget { get; private set; }

    public List<Transform> targets { get; private set; } = new List<Transform>();

    public List<Missile> targetedMissiles {get; private set; } = new List<Missile>();

    public Inventory inventory { get; private set; }

    public Skeleton skeleton { get; private set; }
    public SwordController2 swordController { get; private set; }

    public GameObject model;

    public bool isUsingSword { get; private set; }
    public bool isHided { get; private set; }
    public bool isBulletTime { get; private set; }

    public bool isKilled { get; private set; }

    public Vector3 velocityVel;

    public bool manualMovement = false;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        skeleton = GetComponent<Skeleton>();
        swordController = GetComponent<SwordController2>();

        AddVelocitySolver(GetComponent<AccelerationBasedVelocitySolver>());

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        kinematicCapsule = new CapsuleParams() { center = capsuleCollider.center, radius = capsuleCollider.radius, height = capsuleCollider.height };
        // Destroy(capsuleCollider);

        stemina = maxStemina;

        inventory = new Inventory(this);

        Aim(transform.position + transform.forward * 10);

        GameManager.Instance.meches.Add(this);
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
        if (isUsingSword) {
            targets.Clear();
            Mech meleeTarget = GetMeleeTarget();
            // @Todo: This sucks.
            if (meleeTarget) targets.Add(meleeTarget.skeleton.cockpit.transform);
        }
        else {
            targets = GetVisibleTargets();
        }

        for (int i = targetedMissiles.Count-1; i >= 0; i--) {
            if (!targetedMissiles[i]) {
                targetedMissiles.RemoveAt(i);
            }
        }

        // Lose control after sword animation is finished.
        if (isKilled) {
            if (!disableMovement && rigid.isKinematic) {
                rigid.isKinematic = false;
                rigid.useGravity = true;
            }
        }

        if (isBulletTime) {
            stemina = Mathf.Max(stemina - bulletTimeSteminaConsumeRate * Time.unscaledDeltaTime, 0);
            if (stemina <= 0) {
                EndBulletTime();
            }
        }

        if (!boost && !isHided && !isBulletTime) {
            stemina = Mathf.Min(stemina + steminaRestoreRate * Time.deltaTime, maxStemina);
        }
    }

    public void Move(Vector3 moveDir) {
        if (manualMovement) return;

        if (boost) {
            stemina = Mathf.Max(stemina - boostSteminaConsumRate * Time.deltaTime, 0);
            if (stemina <= 0 && Time.time - boostSince > minimumBoostTime) {
                EndBoost();
            }
        }

        float smoothTime;
        Vector3 targetVelocity = velocitySolvers[velocitySolvers.Count-1].UpdateSolver(this, moveDir, boost, out smoothTime);

        velocity = Vector3.SmoothDamp(velocity, targetVelocity, ref velocityVel, smoothTime);

        accumulatedDelta += velocity * Time.deltaTime;
    }

    public void AddVelocitySolver(VelocitySolver velocitySolver) {
        velocitySolvers.Add(velocitySolver);
    }

    public void RemoveVelocitySolver(VelocitySolver velocitySolver) {
        velocitySolvers.Remove(velocitySolver);
    }

    public bool BeginBoost() {
        if (disableMovement) return false;

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
        if (isHided || disableMovement) return;

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

    public void BeginBulletTime() {
        if (isBulletTime || disableMovement) return;

        if (stemina < minSteminaRequiredToBulletTime) return;

        isBulletTime = true;

        DOTween.Kill("timeScale");
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.02f, 0.1f).SetId("timeScale").SetEase(Ease.OutCubic).SetUpdate(true);

    }

    public void EndBulletTime() {
        if (!isBulletTime) return;

        isBulletTime = false;

        DOTween.Kill("timeScale");
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1, 0.1f).SetId("timeScale").SetEase(Ease.OutCubic).SetUpdate(true);
    }

    public void Aim(Vector3 aimTarget) {
        if (swordController.state != SwordSwingState.IDLE) return;

        this.aimTarget = aimTarget;

        skeleton.leftGunPivot.forward = (aimTarget - skeleton.leftGunPivot.position).normalized;
        skeleton.rightGunPivot.forward = (aimTarget - skeleton.rightGunPivot.position).normalized;

        skeleton.headBone.forward = (aimTarget - skeleton.headBone.position).normalized;

        Vector2 dir = new Vector2(aimTarget.x - transform.position.x, aimTarget.z - transform.position.z);
        yaw = -Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
    }

    public List<Transform> GetVisibleTargets(int maxCount = 256) {
        List<Transform> result = new List<Transform>();

        // if (targetType == TargetType.NONE) return result;

        // @Todo: This is fair but sucks.
        // Should implement custom aim boundary or something.
        Camera cam = Camera.main;

        Vector3 prevPos = cam.transform.localPosition;
        Quaternion prevRot = cam.transform.localRotation;

        if (this != GameManager.Instance.player) {
            cam.transform.position = skeleton.headBone.position;
            cam.transform.rotation = skeleton.headBone.rotation;
        }

        RaycastHit[] hits = new RaycastHit[32];

        Vector3 myCockpit = skeleton.cockpit.transform.position;
        foreach (Mech mech in GameManager.Instance.meches) {
            // if (target.type != targetType) continue;

            // Ignore targets in myself.
            // if (target.transform.IsChildOf(transform)) continue;
            if (mech == this) continue;
            if (mech.isKilled) continue;

            Vector3 cockpit = mech.skeleton.cockpit.transform.position;

            Vector2 viewport = cam.WorldToViewportPoint(cockpit);
            if (Vector3.Dot(cockpit - cam.transform.position, cam.transform.forward) > 0
                && 0 <= viewport.x && viewport.x <= 1 && 0 <= viewport.y && viewport.y <= 1) {
                const float sphereRadius = 0.2f;
                Vector3 camToTarget = cockpit - cam.transform.position;

                int count = Physics.SphereCastNonAlloc(
                    cam.transform.position, sphereRadius, camToTarget.normalized, hits, camToTarget.magnitude, ~LayerMask.GetMask("Projectile")
                );

                float distToTarget = float.PositiveInfinity;
                float minDist = float.PositiveInfinity;

                bool failed = false;

                for (int i = 0; i < count; i++) {
                    // Ignore myself.
                    if (hits[i].collider.transform.IsChildOf(transform)) continue;
                    if (hits[i].collider.transform.IsChildOf(mech.transform)) continue;

                    failed = true;
                }

                if (!failed) {
                    result.Add(mech.skeleton.cockpit.transform);
                }
            }
        }

        List<KeyValuePair<Transform, float>> distList = new List<KeyValuePair<Transform, float>>();

        foreach (Transform target in result) {
            Vector2 viewport = cam.WorldToViewportPoint(target.position);
            distList.Add(new KeyValuePair<Transform, float>(target, Vector2.Distance(Vector2.one * 0.5f, viewport)));
        }

        distList.Sort((x, y) => {
            float f = x.Value - y.Value;
            if (f == 0) return 0;
            if (f > 0) return 1;
            return -1;
        });

        result.Clear();
        for (int i = 0; i < distList.Count; i++) {
            result.Add(distList[i].Key);
        }

        // Restore camera transform.
        if (this != GameManager.Instance.player) {
            cam.transform.localPosition = prevPos;
            cam.transform.localRotation = prevRot;
        }

        return result;
    }

    public Vector3 GetMeleeAttackPos(Mech victim) {
        Vector3 dir = (transform.position - victim.transform.position).normalized;
        Vector3 targetPos = victim.transform.position + new Vector3(dir.x, 0, dir.z).normalized * meleeAttackDistance;
        return targetPos;
    }

    public Mech GetMeleeTarget() {
        // @Todo: This is fair but sucks.
        // Should implement custom aim boundary or something.
        Camera cam = Camera.main;

        Vector3 prevPos = cam.transform.localPosition;
        Quaternion prevRot = cam.transform.localRotation;

        if (this != GameManager.Instance.player) {
            cam.transform.position = skeleton.headBone.position;
            cam.transform.rotation = skeleton.headBone.rotation;
        }

        List<Mech> candidates = new List<Mech>();

        RaycastHit[] hits = new RaycastHit[32];
        Vector3 myCockpit = skeleton.cockpit.transform.position;

        foreach (Mech mech in GameManager.Instance.meches) {
            if (mech == this) continue;
            if (mech.isKilled) continue;

            Vector3 cockpit = mech.skeleton.cockpit.transform.position;

            Vector2 viewport = cam.WorldToViewportPoint(cockpit);
            if (Vector3.Dot(cockpit - myCockpit, cam.transform.forward) > 0
                && Vector3.Distance(cockpit, myCockpit) < maxMeleeAttackRange
                && 0 <= viewport.x && viewport.x <= 1 && 0 <= viewport.y && viewport.y <= 1) {
                // Can I move to it?
                Vector3 targetPos = GetMeleeAttackPos(mech);
                Vector3 delta = (targetPos - rigid.position);

                if (!Physics.CapsuleCast(
                    rigid.position + kinematicCapsule.center - (kinematicCapsule.height/2 + kinematicCapsule.radius) * Vector3.up,
                    rigid.position + kinematicCapsule.center + (kinematicCapsule.height/2 - kinematicCapsule.radius) * Vector3.up,
                    kinematicCapsule.radius, delta.normalized, delta.magnitude + rigidbodyCastPad, ~LayerMask.GetMask("Mech"))
                ) {
                    candidates.Add(mech);
                }
            }
        }

        if (candidates.Count == 0) return null;
        if (candidates.Count == 1) return candidates[0];

        Mech bestTarget = null;
        float minDist = float.PositiveInfinity;
        foreach (Mech mech in candidates) {
            Vector2 viewport = cam.WorldToViewportPoint(mech.skeleton.cockpit.transform.position);
            float dist = Vector2.Distance(Vector2.one * 0.5f, viewport);
            if (dist < minDist) {
                minDist = dist;
                bestTarget = mech;
            }
        }

        return bestTarget;
    }

    public void UpdatePivots() {
        // @Todo: Does not consider animation.
        foreach (KeyValuePair<Inventory.Slot, Item> p in inventory.items) {
            p.Value.transform.SetParent(skeleton.GetPivot(p.Key), false);
            p.Value.transform.localPosition = Vector3.zero;
            p.Value.transform.localRotation = Quaternion.identity;

            p.Value.equippedPartName = skeleton.GetPartBySlot(p.Key).partName;
        }
    }

    public void BeginSword() {
        Item sword = inventory.GetItem(Inventory.Slot.SWORD);

        if (!sword) {
            if (this == GameManager.Instance.player) {
                UiManager.Instance.ShowSystemMessage("NO SWORD");
            }
        }

        Part right = skeleton.GetPart(PartName.LOWER_RIGHT_ARM);
        Part left = skeleton.GetPart(PartName.LOWER_LEFT_ARM);

        if (right.disabled && left.disabled) return;

        skeleton.DisableHandIk(false);
        skeleton.DisableHandIk(true);

        isUsingSword = true;

        if (right.disabled) swordController.SetRightHand(false);
        else swordController.SetRightHand(true);

        UpdatePivots();
    }

    public void EndSword() {
        if (swordController.state != SwordSwingState.IDLE) return;

        isUsingSword = false;

        skeleton.DisableHandIk(false);
        skeleton.DisableHandIk(true);

        Part right = skeleton.GetPart(PartName.LOWER_RIGHT_ARM);
        Part left = skeleton.GetPart(PartName.LOWER_LEFT_ARM);

        // Switch hand if possible.
        if (right.disabled && !left.disabled
            && inventory.GetItem(Inventory.Slot.RIGHT_HAND) != null
            && inventory.GetItem(Inventory.Slot.LEFT_HAND) == null
        ) {
            Item item = inventory.GetItem(Inventory.Slot.RIGHT_HAND);
            Unequip(Inventory.Slot.RIGHT_HAND);
            Equip(item, Inventory.Slot.LEFT_HAND);
        }
        if (left.disabled && !right.disabled
            && inventory.GetItem(Inventory.Slot.LEFT_HAND) != null
            && inventory.GetItem(Inventory.Slot.RIGHT_HAND) == null
        ) {
            Item item = inventory.GetItem(Inventory.Slot.LEFT_HAND);
            Unequip(Inventory.Slot.LEFT_HAND);
            Equip(item, Inventory.Slot.RIGHT_HAND);
        }

        UpdatePivots();
    }

    public void BeginMeleeAttack() {
        if (swordController.state != SwordSwingState.IDLE) return;

        // @Todo: Make a dedicated variable for storing melee target.
        if (targets.Count == 0) return;

        if (isHided) EndHide();

        Mech target = targets[0].GetComponentInParent<Mech>();

        swordController.BeginAttack(target);
    }

    // public void BeginSwing() {
    //     if (isHided) EndHide();

    //     if (swordController.state == SwordSwingState.IDLE) {
    //         swordController.BeginSwing();
    //     }
    // }

    public bool CanUseWeapon(Inventory.Slot slot) {
        Item item = inventory.GetItem(slot);
        if (!item) return false;

        Weapon weapon = item.GetComponent<Weapon>();
        if (!weapon) return false;

        if (weapon.ammo <= 0) return false;

        Part part = skeleton.GetPartBySlot(slot);
        if (part.disabled) return false;

        if (slot == Inventory.Slot.LEFT_HAND && skeleton.GetPart(PartName.LOWER_LEFT_ARM).disabled) return false;
        if (slot == Inventory.Slot.RIGHT_HAND && skeleton.GetPart(PartName.LOWER_RIGHT_ARM).disabled) return false;

        return true;
    }

    public void UseWeapon(Inventory.Slot slot, Transform target = null) {
        if (!CanUseWeapon(slot)) return;

        if (isHided) EndHide();

        inventory.GetItem(slot).GetComponent<Weapon>().Shoot(aimTarget, target);
    }

    public void ShootBullets(List<Transform> targets) {
        if (isUsingSword) return;

        Weapon left = null, right = null;
        if (CanUseWeapon(Inventory.Slot.LEFT_HAND)) left = inventory.GetItem(Inventory.Slot.LEFT_HAND)?.GetComponent<Weapon>();
        if (CanUseWeapon(Inventory.Slot.RIGHT_HAND)) right = inventory.GetItem(Inventory.Slot.RIGHT_HAND)?.GetComponent<Weapon>();

        if (left != null && left.type != WeaponType.BULLET_WEAPON) left = null;
        if (right != null && right.type != WeaponType.BULLET_WEAPON) right = null;

        if (left == null && right == null) {
            if (this == GameManager.Instance.player) {
                UiManager.Instance.ShowSystemMessage("NO AMMO");
            }
            return;
        }

        if (isHided) EndHide();

        if (left) skeleton.EnableHandIk(false, false, 3);
        if (right) skeleton.EnableHandIk(true, false, 3);

        if (targets.Count == 0) {
            left?.Shoot(aimTarget);
            right?.Shoot(aimTarget);
            return;
        }

        int index = 0;
        if (right != null) {
            if (index < targets.Count) {
                right.Shoot(targets[index].position);
                index = Mathf.Min(index+1, targets.Count-1);
            }
        }
        if (left != null) {
            if (index < targets.Count) {
                left.Shoot(targets[index].position);
                index = Mathf.Min(index+1, targets.Count-1);
            }
        }
    }

    public List<Weapon> GetMissileWeapons() {
        List<Weapon> weapons = new List<Weapon>();
        foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
            if (CanUseWeapon(slot)) {
                Weapon weapon = inventory.GetItem(slot).GetComponent<Weapon>();
                if (weapon && weapon.type == WeaponType.MISSLE_WEAPON) {
                    weapons.Add(weapon);
                }
            }
        }
        return weapons;
    }

    public void LaunchMissiles(List<Transform> targets) {
        List<Weapon> weapons = GetMissileWeapons();

        // Empty weapons are not included.
        if (weapons.Count == 0) {
            if (this == GameManager.Instance.player) {
                UiManager.Instance.ShowSystemMessage("NO AMMO");
            }
            return;
        }

        if (isHided) EndHide();

        const float missileLaunchDelay = 0.1f;

        if (targets.Count == 0) {
            float delay = 0;
            for (int i = 0; i < weapons.Count; i++) {
                weapons[i].ScheduleLaunch(aimTarget, null, delay);
                delay += missileLaunchDelay;
            }
            return;
        }

        float[] targetDelay = new float[targets.Count];
        for (int i = 0; i < weapons.Count; i++) {
            weapons[i].ScheduleLaunch(Vector3.zero, targets[i % targets.Count], targetDelay[i % targets.Count]);
            targetDelay[i % targets.Count] += missileLaunchDelay;
        }
    }

    public void AddTargetedMissile(Missile missile) {
        targetedMissiles.Add(missile);
    }

    public void GiveDamage(Mech by, Collider collider, int damage) {
        if (by == GameManager.Instance.player) hitByPlayerFlag = true;

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

        Weapon weapon = collider.GetComponent<Weapon>();
        if (weapon) {
            if (isHided) {
                EndHide(attacked: true);
            }

            weapon.Hit(damage);
        }
    }

    void FixedUpdate() {
        if (manualMovement || isKilled) return;

        Vector3 delta = accumulatedDelta;

        accumulatedDelta = Vector3.zero;

        const float pad = 0.1f;
        for (int i = 0; i < 3; i++) {
            RaycastHit hit;
            Vector3 origin = rigid.position - delta.normalized * pad;
            if (delta.sqrMagnitude > 1e-5f && Physics.CapsuleCast(
                origin + kinematicCapsule.center - (kinematicCapsule.height/2 + kinematicCapsule.radius) * Vector3.up,
                origin + kinematicCapsule.center + (kinematicCapsule.height/2 - kinematicCapsule.radius) * Vector3.up,
                kinematicCapsule.radius, delta.normalized, out hit, delta.magnitude + pad,
                LayerMask.GetMask("Kinematic", "Mech", "Ground", "Objective"))
            ) {

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

    public bool TryToEquip(Item item) {
        if (item.equipAt == EquipAt.HANDHELD) {
            Part left = skeleton.GetPart(PartName.LOWER_LEFT_ARM);
            Part right = skeleton.GetPart(PartName.LOWER_RIGHT_ARM);

            if (!right.disabled && inventory.GetItem(Inventory.Slot.RIGHT_HAND) == null) {
                Equip(item, Inventory.Slot.RIGHT_HAND);
                return true;
            }
            if (!left.disabled && inventory.GetItem(Inventory.Slot.LEFT_HAND) == null) {
                Equip(item, Inventory.Slot.LEFT_HAND);
                return true;
            }

            return false;
        }
        else if (item.equipAt == EquipAt.AUXILIARY) {
            Inventory.Slot[] slots = new Inventory.Slot[] {
                Inventory.Slot.LEFT_SHOULDER, Inventory.Slot.RIGHT_SHOULDER,
                Inventory.Slot.LEFT_ARM, Inventory.Slot.RIGHT_ARM,
                Inventory.Slot.LEFT_LEG, Inventory.Slot.RIGHT_LEG,
            };

            foreach (Inventory.Slot slot in slots) {
                Part part = skeleton.GetPartBySlot(slot);
                if (part.disabled || inventory.GetItem(slot) != null) continue;

                Equip(item, slot);

                return true;
            }

            return false;
        }
        else if (item.equipAt == EquipAt.SWORD) {
            if (inventory.GetItem(Inventory.Slot.SWORD) != null) {
                Unequip(Inventory.Slot.SWORD);
            }
            Equip(item, Inventory.Slot.SWORD);

            return true;
        }
        return false;
    }

    public bool Equip(Item item, Inventory.Slot slot) {
        Transform pivot = skeleton.GetPivot(slot);
        Part part = skeleton.GetPartBySlot(slot);

        if (inventory.SetItem(item, slot, part)) {
            item.transform.parent = pivot;
            item.transform.localPosition = Vector3.zero;
            item.transform.localRotation = Quaternion.identity;

            Weapon weapon = item.GetComponent<Weapon>();
            if (weapon) inventory.BalanceAmmo(weapon.type);

            item.GetComponent<Rigidbody>().isKinematic = true;

            return true;
        }
        return false;
    }

    public bool Unequip(Inventory.Slot slot) {
        Item item = inventory.GetItem(slot);
        if (item != null) {
            inventory.SetItem(null, slot, null);

            item.transform.parent = null;

            item.GetComponent<Rigidbody>().isKinematic = false;

            return true;
        }
        return false;
    }

    public void UpdateSkeleton() {
        Part lowerLeft = skeleton.GetPart(PartName.LOWER_LEFT_ARM);
        Part lowerRight = skeleton.GetPart(PartName.LOWER_RIGHT_ARM);

        // Drop items first.
        foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
            Item item = inventory.GetItem(slot);
            if (item == null) continue;

            Part part = skeleton.GetPart(item.equippedPartName);
            if (part.disabled) {
                Unequip(slot);
            }
        }

        if (isUsingSword) {
            if (lowerRight.disabled && swordController.isRightHanded) EndSword();
            else if (lowerLeft.disabled && !swordController.isRightHanded) EndSword();
        }
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

        Debug.Log("Mech killed.");

        if (this != GameManager.Instance.player) {
            // @Todo: Display later if using sword.
            // UiManager.Instance.ShowSystemMessage("ENEMY DESTROYED");
        }
    }
}

class CapsuleParams {
    public Vector3 center;
    public float radius;
    public float height;
}