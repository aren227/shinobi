using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour
{
    public Mech owner { get; private set; }

    public Collider itemHintCollider;

    public string displayName;

    public EquipAt equipAt;

    public bool isEquipped => owner != null;

    public void Equip(Mech owner) {
        if (this.owner != null) return;

        this.owner = owner;

        itemHintCollider.enabled = false;
    }

    public void Unequip() {
        if (this.owner == null) return;

        this.owner = null;

        itemHintCollider.enabled = false;
    }
}

public enum EquipAt {
    HANDHELD,
    AUXILIARY,
}