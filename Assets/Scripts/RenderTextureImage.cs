using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RenderTextureImage : MonoBehaviour
{
    RawImage image;
    RectTransform rectTransform;
    CanvasScaler canvasScaler;
    Canvas canvas;

    public RenderTexture renderTexture { get; private set; }

    void Awake() {
        image = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        canvasScaler = GetComponentInParent<CanvasScaler>();
        canvas = GetComponentInParent<Canvas>();
    }

    void Update() {
        if (canvas.enabled) {
            Vector2Int dim = GetSize(canvasScaler, rectTransform);

            if (renderTexture == null || dim.x != renderTexture.width || dim.y != renderTexture.height) {
                CreateRenderTexture(dim);
            }
        }
    }

    void CreateRenderTexture(Vector2Int dim) {
        if (renderTexture != null) {
            renderTexture.Release();
        }

        Debug.Log("Create a rt with " + dim);

        renderTexture = new RenderTexture(dim.x, dim.y, 16, RenderTextureFormat.DefaultHDR);
        renderTexture.Create();

        image.texture = renderTexture;
    }

    public static Vector2Int GetSize(CanvasScaler canvasScaler, RectTransform rect) {
        float wRatio = Screen.width / canvasScaler.referenceResolution.x;
        float hRatio = Screen.height / canvasScaler.referenceResolution.y;
        float ratio =
            wRatio * (1f - canvasScaler.matchWidthOrHeight) +
            hRatio * (canvasScaler.matchWidthOrHeight);
        float pixelWidth  = rect.rect.width  * ratio;
        float pixelHeight = rect.rect.height * ratio;
        return new Vector2Int(Mathf.RoundToInt(pixelWidth), Mathf.RoundToInt(pixelHeight));
    }
}
