using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerMechController : MonoBehaviour
{
    Mech mech;

    CameraController cameraController;
    // SwordController swordController;
    SwordController2 swordController2;
    UiManager uiManager;

    Vector2 cursorPos;

    List<Transform> markers = new List<Transform>();

    bool isPaused = false;

    void Awake() {
        cameraController = FindObjectOfType<CameraController>();
        // swordController = FindObjectOfType<SwordController>();
        uiManager = FindObjectOfType<UiManager>();

        mech = GetComponent<Mech>();
        swordController2 = GetComponent<SwordController2>();
    }

    void Start() {
        foreach (Inventory.Slot slot in new Inventory.Slot[] {
            Inventory.Slot.RIGHT_HAND,
            Inventory.Slot.LEFT_HAND,
        }) {
            GameObject bulletWeapon = Instantiate(PrefabRegistry.Instance.bulletWeapon);

            bulletWeapon.GetComponent<Weapon>().ammo = 1000;

            mech.Equip(bulletWeapon.GetComponent<Item>(), slot);
        }
        foreach (Inventory.Slot slot in new Inventory.Slot[] {
            Inventory.Slot.LEFT_SHOULDER,
            Inventory.Slot.RIGHT_SHOULDER,
            Inventory.Slot.LEFT_ARM,
            Inventory.Slot.RIGHT_ARM,
            Inventory.Slot.LEFT_LEG,
            Inventory.Slot.RIGHT_LEG,
        }) {
            GameObject missileWeapon = Instantiate(PrefabRegistry.Instance.missileWeapon);

            missileWeapon.GetComponent<Weapon>().ammo = 300;

            mech.Equip(missileWeapon.GetComponent<Item>(), slot);
        }
        {
            GameObject sword = Instantiate(PrefabRegistry.Instance.sword);

            mech.Equip(sword.GetComponent<Item>(), Inventory.Slot.SWORD);
        }
    }

    bool IsInteractable() {
        return !FindObjectOfType<InventoryCanvas2>().open;
    }

    void Update() {
        if (GameManager.Instance.isPaused) return;

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
        else if (!Input.GetKey(KeyCode.LeftShift) && mech.boost && !mech.isFollowing) {
            mech.EndBoost();
        }

        mech.Move(moveDir);

        Vector3 aimTarget = cameraController.cameraTarget.position + cameraController.cameraTarget.forward * 1000;

        RaycastHit aimHit;
        if (Physics.Raycast(cameraController.cameraTarget.position, cameraController.cameraTarget.forward, out aimHit, 1000, ~LayerMask.GetMask("Mech"), QueryTriggerInteraction.Ignore)) {
            aimTarget = aimHit.point;
        }

        if (mech.swordController.state == SwordSwingState.IDLE) {
            // @Todo: Better mech rotation.
            // mech.yaw = cameraController.yaw;

            mech.Aim(aimTarget);
        }

        if (mech.isBulletTime) {
            const float mouseSensitivity = 10f;
            cursorPos += new Vector2(Input.GetAxis("Mouse X") / Screen.width * mouseSensitivity, Input.GetAxis("Mouse Y") / Screen.height * mouseSensitivity);
            cursorPos.x = Mathf.Clamp01(cursorPos.x);
            cursorPos.y = Mathf.Clamp01(cursorPos.y);
        }
        else {
            cursorPos = Vector2.one * 0.5f;
        }

        // if (Input.GetKeyDown(KeyCode.Q)) {
        //     if (!mech.isUsingSword) mech.BeginSword();
        //     else mech.EndSword();
        // }

        if (Input.GetAxis("Mouse ScrollWheel") != 0) {
            if (!mech.isUsingSword) mech.BeginSword();
            else mech.EndSword();
        }

        if (Input.GetKey(KeyCode.E) && !mech.isBulletTime) {
            mech.BeginBulletTime();
            markers.Clear();
        }
        if (!Input.GetKey(KeyCode.E) && mech.isBulletTime) {
            mech.EndBulletTime();

            List<Weapon> weapons = mech.GetMissileWeapons();
            for (int i = 0; i < Mathf.Min(weapons.Count, markers.Count); i++) {
                weapons[i].Shoot(Vector3.zero, markers[i]);
            }
        }

        if (Input.GetKeyDown(KeyCode.Q)) {
            if (!mech.isHided) mech.BeginHide();
            else mech.EndHide();
        }

        Interactable interactable = Interactable.GetInteractable(mech.skeleton.cockpit.transform.position);
        uiManager.SetInteractable(interactable);
        // @Todo: Better ui.
        if (Input.GetKeyDown(KeyCode.F)) {
            if (interactable) {
                interactable.Interact();
            }
        }

        // if (Input.GetKeyDown(KeyCode.F)) {
        //     Item[] items = FindObjectsOfType<Item>();
        //     Item selected = null;
        //     float minDist = 7;
        //     foreach (Item item in items) {
        //         if (item.isEquipped) continue;
        //         if (minDist > Vector3.Distance(item.transform.position, mech.transform.position)) {
        //             selected = item;
        //             minDist = Vector3.Distance(item.transform.position, mech.transform.position);
        //         }
        //     }

        //     if (selected != null) {
        //         if (mech.TryToEquip(selected)) {
        //             SoundBank.Instance.PlaySound("ammo_pickup", mech.skeleton.cockpit.transform.position, 0.5f);
        //         }
        //     }
        // }

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

            if (swordController2.state == SwordSwingState.IDLE && !mech.isFollowing && Input.GetMouseButton(1)) {
                mech.BeginFollowing();
            }
            if (mech.isFollowing && !Input.GetMouseButton(1)) {
                mech.RequestEndFollowing();
            }
        }
        else if (!mech.isBulletTime) {
            if (Input.GetMouseButton(0)) {
                mech.ShootBullets(mech.targets);
            }
            if (Input.GetMouseButtonDown(1)) {
                mech.LaunchMissiles(mech.targets);
            }
        }
        else {
            if (Input.GetMouseButtonDown(0)) {
                List<Weapon> weapons = mech.GetMissileWeapons();
                if (markers.Count < weapons.Count) {
                    // Place a marker.
                    Ray ray = cameraController.cam.ViewportPointToRay(cursorPos);
                    RaycastHit hit;
                    if (Physics.Raycast(ray, out hit, float.PositiveInfinity, LayerMask.GetMask("Armor", "Frame", "Thruster", "Weapon"), QueryTriggerInteraction.Collide)) {
                        Mech targetMech = hit.collider.GetComponentInParent<Mech>();
                        if (targetMech != mech) {
                            GameObject marker = new GameObject("Marker");
                            marker.transform.parent = hit.collider.transform;
                            marker.transform.position = hit.point;

                            markers.Add(marker.transform);
                        }
                    }
                }
                else {
                    // Fail
                }
            }
        }

        if (mech.isBulletTime) {
            uiManager.SetTargets(markers, cameraController.cam);
        }
        else {
            uiManager.SetTargets(mech.targets, cameraController.cam);
        }

        uiManager.SetTargetedMissiles(mech.targetedMissiles, cameraController.cam);

        uiManager.SetStemina(Mech.maxStemina, mech.stemina, mech.skeleton.thruster.GetSteminaRequiredToBoost());
        uiManager.SetSpeed(mech.velocity.magnitude);
        uiManager.SetCrosshairPos(cursorPos);

        uiManager.SetShowAmmo(!mech.isUsingSword);
        if (!mech.isUsingSword) {
            List<Weapon> bulletWeapons = mech.inventory.GetWeapons(WeaponType.BULLET_WEAPON);
            List<Weapon> missileWeapons = mech.inventory.GetWeapons(WeaponType.MISSLE_WEAPON);

            int bullet = 0, missile = 0;
            foreach (Weapon weapon in bulletWeapons) bullet += weapon.ammo;
            foreach (Weapon weapon in missileWeapons) missile += weapon.ammo;

            uiManager.SetBulletAmmo(bullet);
            uiManager.SetBulletWeaponCount(bulletWeapons.Count);
            uiManager.SetMissileAmmo(missile);
            uiManager.SetMissileWeaponCount(missileWeapons.Count);
        }

        Part part = mech.skeleton.GetPart(PartName.BODY);
        {
            if (part.health > part.frameDurability) uiManager.SetCockpitHealth(1);
            else uiManager.SetCockpitHealth((float)part.health / part.frameDurability);
        }

        // Part body = mech.skeleton.GetPart(PartName.BODY);
        // if (body && body.IsCritical()) {
        //     uiManager.ShowSystemMessage("CRITICAL DAMAGE");
        // }

        // for (int i = 0; i < 10; i++) {
        //     if (Input.GetKeyDown(KeyCode.Alpha0 + i)) {
        //         Part p = mech.skeleton.GetPart((PartName)i);
        //         p.Hit(1000);
        //     }
        // }
    }
}
