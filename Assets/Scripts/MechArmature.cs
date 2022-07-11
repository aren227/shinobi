using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MechArmature : MonoBehaviour
{
    public List<Transform> bones = new List<Transform>();

    void Awake() {
        AddBoneRecursive(transform);
    }

    void AddBoneRecursive(Transform bone) {
        bones.Add(bone);
        for (int i = 0; i < bone.childCount; i++) {
            AddBoneRecursive(bone.GetChild(i));
        }
    }

    public void Blend(List<KeyValuePair<MechArmature, float>> keys) {
        float s = 0;
        foreach (KeyValuePair<MechArmature, float> p in keys) {
            s += p.Value;
        }

        for (int i = 0; i < bones.Count; i++) {
            Vector3 pos = Vector3.zero;
            Quaternion rot = Quaternion.identity;

            float sum = 0;
            foreach (KeyValuePair<MechArmature, float> p in keys) {
                float prop = p.Value / s;

                Matrix4x4 mat = p.Key.transform.worldToLocalMatrix * p.Key.bones[i].localToWorldMatrix;
                Vector3 localPos = mat.GetColumn(3);
                Quaternion localRot = Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));

                // Vector3 localPos = p.Key.transform.worldToLocalMatrix * new Vector4(p.Key.bones[i].position.x, p.Key.bones[i].position.y, p.Key.bones[i].position.z, 1);
                // Quaternion localRot = Quaternion.Inverse(p.Key.transform.rotation) * p.Key.bones[i].rotation;

                pos += localPos * prop;

                if (sum == 0) {
                    rot = localRot;
                }
                else {
                    rot = Quaternion.Slerp(localRot, rot, sum / (sum + prop));
                }

                sum += prop;
            }

            bones[i].position = transform.localToWorldMatrix * new Vector4(pos.x, pos.y, pos.z, 1);
            bones[i].rotation = transform.rotation * rot;
        }
    }
}
