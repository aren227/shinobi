using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class Skeleton : MonoBehaviour
{
    public Mech mech { get; private set; }

    public Transform pivotRoot;
    public Transform boneRoot;
    public Transform modelRoot; // Frame
    public Transform modelRoot2; // Armor
    public Transform frameColliderRoot;
    public Transform armorColliderRoot;

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

    // public Transform cockpit;

    public Transform leftArmSlicePivot;
    public Transform rightArmSlicePivot;
    public Transform leftBodySlicePivot;
    public Transform rightBodySlicePivot;
    public Transform leftLegSlicePivot;
    public Transform rightLegSlicePivot;

    public Transform swordSwingMirror;
    public Transform swordSwingPivot;

    public Thruster thruster;
    public Cockpit cockpit;

    public Transform leftHandIkTarget;
    public Transform rightHandIkTarget;
    public Transform leftHandIkHint;
    public Transform leftHandIkSwingHint;
    public Transform rightHandIkHint;
    public Transform rightHandIkSwingHint;

    public Transform slicePivot;

    public TwoBoneIKConstraint leftHandIk;
    public TwoBoneIKConstraint rightHandIk;

    public ParticleSystem hideEffect;

    Animator animator;

    Dictionary<Inventory.Slot, Transform> pivots = new Dictionary<Inventory.Slot, Transform>();

    List<SurfaceRenderer>[] surfaceRenderersByBone;

    Dictionary<Collider, Part> partByCollider = new Dictionary<Collider, Part>();
    Dictionary<Transform, PartName> partByPivot = new Dictionary<Transform, PartName>();

    Dictionary<PartName, Part> parts = new Dictionary<PartName, Part>();

    GameObject sliceBox;

    // If these drop to zero, then disable ik.
    float leftHandIkTime, rightHandIkTime;

    string[] boneNames = new string[] {
        "Bone", "Head", "UArm.L", "LArm.L", "Hand.L", "UArm.R", "LArm.R", "Hand.R",
        "ULeg.L", "LLeg.L", "ULeg.R", "LLeg.R",
    };

    PartName[] bonePartNames = new PartName[] {
        PartName.BODY, PartName.HEAD, PartName.UPPER_LEFT_ARM, PartName.LOWER_LEFT_ARM, PartName.NONE,
        PartName.UPPER_RIGHT_ARM, PartName.LOWER_RIGHT_ARM, PartName.NONE,
        PartName.UPPER_LEFT_LEG, PartName.LOWER_LEFT_LEG, PartName.UPPER_RIGHT_LEG, PartName.LOWER_RIGHT_LEG,
    };

    string[] modelNames = new string[] {
        "Body", "Head", "Arm_R_Up", "Arm_R_Down", null, "Arm_L_Up", "Arm_L_Down", null,
        "Leg_R_Up", "Leg_R_Down", "Leg_L_Up", "Leg_L_Down",
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

        partByPivot.Add(leftHandPivot, PartName.LOWER_LEFT_ARM);
        partByPivot.Add(rightHandPivot, PartName.LOWER_RIGHT_ARM);
        partByPivot.Add(leftShoulderPivot, PartName.BODY);
        partByPivot.Add(rightShoulderPivot, PartName.BODY);
        partByPivot.Add(leftArmPivot, PartName.UPPER_LEFT_ARM);
        partByPivot.Add(rightArmPivot, PartName.UPPER_RIGHT_ARM);
        partByPivot.Add(leftLegPivot, PartName.UPPER_LEFT_LEG);
        partByPivot.Add(rightLegPivot, PartName.UPPER_RIGHT_LEG);
        partByPivot.Add(leftBackPivot, PartName.BODY);
        partByPivot.Add(middleBackPivot, PartName.BODY);
        partByPivot.Add(rightBackPivot, PartName.BODY);

        // Force apply.
        animator.Play("Armature|Rest");
        animator.Update(0);

        AttachToBone();

        thruster = GetComponentInChildren<Thruster>();

        // @Hardcoded
        animator.Play("Move Blend Tree");
    }

    void Start() {
        GameObject cloned = Instantiate(PrefabRegistry.Instance.sliceEffectBox);
        cloned.transform.parent = cockpit.transform;
        cloned.transform.localPosition = Vector3.zero;
    }

    void Update() {
        Vector3 localVelocity = Quaternion.Inverse(mech.transform.rotation) * mech.velocity;
        Vector2 motion = new Vector2(localVelocity.x, localVelocity.z) / 10f;

        if (motion.magnitude > 1) motion = motion.normalized;

        animator.SetFloat("X", motion.x);
        animator.SetFloat("Y", motion.y);

        if (leftHandIkTime <= 0 && leftHandIk.enabled) {
            DisableHandIk(false);

            Item item = mech.skeleton.leftHandPivot.GetComponentInChildren<Item>();
            if (item) {
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
            }
        }
        if (rightHandIkTime <= 0 && rightHandIk.enabled) {
            DisableHandIk(true);

            Item item = mech.skeleton.rightHandPivot.GetComponentInChildren<Item>();
            if (item) {
                item.transform.localPosition = Vector3.zero;
                item.transform.localRotation = Quaternion.identity;
            }
        }

        if (leftHandIk.weight > 0) {
            Item item = mech.skeleton.leftHandPivot.GetComponentInChildren<Item>();
            if (item) {
                Weapon weapon = item.GetComponent<Weapon>();
                if (weapon) {
                    Vector3 dir = (mech.aimTarget - weapon.transform.position).normalized;
                    weapon.transform.position = mech.skeleton.leftGunPivot.position + dir * 0.3f;
                    weapon.transform.forward = dir;
                }

                leftHandIkTarget.position = item.transform.position;
                leftHandIkTarget.rotation = item.transform.rotation;
            }
        }
        if (rightHandIk.weight > 0) {
            Item item = mech.skeleton.rightHandPivot.GetComponentInChildren<Item>();
            if (item) {
                Weapon weapon = item.GetComponent<Weapon>();
                if (weapon) {
                    Vector3 dir = (mech.aimTarget - weapon.transform.position).normalized;
                    weapon.transform.position = mech.skeleton.rightGunPivot.position + dir * 0.3f;
                    weapon.transform.forward = dir;
                }

                rightHandIkTarget.position = item.transform.position;
                rightHandIkTarget.rotation = item.transform.rotation;
            }
        }

        leftHandIkTime = Mathf.Max(leftHandIkTime - Time.deltaTime, 0);
        rightHandIkTime = Mathf.Max(rightHandIkTime - Time.deltaTime, 0);

        // SetBodySlice(new Vector3(1, 1, 0).normalized, Mathf.InverseLerp(-1, 1, Mathf.Sin(Time.time)));
    }

    public void BeginMeleeAttackAnimation(bool isRight) {
        if (isRight) {
            animator.SetTrigger("Right Attack");
        }
        else {
            animator.SetTrigger("Left Attack");
        }
    }

    public void EnableHandIk(bool isRight, bool isSwing, float time) {
        if (isRight) {
            rightHandIk.enabled = true;
            rightHandIk.weight = 1;
            rightHandIkTime = time;

            if (isSwing) {
                rightHandIk.data.hint = rightHandIkSwingHint;
            }
            else {
                rightHandIk.data.hint = rightHandIkHint;
            }
        }
        else {
            leftHandIk.enabled = true;
            leftHandIk.weight = 1;
            leftHandIkTime = time;

            if (isSwing) {
                leftHandIk.data.hint = leftHandIkSwingHint;
            }
            else {
                leftHandIk.data.hint = leftHandIkHint;
            }
        }
    }

    public void DisableHandIk(bool isRight) {
        if (isRight) {
            rightHandIk.weight = 0;
            rightHandIk.enabled = false;
            rightHandIkTime = 0;
        }
        else {
            leftHandIk.weight = 0;
            leftHandIk.enabled = false;
            leftHandIkTime = 0;
        }
    }

    void FindRecursive(Transform current, Transform[] array, string[] names) {
        for (int i = 0; i < current.childCount; i++) {
            Transform child = current.GetChild(i);

            for (int j = 0; j < names.Length; j++) {
                if (child.name == names[j]) {
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
        Transform[] frameColliders = new Transform[modelNames.Length];
        Transform[] armorColliders = new Transform[modelNames.Length];

        FindRecursive(boneRoot, bones, boneNames);
        FindRecursive(modelRoot, models, modelNames);
        FindRecursive(modelRoot2, models2, modelNames);
        FindRecursive(frameColliderRoot, frameColliders, boneNames);
        FindRecursive(armorColliderRoot, armorColliders, boneNames);

        for (int i = 0; i < boneNames.Length; i++) {
            if (bones[i] == null) Debug.LogError($"Bone {boneNames[i]} not found!");
            if (models[i] == null) Debug.LogWarning($"Model {modelNames[i]} not found!");
            if (models2[i] == null) Debug.LogWarning($"Model2 {modelNames[i]} not found!");
            if (frameColliders[i] == null) Debug.LogWarning($"Frame collider {boneNames[i]} not found!");
            if (armorColliders[i] == null) Debug.LogWarning($"Armor collider {boneNames[i]} not found!");
        }

        parts = new Dictionary<PartName, Part>();
        for (int i = 0; i < boneNames.Length; i++) {
            Part part = bones[i].GetComponent<Part>();

            if (part == null) continue;

            parts[part.partName] = part;

            part.skeleton = this;
        }

        // Assume that bones are in rest pose.

        // Attach model
        // We need another root objects because pivots of model is arbitrary.
        GameObject[] modelRoots = new GameObject[modelNames.Length];
        for (int i = 0; i < modelNames.Length; i++) {
            if (models[i] == null) continue;

            modelRoots[i] = new GameObject("Model");
            modelRoots[i].transform.parent = bones[i];

            Matrix4x4 modelToBone = bones[i].worldToLocalMatrix * models[i].localToWorldMatrix;
            Matrix4x4 boneToModel = modelToBone.inverse;

            modelRoots[i].transform.localPosition = boneToModel.ExtractPosition();
            modelRoots[i].transform.localRotation = boneToModel.ExtractRotation();
            modelRoots[i].transform.localScale = boneToModel.ExtractScale();

            models[i].transform.parent = modelRoots[i].transform;

            parts[bonePartNames[i]].frameRoot = modelRoots[i].transform;
        }

        // Attach armor
        GameObject[] modelRoots2 = new GameObject[modelNames.Length];
        for (int i = 0; i < modelNames.Length; i++) {
            if (models2[i] == null) continue;

            modelRoots2[i] = new GameObject("Model2");
            modelRoots2[i].transform.parent = bones[i];

            Matrix4x4 modelToBone = bones[i].worldToLocalMatrix * models2[i].localToWorldMatrix;
            Matrix4x4 boneToModel = modelToBone.inverse;

            modelRoots2[i].transform.localPosition = boneToModel.ExtractPosition();
            modelRoots2[i].transform.localRotation = boneToModel.ExtractRotation();
            modelRoots2[i].transform.localScale = boneToModel.ExtractScale();

            models2[i].transform.parent = modelRoots2[i].transform;

            parts[bonePartNames[i]].armorRoot = modelRoots2[i].transform;
        }

        // Attach frame collider
        GameObject[] frameColliderRoots = new GameObject[modelNames.Length];
        for (int i = 0; i < modelNames.Length; i++) {
            if (frameColliders[i] == null) continue;

            frameColliderRoots[i] = new GameObject("Frame Collider");
            frameColliderRoots[i].transform.parent = bones[i];

            Matrix4x4 modelToBone = bones[i].worldToLocalMatrix * frameColliders[i].localToWorldMatrix;
            Matrix4x4 boneToModel = modelToBone.inverse;

            frameColliderRoots[i].transform.localPosition = boneToModel.ExtractPosition();
            frameColliderRoots[i].transform.localRotation = boneToModel.ExtractRotation();
            frameColliderRoots[i].transform.localScale = boneToModel.ExtractScale();

            frameColliders[i].transform.parent = frameColliderRoots[i].transform;

            AddColliderToPart(frameColliders[i].GetComponent<Collider>(), isArmor: false, parts[bonePartNames[i]]);
        }

        // Attach armor collider
        GameObject[] armorColliderRoots = new GameObject[modelNames.Length];
        for (int i = 0; i < modelNames.Length; i++) {
            if (armorColliders[i] == null) continue;

            armorColliderRoots[i] = new GameObject("Armor Collider");
            armorColliderRoots[i].transform.parent = bones[i];

            Matrix4x4 modelToBone = bones[i].worldToLocalMatrix * armorColliders[i].localToWorldMatrix;
            Matrix4x4 boneToModel = modelToBone.inverse;

            armorColliderRoots[i].transform.localPosition = boneToModel.ExtractPosition();
            armorColliderRoots[i].transform.localRotation = boneToModel.ExtractRotation();
            armorColliderRoots[i].transform.localScale = boneToModel.ExtractScale();

            armorColliders[i].transform.parent = armorColliderRoots[i].transform;

            AddColliderToPart(armorColliders[i].GetComponent<Collider>(), isArmor: true, parts[bonePartNames[i]]);
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

    public Transform GetPivot(Inventory.Slot slot) {
        Part lowerLeft = GetPart(PartName.LOWER_LEFT_ARM);
        Part lowerRight = GetPart(PartName.LOWER_RIGHT_ARM);

        if (slot == Inventory.Slot.SWORD) {
            if (mech.isUsingSword) {
                if (mech.swordController.isRightHanded) return rightHandPivot;
                else return leftHandPivot;
            }
            return middleBackPivot;
        }
        if (slot == Inventory.Slot.LEFT_HAND) {
            if (mech.isUsingSword || lowerLeft.disabled) return leftBackPivot;
            else return leftHandPivot;
        }
        if (slot == Inventory.Slot.RIGHT_HAND) {
            if (mech.isUsingSword || lowerRight.disabled) return rightBackPivot;
            else return rightHandPivot;
        }

        return pivots[slot];
    }

    public Part GetPartBySlot(Inventory.Slot slot) {
        return GetPart(partByPivot[GetPivot(slot)]);
    }

    public void AddColliderToPart(Collider collider, bool isArmor, Part part) {
        partByCollider[collider] = part;
        if (isArmor) part.armorColliders.Add(collider);
        else part.frameColliders.Add(collider);
    }

    public Part GetPartByCollider(Collider collider) {
        if (partByCollider.ContainsKey(collider)) return partByCollider[collider];
        return null;
    }

    public IEnumerable<Part> GetParts() {
        return parts.Values;
    }

    public void AddHole(int bone, Vector3 globalPos) {
        // @Todo
    }

    public void SetBodySlice(Vector3 dir, float ratio) {
        if (sliceBox == null) {
            sliceBox = Instantiate(PrefabRegistry.Instance.sliceEffectBox);
            // sliceBox.transform.parent = cockpit;
            sliceBox.transform.parent = GetPart(PartName.BODY).transform;

            foreach (List<SurfaceRenderer> surfaceRenderers in surfaceRenderersByBone) {
                foreach (SurfaceRenderer surfaceRenderer in surfaceRenderers) {
                    // @Todo
                    // surfaceRenderer.holes.Add(sliceBox.GetComponent<MeshFilter>());
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

    public void SetHoleCube(MeshFilter cubeMeshFilter) {
        foreach (Part part in parts.Values) {
            part.SetHoleCube(cubeMeshFilter);
        }
    }

    public Part GetPart(PartName partName) {
        return parts[partName];
    }
}

public enum PartName {
    BODY,
    HEAD,
    UPPER_LEFT_ARM,
    LOWER_LEFT_ARM,
    UPPER_RIGHT_ARM,
    LOWER_RIGHT_ARM,
    UPPER_LEFT_LEG,
    LOWER_LEFT_LEG,
    UPPER_RIGHT_LEG,
    LOWER_RIGHT_LEG,
    NONE,
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