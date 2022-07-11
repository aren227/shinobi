using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MissileWeapon : MonoBehaviour
{
    public Transform point;

    Mech owner;

    ParticleManager particleManager;

    public void SetOwner(Mech mech) {
        owner = mech;
    }

    void Awake() {
        particleManager = FindObjectOfType<ParticleManager>();
    }

    void Update() {
        Transform camera = owner.cameraController.cameraTarget;

        Vector3 cameraPoint = camera.position;
        Vector3 direction = camera.forward;

        if (Input.GetMouseButtonDown(1)) {
            GameObject obj = GameObject.Instantiate(GameObject.FindObjectOfType<ParticleManager>().missile);

            const float pushForward = 1f;

            obj.transform.position = point.position + point.forward * pushForward;
            obj.transform.forward = camera.forward;
        }
    }
}
