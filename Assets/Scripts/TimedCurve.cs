using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class TimedCurve
{
    public AnimationCurve curve;
    public float duration;

    public float minValue = 0;
    public float maxValue = 1;

    public float Evaluate(float t)
    {
        float x = curve.Evaluate(Mathf.Clamp01(t / duration));
        return Mathf.LerpUnclamped(minValue, maxValue, x);
    }
}
