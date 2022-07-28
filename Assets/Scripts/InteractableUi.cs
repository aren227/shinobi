using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableUi : MonoBehaviour
{
    public Text mainText;
    public Text subText;

    public void SetText(string main, string sub) {
        mainText.text = main;
        subText.text = sub;
    }
}
