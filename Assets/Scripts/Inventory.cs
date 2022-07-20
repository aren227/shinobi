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

    public bool SetItem(Item item, Slot slot, Part part) {
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

        item.Equip(owner, part.partName);

        return true;
    }

    public List<Item> GetItems() {
        return new List<Item>(items.Values);
    }

    void BalanceAmmo(List<Weapon> weapons) {
        int totalAmmo = 0;
        foreach (Weapon weapon in weapons) {
            totalAmmo += weapon.ammo;
        }

        if (totalAmmo <= 0) return;

        for (int i = 0; i < weapons.Count; i++) {
            weapons[i].ammo = totalAmmo / weapons.Count;
        }

        totalAmmo -= totalAmmo / weapons.Count;

        for (int i = 0; i < weapons.Count; i++) {
            if (totalAmmo > 0) {
                weapons[i].ammo += 1;
                totalAmmo--;
            }
        }
    }

    public List<Weapon> GetWeapons(WeaponType weaponType) {
        List<Weapon> res = new List<Weapon>();
        foreach (Item item in items.Values) {
            Weapon weapon = item.GetComponent<Weapon>();
            if (weapon && weapon.type == weaponType) {
                res.Add(weapon);
            }
        }
        return res;
    }

    public void BalanceAmmo(WeaponType weaponType) {
        BalanceAmmo(GetWeapons(weaponType));
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