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

    public GameObject bulletWeapon;
    public GameObject bullet;
    public GameObject missileWeapon;
    public GameObject missile;
    public GameObject sword;
    public GameObject sliceEffectBox;

    public GameObject mech;

    public GameObject audioSource;

    public GameObject bulletHole;
    public Material bulletHoleMat;
    public Material fresnelMat;

    public Material depthPassMat;
    public Material depthWriteMat;
    public Material depthWrite2Mat;
}
