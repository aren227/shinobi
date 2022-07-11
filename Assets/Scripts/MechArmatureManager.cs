using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechArmatureManager : MonoBehaviour
{
    public GameObject idlePrefab;
    public GameObject forwardPrefab;
    public GameObject backPrefab;
    public GameObject rightPrefab;

    public MechArmature idle;
    public MechArmature forward;
    public MechArmature back;
    public MechArmature left;
    public MechArmature right;

    public Mech mech;
    public MechArmature mechArmature;

    void Awake() {
        idle = Instantiate(idlePrefab).GetComponent<MechArmature>();
        forward = Instantiate(forwardPrefab).GetComponent<MechArmature>();
        back = Instantiate(backPrefab).GetComponent<MechArmature>();
        left = Instantiate(rightPrefab).GetComponent<MechArmature>();
        right = Instantiate(rightPrefab).GetComponent<MechArmature>();

        // @Hack
        left.bones[0].localEulerAngles = new Vector3(left.bones[0].localEulerAngles.x, -left.bones[0].localEulerAngles.y, left.bones[0].localEulerAngles.z);
        left.bones[0].localScale = new Vector3(-left.bones[0].localScale.x, left.bones[0].localScale.y, left.bones[0].localScale.z);

        idle.gameObject.SetActive(false);
        forward.gameObject.SetActive(false);
        back.gameObject.SetActive(false);
        left.gameObject.SetActive(false);
        right.gameObject.SetActive(false);

        mech = FindObjectOfType<Mech>();
        mechArmature = mech.mechArmature;
    }

    void Update() {
        Vector3 vel = Quaternion.Inverse(mech.transform.rotation) * mech.velocity;

        List<KeyValuePair<MechArmature, float>> keys = new List<KeyValuePair<MechArmature, float>>();

        float idleness = 0, t;

        const float speedRate = 3;

        t = Mathf.Clamp01(Mathf.Max(Vector3.Dot(Vector3.forward, vel), 0) / speedRate);
        idleness += 1-t;

        keys.Add(new KeyValuePair<MechArmature, float>(forward, t));

        t = Mathf.Clamp01(Mathf.Max(Vector3.Dot(Vector3.back, vel), 0) / speedRate);
        idleness += 1-t;

        keys.Add(new KeyValuePair<MechArmature, float>(back, t));

        t = Mathf.Clamp01(Mathf.Max(Vector3.Dot(Vector3.left, vel), 0) / speedRate);
        idleness += 1-t;

        keys.Add(new KeyValuePair<MechArmature, float>(left, t));

        t = Mathf.Clamp01(Mathf.Max(Vector3.Dot(Vector3.right, vel), 0) / speedRate);
        idleness += 1-t;

        keys.Add(new KeyValuePair<MechArmature, float>(right, t));

        keys.Add(new KeyValuePair<MechArmature, float>(idle, idleness / 4));

        mechArmature.Blend(keys);
    }
}
