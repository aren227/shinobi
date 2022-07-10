using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DampBasedVelocitySolver : VelocitySolver
{
    const float walkSpeed = 10;
    const float boostSpeed = 50;

    float boostTransitionT;
    const float boostTransitionDuration = 0.3f;

    Vector3 prevVelocity1;
    Vector3 velocity1Vel;
    Vector3 prevVelocity2;
    Vector3 velocity2Vel;

    bool prevBoost = false;

    public Vector3 Update(Vector3 input, bool boost) {
        Vector3 velocity1 = input * walkSpeed;
        Vector3 velocity2 = input * boostSpeed;

        if (!prevBoost && boost) {
            prevVelocity2 = velocity2;
            velocity2Vel = Vector3.zero;
        }

        if (boost) boostTransitionT = Mathf.Min(boostTransitionT + Time.deltaTime, boostTransitionDuration);
        else boostTransitionT = Mathf.Max(boostTransitionT - Time.deltaTime, 0);

        velocity1 = Vector3.SmoothDamp(prevVelocity1, velocity1, ref velocity1Vel, 0.5f, float.PositiveInfinity, Time.deltaTime);
        velocity2 = Vector3.SmoothDamp(prevVelocity2, velocity2, ref velocity2Vel, 0.8f, float.PositiveInfinity, Time.deltaTime);

        float tweenedT = 1 - Mathf.Pow(1 - boostTransitionT, 2);

        Vector3 velocity = Vector3.Lerp(velocity1, velocity2, tweenedT);

        prevVelocity1 = velocity1;
        prevVelocity2 = velocity2;

        return velocity;
    }
}
