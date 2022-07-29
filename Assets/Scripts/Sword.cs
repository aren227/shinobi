using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    public CapsuleCollider coll;

    void Awake() {
        coll.enabled = false;
    }
}
