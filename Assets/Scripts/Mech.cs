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

    public Transform leftWeaponPivot;
    public Transform rightWeaponPivot;

    public Vector3 velocity;

    Vector3 accumulatedDelta;

    bool boost = false;

    public const float maxStemina = 500;
    public float stemina = 0;

    const float steminaConsumRate = 10;
    const float steminaRestoreRate = 5;
    const float steminaRequiredToBoost = 10;

    public TargetType targetType;

    public MechArmature mechArmature;

    public Inventory inventory;

    public Skeleton skeleton;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
        skeleton = GetComponent<Skeleton>();

        velocitySolver = FindObjectOfType<AccelerationBasedVelocitySolver>();

        CapsuleCollider capsuleCollider = GetComponent<CapsuleCollider>();
        kinematicCapsule = new CapsuleParams() { center = capsuleCollider.center, radius = capsuleCollider.radius, height = capsuleCollider.height };
        Destroy(capsuleCollider);

        stemina = maxStemina;

        inventory = new Inventory(this);

        // @Temp
        yaw = Random.Range(0f, 360f);

        if (GetComponent<PlayerMechController>()) Player = this;
    }

    void Start() {
        foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
            GameObject missileWeapon = Instantiate(PrefabRegistry.Instance.missileWeapon);

            SetAuxiliary(missileWeapon.GetComponent<MissileWeapon>(), slot);
        }
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

    public void Look(Vector3 forward) {
        leftWeaponPivot.forward = forward;
        rightWeaponPivot.forward = forward;
    }

    void Update() {

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

    // @Todo: Generalize to other auxiliary weapons (or shield).
    void SetAuxiliary(MissileWeapon weapon, Inventory.Slot slot) {
        if (inventory.SetItem(weapon, slot)) {
            Transform pivot = skeleton.GetAuxiliaryPivot(slot);

            weapon.transform.SetParent(pivot, false);
            weapon.transform.localPosition = Vector3.zero;
            weapon.transform.localRotation = Quaternion.identity;
        }
    }
}

class CapsuleParams {
    public Vector3 center;
    public float radius;
    public float height;
}