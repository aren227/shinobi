using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Damagable : MonoBehaviour
{
    public int maxHealth = 100;
    public int health;

    public UnityEvent<int> damageListener { get; private set; } = new UnityEvent<int>();

    SurfaceRenderer[] surfaceRenderers;

    Mesh holeMesh;
    Vector3 holeMeshScale;

    MeshFilter holeMeshFilter;

    Mesh combinedHoleMesh;

    void Awake() {
        // holeMesh = PrefabRegistry.Instance.bulletHole.GetComponent<MeshFilter>().sharedMesh;
        // holeMeshScale = PrefabRegistry.Instance.bulletHole.transform.lossyScale;

        // combinedHoleMesh = new Mesh();

        // GameObject holeMeshObject = new GameObject("Hole Mesh");
        // holeMeshObject.transform.parent = transform;
        // holeMeshObject.transform.localPosition = Vector3.zero;
        // holeMeshObject.transform.localRotation = Quaternion.identity;
        // holeMeshObject.transform.localScale = Vector3.one;

        // MeshRenderer meshRenderer = holeMeshObject.AddComponent<MeshRenderer>();
        // meshRenderer.sharedMaterial = PrefabRegistry.Instance.bulletHoleMat;

        // holeMeshFilter = holeMeshObject.AddComponent<MeshFilter>();
        // holeMeshFilter.sharedMesh = combinedHoleMesh;

        health = maxHealth;
    }

    public void Hit(int damage) {
        health = Mathf.Max(health - damage, 0);

        damageListener.Invoke(damage);

        // // Lazy
        // if (surfaceRenderers == null) {
        //     surfaceRenderers = GetComponentsInChildren<SurfaceRenderer>();

        //     foreach (SurfaceRenderer surfaceRenderer in surfaceRenderers) {
        //         surfaceRenderer.holes.Add(holeMeshFilter);
        //     }
        // }

        // CombineInstance lastCombineInstance = new CombineInstance();

        // lastCombineInstance.mesh = combinedHoleMesh;
        // lastCombineInstance.transform = Matrix4x4.identity;

        // CombineInstance newCombineInstance = new CombineInstance();

        // Matrix4x4 toLocal = transform.worldToLocalMatrix;

        // pos = toLocal * new Vector4(pos.x, pos.y, pos.z, 1);
        // normal = (toLocal * new Vector4(normal.x, normal.y, normal.z, 0)).normalized;

        // Quaternion rot = Quaternion.AngleAxis(Random.Range(0f, 360f), normal);

        // newCombineInstance.mesh = holeMesh;
        // newCombineInstance.transform = Matrix4x4.TRS(pos, rot, holeMeshScale);

        // // @Inefficient
        // Mesh newlyCombinedMesh = new Mesh();

        // newlyCombinedMesh.CombineMeshes(
        //     new CombineInstance[] { lastCombineInstance, newCombineInstance },
        //     mergeSubMeshes: true,
        //     useMatrices: true,
        //     hasLightmapData: false
        // );

        // combinedHoleMesh = newlyCombinedMesh;

        // holeMeshFilter.sharedMesh = combinedHoleMesh;
    }
}
