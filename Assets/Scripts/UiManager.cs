using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public Slider steminaSlider;
    public Text speedText;

    public void SetMaxStemina(float maxStemina) {
        steminaSlider.maxValue = maxStemina;
    }

    public void SetStemina(float stemina) {
        steminaSlider.value = stemina;
    }

    public void SetSpeed(float speedMeterPerSec) {
        speedText.text = $"{Mathf.Round(speedMeterPerSec * (3600f / 1000f))} km/h";
    }
}
