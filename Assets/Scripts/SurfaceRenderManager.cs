using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class SurfaceRenderManager : MonoBehaviour
{
    CommandBuffer commandBuffer;

    void Awake() {
        commandBuffer = new CommandBuffer();
        commandBuffer.name = "Robot Surface Prepass";

        Camera.main.AddCommandBuffer(CameraEvent.BeforeGBuffer, commandBuffer);
    }

    void LateUpdate() {
        SurfaceRenderer[] surfaceRenderers = FindObjectsOfType<SurfaceRenderer>();

        commandBuffer.Clear();
        foreach (SurfaceRenderer surfaceRenderer in surfaceRenderers) {
            surfaceRenderer.AppendToCommandBuffer(commandBuffer);
        }
    }
}
