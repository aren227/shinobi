using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public Mech owner;

    public Dictionary<Slot, Item> items = new Dictionary<Slot, Item>();

    public Inventory(Mech owner) {
        this.owner = owner;
    }

    public Item GetItem(Slot slot) {
        if (items.ContainsKey(slot)) return items[slot];
        return null;
    }

    public bool SetItem(Item item, Slot slot) {
        // Remove
        if (item == null) {
            if (items.ContainsKey(slot)) {
                item = items[slot];

                items.Remove(slot);

                item.Unequip();

                return true;
            }
            return false;
        }

        if (items.ContainsKey(slot)) return false;

        items[slot] = item;

        item.Equip(owner);

        return true;
    }

    public List<Item> GetItems() {
        return new List<Item>(items.Values);
    }

    public enum Slot {
        LEFT_HAND,
        RIGHT_HAND,
        LEFT_SHOULDER,
        RIGHT_SHOULDER,
        LEFT_ARM,
        RIGHT_ARM,
        LEFT_LEG,
        RIGHT_LEG,
        SWORD,
    }
}