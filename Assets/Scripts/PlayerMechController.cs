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

        if (Input.GetKeyDown(KeyCode.Tab)) {
            if (mech.targetType == TargetType.THERMAL) mech.targetType = TargetType.VITAL;
            else mech.targetType = TargetType.THERMAL;
        }

        List<Target> targets = GetVisibleTargets();

        if (Input.GetMouseButtonDown(1)) {
            // @Todo: Simple algorithm. Need to be refined.
            List<MissileWeapon> weapons = new List<MissileWeapon>();
            foreach (Item item in mech.inventory.GetItems()) {
                if (item is MissileWeapon) weapons.Add(item as MissileWeapon);
            }

            weapons.Sort((x, y) => x.ammo - y.ammo);

            for (int i = 0; i < Mathf.Min(weapons.Count, targets.Count); i++) {
                weapons[i].Launch(targets[i].transform);
            }
        }

        mech.yaw = cameraController.cameraArm.eulerAngles.y;

        uiManager.SetStemina(mech.stemina);
        uiManager.SetSpeed(mech.velocity.magnitude);
        uiManager.SetTargets(targets, cameraController.cam);
    }

    // @Todo: This should be move to Mech.cs.
    public List<Target> GetVisibleTargets() {
        Target[] targets = FindObjectsOfType<Target>();
        List<Target> result = new List<Target>();

        Camera cam = cameraController.cam;

        RaycastHit[] hits = new RaycastHit[32];

        foreach (Target target in targets) {
            if (target.type != mech.targetType) continue;
            if (target.transform.IsChildOf(transform)) continue;

            Vector2 viewport = cam.WorldToViewportPoint(target.transform.position);
            if (0 <= viewport.x && viewport.x <= 1 && 0 <= viewport.y && viewport.y <= 1) {
                const float sphereRadius = 0.5f;
                Vector3 camToTarget = target.transform.position - cam.transform.position;

                int count = Physics.SphereCastNonAlloc(
                    cam.transform.position, sphereRadius, camToTarget.normalized, hits, camToTarget.magnitude, ~LayerMask.GetMask("Missile")
                );

                bool success = true;
                for (int i = 0; i < count; i++) {
                    if (hits[i].collider.transform.IsChildOf(transform)) continue;
                    if (hits[i].collider.gameObject == target.gameObject) continue;

                    success = false;
                    break;
                }

                if (success) {
                    result.Add(target);
                }
            }
        }

        return result;
    }
}
