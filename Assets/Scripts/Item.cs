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

    public void Equip(Mech owner, PartName part) {
        if (this.owner != null) return;

        this.owner = owner;
        this.equippedPartName = part;
    }

    public void Unequip() {
        if (this.owner == null) return;

        this.owner = null;
    }
}

public enum EquipAt {
    HANDHELD,
    AUXILIARY,
    SWORD,
}