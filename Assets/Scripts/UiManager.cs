using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public Slider steminaSlider;
    public Text speedText;

    public GameObject thermalTargetCursor;

    public Material crackOverlayMat;

    Canvas canvas;

    List<GameObject> thermalTargetCursors = new List<GameObject>();

    void Awake() {
        canvas = GetComponent<Canvas>();

        thermalTargetCursor.SetActive(false);
    }

    void Start() {
        SetCockpitHealth(1);
    }

    public void SetMaxStemina(float maxStemina) {
        steminaSlider.maxValue = maxStemina;
    }

    public void SetStemina(float stemina) {
        steminaSlider.value = stemina;
    }

    public void SetSpeed(float speedMeterPerSec) {
        speedText.text = $"{Mathf.Round(speedMeterPerSec * (3600f / 1000f))} km/h";
    }

    public void SetTargets(List<Target> targets, Camera cam) {
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();

        for (int i = 0; i < targets.Count; i++) {
            if (thermalTargetCursors.Count <= i) {
                GameObject cloned = Instantiate(thermalTargetCursor);

                cloned.transform.parent = canvas.transform;

                thermalTargetCursors.Add(cloned);
            }

            thermalTargetCursors[i].SetActive(true);

            // @Hardcoded
            Color color;
            if (targets[i].type == TargetType.THERMAL) color = Color.red;
            else if (targets[i].type == TargetType.VITAL) color = Color.blue;
            else if (targets[i].type == TargetType.MISSILE) color = Color.green;
            else color = Color.gray;

            thermalTargetCursors[i].GetComponentInChildren<Image>().color = color;

            RectTransform rect = thermalTargetCursors[i].GetComponent<RectTransform>();

            Vector2 viewport = cam.WorldToViewportPoint(targets[i].transform.position);
            Vector2 anchored = new Vector2(
                ((viewport.x*canvasRect.sizeDelta.x)-(canvasRect.sizeDelta.x*0.5f)),
                ((viewport.y*canvasRect.sizeDelta.y)-(canvasRect.sizeDelta.y*0.5f))
            );

            rect.anchoredPosition = anchored;
        }

        // Disable remainders.
        for (int i = targets.Count; i < thermalTargetCursors.Count; i++) {
            thermalTargetCursors[i].SetActive(false);
        }
    }

    public void SetCockpitHealth(float healthRate) {
        if (healthRate >= 1f) {
            // Hide effects.
            crackOverlayMat.SetFloat("_Health", 1f);
        }
        else {
            crackOverlayMat.SetFloat("_Health", Mathf.Lerp(0f, 0.5f, healthRate));
        }
    }
}
