using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ItemBadge : MonoBehaviour
{
    bool hovered = false, selected = false;

    public void SetItem(Item item) {
        Text text = GetComponentInChildren<Text>();

        if (item == null) text.text = "";
        else text.text = item.displayName;
    }

    public void SetHovered(bool hovered) {
        this.hovered = hovered;
        UpdateColor();
    }

    public void SetSelected(bool selected) {
        this.selected = selected;
        UpdateColor();
    }

    void UpdateColor() {
        Image image = GetComponent<Image>();

        Color color = Color.white * 0.1f;

        if (hovered) color += Color.white * 0.6f;
        if (selected) color += Color.red;

        image.color = color;
    }
}
