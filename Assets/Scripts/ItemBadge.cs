using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ItemBadge : MonoBehaviour
{
    public void SetItem(Item item) {
        Text text = GetComponentInChildren<Text>();
        text.text = item.name;
    }

    public void SetHighlightColor(Color color) {
        Image image = GetComponent<Image>();
        image.color = color;
    }

    public void SetOnClick(UnityAction action) {
        GetComponentInChildren<Button>().onClick.AddListener(action);
    }
}
