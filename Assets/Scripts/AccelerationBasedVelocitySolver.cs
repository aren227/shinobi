using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerationBasedVelocitySolver : MonoBehaviour, VelocitySolver
{
    const float walkSpeed = 10;
    const float boostSpeed = 30;

    Vector3 velocity;
    Vector3 velocityVel;

    bool prevBoost = false;

    public TimedCurve boostAccelerationCurve;
    public TimedCurve boostAccelerationSmoothTimeCurve;
    float boostT;

    public Vector3 Update(Vector3 input, bool boost) {
        float speed = walkSpeed;
        float smoothTime = 0.5f;

        if (!prevBoost && boost) {
            boostT = 0;
        }

        if (boost) {
            speed = boostAccelerationCurve.Evaluate(boostT) * boostSpeed;
            smoothTime = boostAccelerationSmoothTimeCurve.Evaluate(boostT) * 0.5f;
            boostT += Time.deltaTime;
        }

        prevBoost = boost;

        Vector3 velocityTarget = input * speed;
        velocity = Vector3.SmoothDamp(velocity, velocityTarget, ref velocityVel, smoothTime);

        // if (input.sqrMagnitude > 0) {
        //     velocity += input * 5f * Time.deltaTime;
        //     if (boost) velocity += input * 20f * Time.deltaTime;
        // }
        // else {
        //     float[] xyz = new float[] { velocity.x, velocity.y, velocity.z };
        //     for (int i = 0; i < 3; i++) {
        //         if (Mathf.Abs(xyz[i]) > 0.05f) xyz[i] -= xyz[i] * 7f * Time.deltaTime;
        //         else xyz[i] = 0;
        //     }
        //     velocity = new Vector3(xyz[0], xyz[1], xyz[2]);
        // }

        // if (boost) {
        //     if (velocity.magnitude > boostSpeed) velocity = velocity.normalized * boostSpeed;
        // }
        // else {
        //     if (velocity.magnitude > walkSpeed) velocity = velocity.normalized * walkSpeed;
        // }

        return velocity;
    }
}
