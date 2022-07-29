using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowingVelocitySolver : MonoBehaviour, VelocitySolver
{
    public Mech target;

    public TimedCurve speedCurve;

    const float maxSpeed = 80;
    const float reflectSpeed = 50;
    const float fov = 90;
    const float maxAngleDiff = 180;
    const float accelerationSmoothTime = 0f;
    const float cancelSmoothTime = 0.5f;

    public bool canceled = false;
    float timestamp;

    Vector3 dir;
    bool dirInit = false;

    public void Init(Mech target) {
        this.target = target;
        canceled = false;
        timestamp = Time.time;
        dirInit = false;
    }

    public void Reflect(Vector3 normal, Mech from, Mech to) {
        from.velocity = normal * reflectSpeed;
    }

    public Vector3 UpdateSolver(Mech mech, Vector3 input, bool boost, out float smoothTime) {
        if (canceled) {
            smoothTime = cancelSmoothTime;

            return Vector3.zero;
        }
        else {
            smoothTime = accelerationSmoothTime;

            Vector3 targetDir = (target.transform.position - mech.transform.position).normalized;

            if (!dirInit) {
                dir = targetDir;
                dirInit = true;
            }

            // @Copypasta: From Missile.
            float maxAngle = maxAngleDiff * Time.deltaTime;
            float angle = Vector3.Angle(targetDir, dir);
            if (angle > 0.01f && angle < fov) {
                Quaternion q = Quaternion.AngleAxis(-Mathf.Min(maxAngle, Vector3.Angle(targetDir, dir)), Vector3.Cross(targetDir, dir));
                dir = q * dir;
            }
            else if (angle > fov) {
                canceled = true;
            }

            // @Temp
            // TimedCurve speedCurve = mech.GetComponent<AccelerationBasedVelocitySolver>().boostAccelerationCurve;
            // TimedCurve smoothTimeCurve = mech.GetComponent<AccelerationBasedVelocitySolver>().boostAccelerationSmoothTimeCurve;

            // @Copypasta: From AccelerationBasedVelocitySolver.
            float speed = speedCurve.Evaluate(Time.time - timestamp) * maxSpeed;
            smoothTime = 0;

            return dir * speed;
        }
    }
}
