using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordVelocitySolver : VelocitySolver
{
    public Mech attacker;
    public Mech victim;

    public bool followVictim = false;
    public Vector3 attackerInitTarget;
    public Vector3 attackerBeginPos;

    public SwordVelocitySolver(Mech attacker, Mech victim) {
        this.attacker = attacker;
        this.victim = victim;

        attackerBeginPos = attacker.transform.position;
        if (victim != null) {
            attackerInitTarget = attacker.GetMeleeAttackPos(victim);
        }
        else {
            // @Temp: Need to be adjusted.
            attackerInitTarget = attacker.transform.position + attacker.transform.forward * 5;
        }
    }

    public Vector3 UpdateSolver(Mech mech, Vector3 input, bool boost, out float smoothTime) {
        if (mech == attacker) {
            Vector3 targetPos;
            if (followVictim && victim != null) targetPos = attacker.GetMeleeAttackPos(victim);
            else targetPos = attackerInitTarget;

            Vector3 fromTo = (targetPos - mech.transform.position);
            Vector3 vel = fromTo * 25;

            smoothTime = 0.02f;
            return vel;
        }
        else {
            smoothTime = 0.05f;
            return Vector3.zero;
        }
    }
}
