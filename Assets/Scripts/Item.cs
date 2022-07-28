using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Mech owner { get; private set; }

    public PartName equippedPartName;

    public GameObject model;

    public string displayName;

    public EquipAt equipAt;

    public bool isEquipped => owner != null;

    Interactable interactable;
    Weapon weapon;

    void Awake() {
        interactable = GetComponent<Interactable>();
        weapon = GetComponent<Weapon>();

        interactable.subText = "[F] Equip";

        interactable.onInteract.AddListener(OnInteract);
    }

    public void Equip(Mech owner, PartName part) {
        if (this.owner != null) return;

        this.owner = owner;
        this.equippedPartName = part;

        interactable.enabled = false;
    }

    public void Unequip() {
        if (this.owner == null) return;

        this.owner = null;

        interactable.enabled = true;
    }

    void OnInteract() {
        GameManager.Instance.player.TryToEquip(this);
    }

    void Update() {
        // @Todo: We don't even need to update this text if it is not displayed.
        if (interactable.enabled) {
            // @Todo: This sucks.
            if (weapon && weapon.type == WeaponType.BULLET_WEAPON) {
                interactable.mainText = $"Bullet x {weapon.ammo}";
            }
            else if (weapon && weapon.type == WeaponType.MISSLE_WEAPON) {
                interactable.mainText = $"Missile x {weapon.ammo}";
            }
            else if (equipAt == EquipAt.SWORD) {
                interactable.mainText = $"Sword";
            }
        }
    }
}

public enum EquipAt {
    HANDHELD,
    AUXILIARY,
    SWORD,
}