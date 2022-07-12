using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletWeapon : MonoBehaviour
{
    const float period = 0.15f;
    float lastShoot;

    public Transform point;
    public Transform handle;

    Mech owner;

    ParticleManager particleManager;

    public void SetOwner(Mech mech) {
        owner = mech;
    }

    void Awake() {
        particleManager = FindObjectOfType<ParticleManager>();
    }

    void Update() {
        // Transform camera = owner.cameraController.cameraTarget;

        // Vector3 cameraPoint = camera.position;
        // Vector3 direction = camera.forward;

        // if (Input.GetMouseButton(0) && Time.time - lastShoot >= period) {
        //     lastShoot = Time.time;

        //     GameObject obj = GameObject.Instantiate(GameObject.FindObjectOfType<ParticleManager>().bullet);

        //     const float pushForward = 8f;

        //     obj.transform.position = point.position + point.forward * pushForward;
        //     obj.transform.forward = owner.cameraController.cameraTarget.forward;
        // }
    }
}
