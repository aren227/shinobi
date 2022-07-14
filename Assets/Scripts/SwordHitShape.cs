using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// From topmost, clockwise.
public class SwordHitShape
{
    public float[] proportions = new float[] { 0.2f, 0.1f, 0.2f, 0.2f, 0.1f, 0.2f };
    public float[] fillRate = new float[] { 0.5f, 0.2f, 0.5f, 0.5f, 0.2f, 0.5f };
    public float offset = 0;
}

public enum SwordHitPoint {
    RIGHT_ARM,
    RIGHT_BODY,
    RIGHT_LEG,
    LEFT_LEG,
    LEFT_BODY,
    LEFT_ARM,
}
