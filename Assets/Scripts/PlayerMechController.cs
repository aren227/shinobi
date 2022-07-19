using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMechController : MonoBehaviour
{
    Mech mech;

    CameraController cameraController;
    // SwordController swordController;
    SwordController2 swordController2;
    UiManager uiManager;

    void Awake() {
        cameraController = FindObjectOfType<CameraController>();
        // swordController = FindObjectOfType<SwordController>();
        uiManager = FindObjectOfType<UiManager>();

        mech = GetComponent<Mech>();
        swordController2 = GetComponent<SwordController2>();
    }

    bool IsInteractable() {
        return !FindObjectOfType<InventoryCanvas2>().open;
    }

    void Update() {
        // @Temp
        if (!IsInteractable()) return;

        Vector3[] dirs = new Vector3[] {
            Vector3.forward, Vector3.back, Vector3.left, Vector3.right, Vector3.up, Vector3.down
        };
        KeyCode[] keys = new KeyCode[] {
            KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Space, KeyCode.LeftControl
        };

        Vector3 moveDir = Vector3.zero;
        for (int i = 0; i < 6; i++) {
            if (Input.GetKey(keys[i])) {
                moveDir += dirs[i];
            }
        }
        moveDir.Normalize();

        moveDir = cameraController.GetCameraRotation() * moveDir;

        if (moveDir.sqrMagnitude > 0 && Input.GetKeyDown(KeyCode.LeftShift) && !mech.boost) {
            mech.BeginBoost();
        }
        else if (!Input.GetKey(KeyCode.LeftShift) && mech.boost) {
            mech.EndBoost();
        }

        mech.Move(moveDir);

        Vector3 aimTarget = cameraController.cameraTarget.position + cameraController.cameraTarget.forward * 1000;

        RaycastHit aimHit;
        if (Physics.Raycast(cameraController.cameraTarget.position, cameraController.cameraTarget.forward, out aimHit, 1000)) {
            aimTarget = aimHit.point;
        }

        mech.Aim(aimTarget);

        mech.yaw = cameraController.cameraArm.eulerAngles.y;

        // if (Input.GetKeyDown(KeyCode.Q)) {
        //     if (!mech.isUsingSword) mech.BeginSword();
        //     else mech.EndSword();
        // }

        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            if (!mech.isUsingSword) mech.BeginSword();
            else mech.EndSword();
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            if (!mech.isHided) mech.BeginHide();
            else mech.EndHide();
        }

        // @Todo: Better ui.
        if (Input.GetKeyDown(KeyCode.E)) {
            Item[] items = FindObjectsOfType<Item>();
            Item selected = null;
            float minDist = 3;
            foreach (Item item in items) {
                if (item.isEquipped) continue;
                if (minDist > Vector3.Distance(item.transform.position, mech.transform.position)) {
                    selected = item;
                    minDist = Vector3.Distance(item.transform.position, mech.transform.position);
                }
            }

            if (selected != null) TryToEquip(selected);
        }

        if (mech.isUsingSword) {
            // mech.targetType = TargetType.VITAL;

            // List<Target> targets;

            // targets = mech.GetVisibleTargets(maxCount: 1);

            // if (Input.GetMouseButtonDown(1)) {
            //     if (targets.Count > 0 && Input.GetMouseButtonDown(1)) {
            //         cameraController.locked = targets[0].transform;
            //     }
            // }
            // else if (Input.GetMouseButton(1)) {
            //     targets.Clear();
            //     targets.Add(cameraController.locked.GetComponent<Target>());
            // }
            // else if (Input.GetMouseButtonUp(1)) {
            //     cameraController.locked = null;
            // }

            // if (targets.Count > 0) swordController.target = targets[0].GetComponentInParent<Mech>();
            // else swordController.target = null;

            // uiManager.SetTargets(targets, cameraController.cam);

            // Newer version
            if (swordController2.state == SwordSwingState.IDLE && Input.GetMouseButtonDown(0)) {
                mech.BeginMeleeAttack();
            }
            if (Input.GetMouseButtonDown(1)) {
                mech.SwitchHand();
            }
        }
        else {
            if (Input.GetMouseButton(0)) {
                mech.UseWeapon(Inventory.Slot.LEFT_HAND);
                mech.UseWeapon(Inventory.Slot.RIGHT_HAND);
            }
            if (Input.GetMouseButtonDown(1)) {
                mech.LaunchMissiles();
            }
        }

        uiManager.SetTargets(mech.targets, cameraController.cam);
        uiManager.SetStemina(Mech.maxStemina, mech.stemina, mech.skeleton.thruster.GetSteminaRequiredToBoost());
        uiManager.SetSpeed(mech.velocity.magnitude);
    }

    public void TryToEquip(Item item) {
        Inventory inventory = mech.inventory;
        if (item.equipAt == EquipAt.HANDHELD) {
            if (!mech.Equip(item, Inventory.Slot.LEFT_HAND)) {
                if (!mech.Equip(item, Inventory.Slot.RIGHT_HAND)) {
                    FindObjectOfType<InventoryCanvas2>().Open(item);
                }
            }
        }
        else if (item.equipAt == EquipAt.AUXILIARY) {
            FindObjectOfType<InventoryCanvas2>().Open(item);
        }
        else if (item.equipAt == EquipAt.SWORD) {
            if (inventory.GetItem(Inventory.Slot.SWORD) != null) {
                mech.Unequip(Inventory.Slot.SWORD);
            }
            mech.Equip(item, Inventory.Slot.SWORD);
        }
    }
}
