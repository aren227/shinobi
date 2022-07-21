using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SurfaceRenderManager : MonoBehaviour
{
    public static List<SurfaceRenderer> surfaceRenderers = new List<SurfaceRenderer>();

    CommandBuffer commandBuffer;

    void Awake() {
        commandBuffer = new CommandBuffer();
        commandBuffer.name = "Robot Surface Prepass";

        Camera.main.AddCommandBuffer(CameraEvent.BeforeGBuffer, commandBuffer);
    }

    void LateUpdate() {
        commandBuffer.Clear();
        for (int i = 0; i < surfaceRenderers.Count; i++) {
            if (!surfaceRenderers[i]) surfaceRenderers.RemoveAt(i);
            else {
                surfaceRenderers[i].AppendToCommandBuffer2(commandBuffer);
            }
        }
    }
}
