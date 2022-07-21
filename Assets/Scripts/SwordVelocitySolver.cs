using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordVelocitySolver : VelocitySolver
{
    public Mech attacker;
    public Mech victim;

    const float maxApproachTime = 0.3f;

    const float smoothTimeAfterImpact = 0.3f;

    public bool approached = false;

    float t = 0;

    float speed;
    Vector3 impactVelocity;
    const float impactSpeed = 10;

    public SwordVelocitySolver(Mech attacker, Mech victim) {
        this.attacker = attacker;
        this.victim = victim;
    }

    public void Begin() {
        // attacker.AddVelocitySolver(this);
        attacker.manualMovement = true;

        // victim.AddVelocitySolver(this);

        Vector3 target = attacker.GetMeleeAttackPos(victim);

        impactVelocity = (victim.transform.position - attacker.transform.position).normalized * impactSpeed;
    }

    public void Finish() {
        attacker.RemoveVelocitySolver(this);
        attacker.manualMovement = false;

        attacker.velocity = victim.velocity;
        attacker.velocityVel = victim.velocityVel;

        victim.RemoveVelocitySolver(this);
    }

    public void Update() {
        approached = true;

        Vector3 target = attacker.GetMeleeAttackPos(victim);

        Vector3 dir = (victim.transform.position - attacker.transform.position).normalized;

        float targetYaw = -Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg + 90;

        attacker.transform.position = target;
        attacker.transform.eulerAngles = new Vector3(0, targetYaw, 0);

        // if (approached) {
        //     attacker.transform.position = target;
        // }


        // if (Vector3.Distance(target, attacker.transform.position) < Mech.meleeAttackDistance) {
        //     approached = true;
        //     impactVelocity = attacker.velocity;
        // }
        // else if (t >= maxApproachTime) {
        //     attacker.transform.position = target;
        //     impactVelocity = attacker.velocity;
        //     approached = true;
        // }

        // if (approached) {
        //     victim.AddVelocitySolver(this);
        // }

        // t += Time.deltaTime;
    }

    public Vector3 UpdateSolver(Mech mech, Vector3 input, bool boost, out float smoothTime) {
        if (mech == attacker) {
            if (approached) {
                smoothTime = smoothTimeAfterImpact;
                return impactVelocity;
            }
            else {
                smoothTime = 0.3f;

                Vector3 target = attacker.GetMeleeAttackPos(victim);
                return (target - attacker.transform.position);
            }
        }
        else {
            smoothTime = smoothTimeAfterImpact;
            Debug.Log(impactVelocity);
            return impactVelocity;
        }
    }
}
