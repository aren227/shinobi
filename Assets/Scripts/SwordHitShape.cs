using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// From topmost, clockwise.
public class SwordHitShape
{
    public float[] proportions = new float[] { 0.2f, 0.1f, 0.2f, 0.2f, 0.1f, 0.2f };
    public float[] fillRate = new float[] { 0.5f, 0.3f, 0.5f, 0.5f, 0.3f, 0.5f };
    public float offset = 0;

    public bool IsHit(float angle, out SwordHitPoint where) {
        angle -= offset;
        angle /= 360;

        where = SwordHitPoint.RIGHT_ARM;

        float s = 0;
        for (int i = 0; i < 6; i++) {
            if (angle < s + proportions[i]) {
                where = (SwordHitPoint) i;

                float center = s + proportions[i]/2;
                float halfW = proportions[i]*fillRate[i]/2;
                return center - halfW <= angle && angle <= center + halfW;
            }
            s += proportions[i];
        }

        return false;
    }
}

public enum SwordHitPoint {
    RIGHT_ARM,
    RIGHT_BODY,
    RIGHT_LEG,
    LEFT_LEG,
    LEFT_BODY,
    LEFT_ARM,
}
