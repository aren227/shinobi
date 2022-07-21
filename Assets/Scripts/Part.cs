using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour
{
    public PartName partName;

    Mech mech;

    public Skeleton skeleton;
    public Transform frameRoot;
    public Transform armorRoot;
    public List<Collider> frameColliders = new List<Collider>();
    public List<Collider> armorColliders = new List<Collider>();

    public int armorDurability = 200;
    public int frameDurability = 200;

    bool armorDetached = false;
    bool frameDetached = false;

    public int health { get; private set; }

    public bool disabled { get; private set; }

    public bool hided { get; private set; }

    List<KeyValuePair<MeshRenderer, Material>> originalMaterials = new List<KeyValuePair<MeshRenderer, Material>>();

    UiManager uiManager;

    void Awake() {
        health = armorDurability + frameDurability;
        uiManager = FindObjectOfType<UiManager>();

        mech = GetComponentInParent<Mech>();
    }

    public void Hit(int damage) {
        if (disabled) return;

        health = Mathf.Max(health - damage, 0);

        if (health <= 0) {
            if (!frameDetached) {
                frameDetached = true;

                GameObject newFrameRoot = new GameObject("Frame Debris");

                frameRoot.parent = newFrameRoot.transform;

                foreach (Collider collider in frameColliders) {
                    collider.gameObject.layer = LayerMask.NameToLayer("Debris");
                    collider.transform.parent = newFrameRoot.transform;
                }

                Rigidbody rigidbody = newFrameRoot.AddComponent<Rigidbody>();
                rigidbody.useGravity = true;

                // rigidbody.velocity = Random.insideUnitSphere;
                // rigidbody.angularVelocity = Random.insideUnitSphere;

                Disable(newFrameRoot);

                mech.UpdateSkeleton();
            }
        }
        if (health < frameDurability) {
            if (!armorDetached) {
                armorDetached = true;

                GameObject newArmorRoot = new GameObject("Armor Debris");

                armorRoot.parent = newArmorRoot.transform;

                foreach (Collider collider in armorColliders) {
                    collider.gameObject.layer = LayerMask.NameToLayer("Debris");
                    collider.transform.parent = newArmorRoot.transform;
                }

                Rigidbody rigidbody = newArmorRoot.AddComponent<Rigidbody>();
                rigidbody.useGravity = true;

                // rigidbody.velocity = Random.insideUnitSphere;
                // rigidbody.angularVelocity = Random.insideUnitSphere;
            }
        }

        if (mech == Mech.Player && partName == PartName.BODY) {
            if (health > frameDurability) uiManager.SetCockpitHealth(1);
            else uiManager.SetCockpitHealth((float)health / frameDurability);
        }
    }

    public bool IsCritical() {
        // @Todo: Arbitrary threshold.
        return health < (frameDurability / 2);
    }

    public void Disable(GameObject newParent) {
        if (disabled) return;

        health = 0;
        disabled = true;

        if (hided) SetHide(false);

        if (newParent) {
            if (!frameDetached) {
                frameDetached = true;

                frameRoot.parent = newParent.transform;

                foreach (Collider collider in frameColliders) {
                    collider.gameObject.layer = LayerMask.NameToLayer("Debris");
                    collider.transform.parent = newParent.transform;
                }
            }

            if (!armorDetached) {
                armorDetached = true;

                armorRoot.parent = newParent.transform;

                foreach (Collider collider in armorColliders) {
                    collider.gameObject.layer = LayerMask.NameToLayer("Debris");
                    collider.transform.parent = newParent.transform;
                }
            }
        }

        if (partName == PartName.BODY) {
            skeleton.GetPart(PartName.HEAD).Disable(newParent);
            skeleton.GetPart(PartName.UPPER_LEFT_ARM).Disable(newParent);
            skeleton.GetPart(PartName.UPPER_RIGHT_ARM).Disable(newParent);
            skeleton.GetPart(PartName.UPPER_LEFT_LEG).Disable(newParent);
            skeleton.GetPart(PartName.UPPER_RIGHT_LEG).Disable(newParent);
        }
        else if (partName == PartName.UPPER_LEFT_ARM) {
            skeleton.GetPart(PartName.LOWER_LEFT_ARM).Disable(newParent);
        }
        else if (partName == PartName.UPPER_RIGHT_ARM) {
            skeleton.GetPart(PartName.LOWER_RIGHT_ARM).Disable(newParent);
        }
        else if (partName == PartName.UPPER_LEFT_LEG) {
            skeleton.GetPart(PartName.LOWER_LEFT_LEG).Disable(newParent);
        }
        else if (partName == PartName.UPPER_RIGHT_LEG) {
            skeleton.GetPart(PartName.LOWER_RIGHT_LEG).Disable(newParent);
        }

        foreach (Collider collider in frameColliders) {
            collider.gameObject.layer = LayerMask.NameToLayer("Debris");
        }
        foreach (Collider collider in armorColliders) {
            collider.gameObject.layer = LayerMask.NameToLayer("Debris");
        }

        if (partName == PartName.BODY) {
            skeleton.mech.Kill();
        }
    }

    public void SetHide(bool hide) {
        if (disabled) return;

        this.hided = hide;

        if (hide) {
            originalMaterials.Clear();
            foreach (MeshRenderer meshRenderer in frameRoot.GetComponentsInChildren<MeshRenderer>()) {
                originalMaterials.Add(new KeyValuePair<MeshRenderer, Material>(meshRenderer, meshRenderer.sharedMaterial));

                meshRenderer.sharedMaterial = PrefabRegistry.Instance.fresnelMat;
            }
            foreach (MeshRenderer meshRenderer in armorRoot.GetComponentsInChildren<MeshRenderer>()) {
                originalMaterials.Add(new KeyValuePair<MeshRenderer, Material>(meshRenderer, meshRenderer.sharedMaterial));

                meshRenderer.sharedMaterial = PrefabRegistry.Instance.fresnelMat;
            }

            foreach (SurfaceRenderer surfaceRenderer in frameRoot.GetComponentsInChildren<SurfaceRenderer>()) {
                surfaceRenderer.SetDisabled(true);
            }
            foreach (SurfaceRenderer surfaceRenderer in armorRoot.GetComponentsInChildren<SurfaceRenderer>()) {
                surfaceRenderer.SetDisabled(true);
            }
        }
        else {
            foreach (KeyValuePair<MeshRenderer, Material> p in originalMaterials) {
                p.Key.sharedMaterial = p.Value;
            }
            originalMaterials.Clear();

            foreach (SurfaceRenderer surfaceRenderer in frameRoot.GetComponentsInChildren<SurfaceRenderer>()) {
                surfaceRenderer.SetDisabled(false);
            }
            foreach (SurfaceRenderer surfaceRenderer in armorRoot.GetComponentsInChildren<SurfaceRenderer>()) {
                surfaceRenderer.SetDisabled(false);
            }
        }
    }

    public void SetHoleCube(MeshFilter cubeMeshFilter) {
        foreach (SurfaceRenderer surfaceRenderer in frameRoot.GetComponentsInChildren<SurfaceRenderer>()) {
            surfaceRenderer.cubeMeshFilter = cubeMeshFilter;
        }
        foreach (SurfaceRenderer surfaceRenderer in armorRoot.GetComponentsInChildren<SurfaceRenderer>()) {
            surfaceRenderer.cubeMeshFilter = cubeMeshFilter;
        }
    }

    void Update() {

    }
}
