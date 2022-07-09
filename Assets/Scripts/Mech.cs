using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mech : MonoBehaviour
{
    Rigidbody rigid;

    void Awake() {
        rigid = GetComponent<Rigidbody>();
    }

    void Move(Vector3 dir) {

    }
}
