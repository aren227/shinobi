using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMechController : MonoBehaviour
{
    Mech mech;

    CameraController cameraController;
    UiManager uiManager;

    void Awake() {
        cameraController = FindObjectOfType<CameraController>();
        uiManager = FindObjectOfType<UiManager>();

        mech = GetComponent<Mech>();
    }

    void Start() {
        uiManager.SetMaxStemina(Mech.maxStemina);
    }

    void Update() {
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

        mech.Move(moveDir);

        if (Input.GetKeyDown(KeyCode.Alpha1)) mech.targetType = TargetType.VITAL;
        if (Input.GetKeyDown(KeyCode.Alpha2)) mech.targetType = TargetType.THERMAL;

        Vector3 aimTarget = cameraController.cameraTarget.position + cameraController.cameraTarget.forward * 1000;

        RaycastHit aimHit;
        if (Physics.Raycast(cameraController.cameraTarget.position, cameraController.cameraTarget.forward, out aimHit, 1000)) {
            aimTarget = aimHit.point;
        }

        mech.Aim(aimTarget);

        List<Target> targets = mech.GetVisibleTargets();

        if (Input.GetMouseButton(0)) {
            Weapon leftHand = mech.inventory.GetItem(Inventory.Slot.LEFT_HAND)?.GetComponent<Weapon>();
            Weapon rightHand = mech.inventory.GetItem(Inventory.Slot.RIGHT_HAND)?.GetComponent<Weapon>();

            if (leftHand) leftHand.Shoot(mech.aimTarget);
            if (rightHand) rightHand.Shoot(mech.aimTarget);
        }
        if (Input.GetMouseButtonDown(1)) {
            // @Todo: Simple algorithm. Need to be refined.

            List<Weapon> weapons = new List<Weapon>();
            foreach (Item item in mech.inventory.GetItems()) {
                Weapon weapon = item.GetComponent<Weapon>();
                if (weapon && weapon.type == WeaponType.MISSLE_WEAPON) {
                    weapons.Add(weapon);
                }
            }

            weapons.Sort((x, y) => x.ammo - y.ammo);

            for (int i = 0; i < Mathf.Min(weapons.Count, targets.Count); i++) {
                weapons[i].Shoot(Vector3.zero, targets[i].transform);
            }
        }

        mech.yaw = cameraController.cameraArm.eulerAngles.y;

        uiManager.SetStemina(mech.stemina);
        uiManager.SetSpeed(mech.velocity.magnitude);
        uiManager.SetTargets(targets, cameraController.cam);
    }
}
