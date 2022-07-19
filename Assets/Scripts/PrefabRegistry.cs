using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabRegistry : MonoBehaviour
{
    public static PrefabRegistry Instance {
        get {
            if (_instance == null) {
                _instance = FindObjectOfType<PrefabRegistry>();
            }
            return _instance;
        }
    }

    static PrefabRegistry _instance;

    public GameObject missileWeapon;
    public GameObject missile;
    public GameObject sliceEffectBox;

    public GameObject bulletHole;
    public Material bulletHoleMat;
    public Material fresnelMat;
}
