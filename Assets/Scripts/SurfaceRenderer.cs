using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SurfaceRenderer : MonoBehaviour
{
    static Material depthPassMat, depthWriteMat;

    MeshFilter meshFilter;
    public List<MeshFilter> holes = new List<MeshFilter>();

    void Awake() {
        if (depthPassMat == null) depthPassMat = new Material(Shader.Find("Unlit/DepthPass"));
        if (depthWriteMat == null) depthWriteMat = new Material(Shader.Find("Unlit/DepthWrite"));

        meshFilter = GetComponent<MeshFilter>();
    }

    public void AppendToCommandBuffer(CommandBuffer cb) {
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
}
