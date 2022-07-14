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

    bool boost = false;

    public const float maxStemina = 100;
    public float stemina = 0;

    const float steminaConsumRate = 10;
    const float steminaRestoreRate = 5;
    const float steminaRequiredToBoost = 10;

    public TargetType targetType;

    public Vector3 aimTarget;

    public MechArmature mechArmature;

    public Inventory inventory;

    public Skeleton skeleton;

    public GameObject model;

    public bool isUsingSword { get; private set; }

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        skeleton = GetComponent<Skeleton>();

        velocitySolver = FindObjectOfType<AccelerationBasedVelocitySolver>();

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

    public void Move(Vector3 moveDir) {
        if (boost) {
            stemina = Mathf.Max(stemina - steminaConsumRate * Time.deltaTime, 0);

            bool boostInput = Input.GetKey(KeyCode.LeftShift) && moveDir.sqrMagnitude > 0;

            if (!boostInput || stemina <= 0) {
                boost = false;
            }
        }
        else {
            stemina = Mathf.Min(stemina + steminaRestoreRate * Time.deltaTime, maxStemina);
            if (Input.GetKeyDown(KeyCode.LeftShift) && moveDir.sqrMagnitude > 0 && stemina >= steminaRequiredToBoost) {
                stemina -= steminaRequiredToBoost;
                boost = true;
            }
        }

        velocity = velocitySolver.Update(moveDir, boost);

        accumulatedDelta += velocity * Time.deltaTime;
    }

    public void Aim(Vector3 aimTarget) {
        this.aimTarget = aimTarget;

        skeleton.leftHandWeaponPivot.forward = (aimTarget - skeleton.leftHandWeaponPivot.position).normalized;
        skeleton.rightHandWeaponPivot.forward = (aimTarget - skeleton.rightHandWeaponPivot.position).normalized;

        skeleton.head.forward = (aimTarget - skeleton.head.position).normalized;

        Vector2 dir = new Vector2(aimTarget.x - transform.position.x, aimTarget.z - transform.position.z);
        yaw = -Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90;
    }

    public List<Target> GetVisibleTargets() {
        List<Target> result = new List<Target>();

        if (targetType == TargetType.NONE) return result;

        Target[] targets = FindObjectsOfType<Target>();

        // @Todo: This is fair but sucks.
        // Should implement custom aim boundary or something.
        Camera cam = Camera.main;

        Vector3 prevPos = cam.transform.localPosition;
        Quaternion prevRot = cam.transform.localRotation;

        if (this != Mech.Player) {
            cam.transform.position = skeleton.head.position;
            cam.transform.rotation = skeleton.head.rotation;
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

                bool success = true;
                for (int i = 0; i < count; i++) {
                    // Ignore myself.
                    if (hits[i].collider.transform.IsChildOf(transform)) continue;
                    // Ignore target collider.
                    if (hits[i].collider.transform.IsChildOf(target.transform)) continue;

                    success = false;
                    break;
                }

                if (success) {
                    result.Add(target);
                }
            }
        }

        // Restore camera transform.
        if (this != Mech.Player) {
            cam.transform.localPosition = prevPos;
            cam.transform.localRotation = prevRot;
        }

        return result;
    }

    void Update() {
        if (this != Mech.Player) {
            Aim(Mech.Player.transform.position);
        }
    }

    public void BeginSword() {
        Item sword = inventory.GetItem(Inventory.Slot.SWORD);

        if (!sword) return;

        isUsingSword = true;

        // Update pivots.
        // @Todo: Does not consider animation.
        foreach (KeyValuePair<Inventory.Slot, Item> p in inventory.items) {
            p.Value.transform.SetParent(skeleton.GetPivot(p.Key, isUsingSword), false);
        }
    }

    public void EndSword() {
        isUsingSword = false;

        foreach (KeyValuePair<Inventory.Slot, Item> p in inventory.items) {
            p.Value.transform.SetParent(skeleton.GetPivot(p.Key, isUsingSword), false);
        }
    }

    void FixedUpdate() {
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
            Transform pivot = skeleton.GetPivot(slot, isUsingSword);

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
}

class CapsuleParams {
    public Vector3 center;
    public float radius;
    public float height;
}