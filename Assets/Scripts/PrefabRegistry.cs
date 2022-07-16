using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabRegistry : MonoBehaviour
{
    public static PrefabRegistry Instance;

    public GameObject missileWeapon;
    public GameObject missile;
    public GameObject sliceEffectBox;

    void Awake() {
        Instance = this;
    }
}
