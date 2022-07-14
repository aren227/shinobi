using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemView : MonoBehaviour
{
    public RenderTextureImage itemImage;
    public Text itemName;

    CanvasGroup canvasGroup;

    void Awake() {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void SetItem(Item item) {
        if (item == null) {
            canvasGroup.alpha = 0;
        }
        else {
            canvasGroup.alpha = 1;

            itemName.text = item.displayName;
        }
    }
}
