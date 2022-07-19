using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Part : MonoBehaviour
{
    public PartName partName;

    public Skeleton skeleton;
    public Transform frameRoot;
    public Transform armorRoot;
    public List<Collider> frameColliders = new List<Collider>();
    public List<Collider> armorColliders = new List<Collider>();

    public int durability = 100;

    public int health { get; private set; }

    public bool disabled { get; private set; }

    List<KeyValuePair<MeshRenderer, Material>> originalMaterials = new List<KeyValuePair<MeshRenderer, Material>>();

    void Awake() {
        health = durability;
    }

    public void Hit(int damage) {
        if (disabled) return;

        // Can't destroy frame with bullet hit.
        health = Mathf.Max(health - damage, 1);

        if (health < durability/2) {
            armorRoot.gameObject.SetActive(false);
            foreach (Collider collider in armorColliders) {
                collider.enabled = false;
            }
        }
    }

    public void Slice() {
        // @Todo: Do slice animation.
        Disable();
    }

    public void Disable() {
        if (disabled) return;

        health = 0;
        disabled = true;

        if (partName == PartName.BODY) {
            skeleton.GetPart(PartName.HEAD).Disable();
            skeleton.GetPart(PartName.UPPER_LEFT_ARM).Disable();
            skeleton.GetPart(PartName.UPPER_RIGHT_ARM).Disable();
            skeleton.GetPart(PartName.UPPER_LEFT_LEG).Disable();
            skeleton.GetPart(PartName.UPPER_RIGHT_LEG).Disable();
        }
        else if (partName == PartName.UPPER_LEFT_ARM) {
            skeleton.GetPart(PartName.LOWER_LEFT_ARM).Disable();
        }
        else if (partName == PartName.UPPER_RIGHT_ARM) {
            skeleton.GetPart(PartName.LOWER_RIGHT_ARM).Disable();
        }
        else if (partName == PartName.UPPER_LEFT_LEG) {
            skeleton.GetPart(PartName.LOWER_LEFT_LEG).Disable();
        }
        else if (partName == PartName.UPPER_RIGHT_LEG) {
            skeleton.GetPart(PartName.LOWER_RIGHT_LEG).Disable();
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

    void Update() {

    }
}