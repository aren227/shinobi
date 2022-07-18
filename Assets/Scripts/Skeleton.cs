using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : MonoBehaviour
{
    Mech mech;

    public Transform pivotRoot;
    public Transform boneRoot;
    public Transform modelRoot; // Frame
    public Transform modelRoot2; // Armor
    public Transform colliderRoot;

    public Transform leftHandPivot;
    public Transform rightHandPivot;
    public Transform leftShoulderPivot;
    public Transform rightShoulderPivot;
    public Transform leftArmPivot;
    public Transform rightArmPivot;
    public Transform leftLegPivot;
    public Transform rightLegPivot;
    public Transform leftBackPivot;
    public Transform middleBackPivot;
    public Transform rightBackPivot;

    // For weapon aiming animation.
    public Transform leftGunPivot;
    public Transform rightGunPivot;

    public Transform headBone;

    public Transform cockpit;

    public Transform leftArmSlicePivot;
    public Transform rightArmSlicePivot;
    public Transform leftBodySlicePivot;
    public Transform rightBodySlicePivot;
    public Transform leftLegSlicePivot;
    public Transform rightLegSlicePivot;

    Animator animator;

    Dictionary<Inventory.Slot, Transform> pivots = new Dictionary<Inventory.Slot, Transform>();

    List<SurfaceRenderer>[] surfaceRenderersByBone;

    GameObject sliceBox;

    string[] boneNames = new string[] {
        "Bone", "Head", "UArm.L", "LArm.L", "UArm.R", "LArm.R",
        "ULeg.L", "LLeg.L", "ULeg.R", "LLeg.R"
    };

    string[] modelNames = new string[] {
        "Body", "Head", "Arm_R_Up", "Arm_R_Down", "Arm_L_Up", "Arm_L_Down",
        "Leg_R_Up", "Leg_R_Down", "Leg_L_Up", "Leg_L_Down"
    };

    void Awake() {
        mech = GetComponent<Mech>();

        animator = boneRoot.GetComponent<Animator>();

        pivots.Add(Inventory.Slot.LEFT_HAND, leftHandPivot);
        pivots.Add(Inventory.Slot.RIGHT_HAND, rightHandPivot);
        pivots.Add(Inventory.Slot.LEFT_SHOULDER, leftShoulderPivot);
        pivots.Add(Inventory.Slot.RIGHT_SHOULDER, rightShoulderPivot);
        pivots.Add(Inventory.Slot.LEFT_ARM, leftArmPivot);
        pivots.Add(Inventory.Slot.RIGHT_ARM, rightArmPivot);
        pivots.Add(Inventory.Slot.LEFT_LEG, leftLegPivot);
        pivots.Add(Inventory.Slot.RIGHT_LEG, rightLegPivot);
        pivots.Add(Inventory.Slot.SWORD, middleBackPivot);

        // Force apply.
        animator.Play("Armature|Rest");
        animator.Update(0);

        AttachToBone();

        // @Hardcoded
        animator.Play("Move Blend Tree");
    }

    void Update() {
        Vector3 localVelocity = Quaternion.Inverse(mech.transform.rotation) * mech.velocity;
        Vector2 motion = new Vector2(localVelocity.x, localVelocity.z) / 10f;

        if (motion.magnitude > 1) motion = motion.normalized;

        animator.SetFloat("X", motion.x);
        animator.SetFloat("Y", motion.y);

        // SetBodySlice(new Vector3(1, 1, 0).normalized, Mathf.InverseLerp(-1, 1, Mathf.Sin(Time.time)));
    }

    void FindRecursive(Transform current, Transform[] array, string[] names) {
        for (int i = 0; i < current.childCount; i++) {
            Transform child = current.GetChild(i);

            for (int j = 0; j < names.Length; j++) {
                if (names[j] == child.name) {
                    array[j] = child;
                    break;
                }
            }

            FindRecursive(child, array, names);
        }
    }

    void AttachToBone() {
        Debug.Assert(boneNames.Length == modelNames.Length);

        Transform[] bones = new Transform[boneNames.Length];
        Transform[] models = new Transform[modelNames.Length];
        Transform[] models2 = new Transform[modelNames.Length];
        Transform[] colliders = new Transform[modelNames.Length];

        FindRecursive(boneRoot, bones, boneNames);
        FindRecursive(modelRoot, models, modelNames);
        FindRecursive(modelRoot2, models2, modelNames);
        FindRecursive(colliderRoot, colliders, boneNames);

        for (int i = 0; i < boneNames.Length; i++) {
            if (bones[i] == null) Debug.LogError($"Bone {boneNames[i]} not found!");
            if (models[i] == null) Debug.LogError($"Model {modelNames[i]} not found!");
            if (models2[i] == null) Debug.LogError($"Model2 {modelNames[i]} not found!");
            if (colliders[i] == null) Debug.LogError($"Collider {boneNames[i]} not found!");
        }

        // Assume that bones are in rest pose.

        // Attach model
        // We need another root objects because pivots of model is arbitrary.
        GameObject[] modelRoots = new GameObject[modelNames.Length];
        for (int i = 0; i < modelNames.Length; i++) {
            modelRoots[i] = new GameObject("Model");
            modelRoots[i].transform.parent = bones[i];

            Matrix4x4 modelToBone = bones[i].worldToLocalMatrix * models[i].localToWorldMatrix;
            Matrix4x4 boneToModel = modelToBone.inverse;

            modelRoots[i].transform.localPosition = boneToModel.ExtractPosition();
            modelRoots[i].transform.localRotation = boneToModel.ExtractRotation();
            modelRoots[i].transform.localScale = boneToModel.ExtractScale();

            models[i].transform.parent = modelRoots[i].transform;
        }

        // Attach armor
        GameObject[] modelRoots2 = new GameObject[modelNames.Length];
        for (int i = 0; i < modelNames.Length; i++) {
            modelRoots2[i] = new GameObject("Model2");
            modelRoots2[i].transform.parent = bones[i];

            Matrix4x4 modelToBone = bones[i].worldToLocalMatrix * models2[i].localToWorldMatrix;
            Matrix4x4 boneToModel = modelToBone.inverse;

            modelRoots2[i].transform.localPosition = boneToModel.ExtractPosition();
            modelRoots2[i].transform.localRotation = boneToModel.ExtractRotation();
            modelRoots2[i].transform.localScale = boneToModel.ExtractScale();

            models2[i].transform.parent = modelRoots2[i].transform;
        }

        // Attach collider
        GameObject[] colliderRoots = new GameObject[modelNames.Length];
        for (int i = 0; i < modelNames.Length; i++) {
            colliderRoots[i] = new GameObject("Collider");
            colliderRoots[i].transform.parent = colliders[i];

            Matrix4x4 modelToBone = bones[i].worldToLocalMatrix * colliders[i].localToWorldMatrix;
            Matrix4x4 boneToModel = modelToBone.inverse;

            colliderRoots[i].transform.localPosition = boneToModel.ExtractPosition();
            colliderRoots[i].transform.localRotation = boneToModel.ExtractRotation();
            colliderRoots[i].transform.localScale = boneToModel.ExtractScale();

            colliders[i].transform.parent = colliderRoots[i].transform;
        }

        // Attach pivot
        Pivot[] pivots = GetComponentsInChildren<Pivot>();
        foreach (Pivot pivot in pivots) {
            for (int i = 0; i < boneNames.Length; i++) {
                if (boneNames[i] == pivot.boneName) {
                    pivot.transform.parent = bones[i].transform;
                    break;
                }
            }
        }

        surfaceRenderersByBone = new List<SurfaceRenderer>[boneNames.Length];
        for (int i = 0; i < boneNames.Length; i++) {
            surfaceRenderersByBone[i] = new List<SurfaceRenderer>();
            AddSurfaceRendererRecursively(bones[i], i);
        }
    }

    void AddSurfaceRendererRecursively(Transform curr, int index) {
        for (int i = 0; i < curr.childCount; i++) {
            Transform child = curr.GetChild(i);
            if (child.GetComponent<MeshRenderer>()) {
                surfaceRenderersByBone[index].Add(child.gameObject.AddComponent<SurfaceRenderer>());
            }

            AddSurfaceRendererRecursively(child, index);
        }
    }

    public Transform GetPivot(Inventory.Slot slot, bool isUsingSword) {
        if (isUsingSword) {
            if (slot == Inventory.Slot.LEFT_HAND) return leftBackPivot;
            if (slot == Inventory.Slot.RIGHT_HAND) return rightBackPivot;
            if (slot == Inventory.Slot.SWORD) return rightHandPivot;
            return pivots[slot];
        }
        else {
            if (slot == Inventory.Slot.SWORD) return middleBackPivot;
            return pivots[slot];
        }
    }

    public void AddHole(int bone, Vector3 globalPos) {
        // @Todo
    }

    public void SetBodySlice(Vector3 dir, float ratio) {
        if (sliceBox == null) {
            sliceBox = Instantiate(PrefabRegistry.Instance.sliceEffectBox);
            sliceBox.transform.parent = cockpit;

            foreach (List<SurfaceRenderer> surfaceRenderers in surfaceRenderersByBone) {
                foreach (SurfaceRenderer surfaceRenderer in surfaceRenderers) {
                    surfaceRenderer.holes.Add(sliceBox.GetComponent<MeshFilter>());
                }
            }
        }

        const float radius = 4;
        const float thickness = 0.15f;
        const float depth = 4;

        Vector3 from = -dir * radius;
        Vector3 to = dir * Mathf.Lerp(-radius, radius, ratio);

        sliceBox.transform.localScale = new Vector3(Vector3.Distance(from, to) + thickness, thickness, depth);
        sliceBox.transform.localPosition = (from + to) / 2;
        sliceBox.transform.localRotation = Quaternion.FromToRotation(Vector3.right, dir);
    }
}

public static class MatrixExtensions
{
    public static Quaternion ExtractRotation(this Matrix4x4 matrix)
    {
        Vector3 forward;
        forward.x = matrix.m02;
        forward.y = matrix.m12;
        forward.z = matrix.m22;

        Vector3 upwards;
        upwards.x = matrix.m01;
        upwards.y = matrix.m11;
        upwards.z = matrix.m21;

        return Quaternion.LookRotation(forward, upwards);
    }

    public static Vector3 ExtractPosition(this Matrix4x4 matrix)
    {
        Vector3 position;
        position.x = matrix.m03;
        position.y = matrix.m13;
        position.z = matrix.m23;
        return position;
    }

    public static Vector3 ExtractScale(this Matrix4x4 matrix)
    {
        Vector3 scale;
        scale.x = new Vector4(matrix.m00, matrix.m10, matrix.m20, matrix.m30).magnitude;
        scale.y = new Vector4(matrix.m01, matrix.m11, matrix.m21, matrix.m31).magnitude;
        scale.z = new Vector4(matrix.m02, matrix.m12, matrix.m22, matrix.m32).magnitude;
        return scale;
    }
}