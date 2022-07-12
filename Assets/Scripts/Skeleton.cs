using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    public Transform leftShoulderWeaponPivot;
    public Transform rightShoulderWeaponPivot;
    public Transform leftArmWeaponPivot;
    public Transform rightArmWeaponPivot;
    public Transform leftLegWeaponPivot;
    public Transform rightLegWeaponPivot;

    Dictionary<Inventory.Slot, Transform> pivots = new Dictionary<Inventory.Slot, Transform>();

    void Awake() {
        pivots.Add(Inventory.Slot.LEFT_SHOULDER, leftShoulderWeaponPivot);
        pivots.Add(Inventory.Slot.RIGHT_SHOULDER, rightShoulderWeaponPivot);
        pivots.Add(Inventory.Slot.LEFT_ARM, leftArmWeaponPivot);
        pivots.Add(Inventory.Slot.RIGHT_ARM, rightArmWeaponPivot);
        pivots.Add(Inventory.Slot.LEFT_LEG, leftLegWeaponPivot);
        pivots.Add(Inventory.Slot.RIGHT_LEG, rightLegWeaponPivot);
    }

    public Transform GetAuxiliaryPivot(Inventory.Slot slot) {
        return pivots[slot];
    }
}
