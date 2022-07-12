using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory
{
    public Dictionary<Slot, Item> items = new Dictionary<Slot, Item>();

    public Weapon left, right;
    public Weapon sword;

    public bool isUsingSword;

    public Item GetItem(Slot slot) {
        if (items.ContainsKey(slot)) return items[slot];
        return null;
    }

    public bool SetItem(Item item, Slot slot) {
        // Remove
        if (item == null) {
            items.Remove(slot);
            return true;
        }

        if (slot == Slot.LEFT_SHOULDER || slot == Slot.RIGHT_SHOULDER) {
            if (item is Shield) return false;
        }

        if (items.ContainsKey(slot)) return false;

        items[slot] = item;
        return true;
    }

    public enum Slot {
        LEFT_SHOULDER,
        RIGHT_SHOULDER,
        LEFT_ARM,
        RIGHT_ARM,
        LEFT_LEG,
        RIGHT_LEG,
    }
}