using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwordCanvas : MonoBehaviour
{
    public Image crosshair;
    public Image aimCircle;

    public Transform radialParent;
    Image[] radials = new Image[6];

    public float aimCircleCanvasRadius = 28;

    void Awake() {
        Debug.Assert(radialParent.childCount == 6);
        Debug.Assert(System.Enum.GetValues(typeof(SwordHitPoint)).Length == 6);

        for (int i = 0; i < 6; i++) {
            radials[i] = radialParent.GetChild(i).GetComponent<Image>();
        }
    }

    public void SetAim(Vector2 pos) {
        RectTransform rect = aimCircle.GetComponent<RectTransform>();

        rect.anchoredPosition = crosshair.GetComponent<RectTransform>().anchoredPosition + pos * aimCircleCanvasRadius;
    }

    public void SetAimColor(Color color) {
        aimCircle.color = color;
    }

    public void SetSwordHitShape(SwordHitShape hitShape) {
        radialParent.localEulerAngles = new Vector3(0, 0, -hitShape.offset);

        float sum = 0;
        for (int i = 0; i < 6; i++) {
            radials[i].transform.localEulerAngles = new Vector3(0, 0, -180 - sum - hitShape.proportions[i]*360/2 + hitShape.proportions[i]*360*hitShape.fillRate[i]/2);
            radials[i].GetComponent<Image>().fillAmount = hitShape.proportions[i] * hitShape.fillRate[i];

            sum += hitShape.proportions[i] * 360;
        }
    }
}
