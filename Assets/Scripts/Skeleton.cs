using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    public Transform leftHandWeaponPivot;
    public Transform rightHandWeaponPivot;
    public Transform leftShoulderWeaponPivot;
    public Transform rightShoulderWeaponPivot;
    public Transform leftArmWeaponPivot;
    public Transform rightArmWeaponPivot;
    public Transform leftLegWeaponPivot;
    public Transform rightLegWeaponPivot;
    public Transform swordBackPivot;
    public Transform swordFrontPivot;
    public Transform weaponLeftBackPivot;
    public Transform weaponRightBackPivot;

    public Transform head;

    Dictionary<Inventory.Slot, Transform> pivots = new Dictionary<Inventory.Slot, Transform>();

    void Awake() {
        pivots.Add(Inventory.Slot.LEFT_HAND, leftHandWeaponPivot);
        pivots.Add(Inventory.Slot.RIGHT_HAND, rightHandWeaponPivot);
        pivots.Add(Inventory.Slot.LEFT_SHOULDER, leftShoulderWeaponPivot);
        pivots.Add(Inventory.Slot.RIGHT_SHOULDER, rightShoulderWeaponPivot);
        pivots.Add(Inventory.Slot.LEFT_ARM, leftArmWeaponPivot);
        pivots.Add(Inventory.Slot.RIGHT_ARM, rightArmWeaponPivot);
        pivots.Add(Inventory.Slot.LEFT_LEG, leftLegWeaponPivot);
        pivots.Add(Inventory.Slot.RIGHT_LEG, rightLegWeaponPivot);
        pivots.Add(Inventory.Slot.SWORD, swordBackPivot);
    }

    public Transform GetPivot(Inventory.Slot slot, bool isUsingSword) {
        if (isUsingSword) {
            if (slot == Inventory.Slot.LEFT_HAND) return weaponLeftBackPivot;
            if (slot == Inventory.Slot.RIGHT_HAND) return weaponRightBackPivot;
            if (slot == Inventory.Slot.SWORD) return swordFrontPivot;
            return pivots[slot];
        }
        else {
            if (slot == Inventory.Slot.LEFT_HAND) return leftHandWeaponPivot;
            if (slot == Inventory.Slot.RIGHT_HAND) return rightHandWeaponPivot;
            if (slot == Inventory.Slot.SWORD) return swordBackPivot;
            return pivots[slot];
        }
    }
}
