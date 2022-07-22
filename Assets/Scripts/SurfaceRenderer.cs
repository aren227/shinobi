using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SurfaceRenderer : MonoBehaviour
{
    static Material depthPassMat, depthWriteMat;
    static Material depthWrite2Mat;

    MeshFilter meshFilter;
    public List<MeshFilter> holes = new List<MeshFilter>();

    public MeshFilter cubeMeshFilter;

    bool disabled = false;

    void Awake() {
        if (depthPassMat == null) depthPassMat = PrefabRegistry.Instance.depthPassMat;
        if (depthWriteMat == null) depthWriteMat = PrefabRegistry.Instance.depthWriteMat;
        if (depthWrite2Mat == null) depthWrite2Mat = PrefabRegistry.Instance.depthWrite2Mat;

        meshFilter = GetComponent<MeshFilter>();

        SurfaceRenderManager.surfaceRenderers.Add(this);
    }

    public void SetDisabled(bool disabled) {
        this.disabled = disabled;
    }

    public void AppendToCommandBuffer(CommandBuffer cb) {
        if (disabled) return;

        if (holes.Count == 0) {
            // Just write front depth.
            cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            cb.DrawMesh(meshFilter.sharedMesh, transform.localToWorldMatrix, depthWriteMat, 0, 0);
        }
        else {
            int id;

            // Draw front holes.
            id = Shader.PropertyToID("_FrontDepth");
            cb.GetTemporaryRT(id, -1, -1, 24, FilterMode.Point, RenderTextureFormat.Depth);
            cb.SetRenderTarget(id);
            cb.ClearRenderTarget(true, true, Color.clear, 1f);

            foreach (MeshFilter meshFilter in holes) {
                cb.DrawMesh(meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, depthWriteMat, 0, 0);
            }

            cb.SetGlobalTexture("_FrontDepth", id);

            // Draw back holes.
            id = Shader.PropertyToID("_BackDepth");
            cb.GetTemporaryRT(id, -1, -1, 24, FilterMode.Point, RenderTextureFormat.Depth);
            cb.SetRenderTarget(id);
            cb.ClearRenderTarget(true, true, Color.clear, 0f);

            foreach (MeshFilter meshFilter in holes) {
                cb.DrawMesh(meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, depthWriteMat, 0, 1);
            }

            cb.SetGlobalTexture("_BackDepth", id);

            // Draw object.
            cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
            // Do pass 0, 1.
            cb.DrawMesh(meshFilter.sharedMesh, transform.localToWorldMatrix, depthPassMat, 0, 0);
            cb.DrawMesh(meshFilter.sharedMesh, transform.localToWorldMatrix, depthPassMat, 0, 1);

            // Draw holes for real.
            foreach (MeshFilter meshFilter in holes) {
                cb.DrawMesh(meshFilter.sharedMesh, meshFilter.transform.localToWorldMatrix, depthPassMat, 0, 2);
            }
        }
    }

    // Do nothing with holes, but only one cube.
    public void AppendToCommandBuffer2(CommandBuffer cb) {
        if (disabled) return;

        Matrix4x4 matrix;
        if (cubeMeshFilter) {
            matrix = cubeMeshFilter.transform.worldToLocalMatrix * transform.localToWorldMatrix;
        }
        // Place a cube far away.
        else {
            matrix = Matrix4x4.TRS(transform.position + new Vector3(100, 100, 100), Quaternion.identity, Vector3.one);
        }

        cb.SetGlobalMatrix(Shader.PropertyToID("_ObjectToCube"), matrix);

        cb.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        cb.DrawMesh(meshFilter.sharedMesh, transform.localToWorldMatrix, depthWrite2Mat, 0, 0);
        cb.DrawMesh(meshFilter.sharedMesh, transform.localToWorldMatrix, depthWrite2Mat, 0, 1);

        if (cubeMeshFilter) cb.DrawMesh(cubeMeshFilter.sharedMesh, cubeMeshFilter.transform.localToWorldMatrix, depthWrite2Mat, 0, 2);
    }
}
