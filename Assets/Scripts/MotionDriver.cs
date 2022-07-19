using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionDriver
{
    public Mech attacker;
    public Mech victim;

    Vector3 victimVelocity;
    Vector3 victimVelocityVel;

    Vector3 attackerPos;
    Vector3 attackerPosVel;

    float attackerYaw;
    float attackerYawVel;

    const float approachTime = 0.1f;

    public bool approached = false;

    float t = 0;

    public MotionDriver(Mech attacker, Mech victim) {
        this.attacker = attacker;
        this.victim = victim;

        victimVelocity = victim.velocity;

        Vector3 target = attacker.GetMeleeAttackPos(victim);

        attackerPos = attacker.transform.position;
        attackerPosVel = target - attacker.transform.position;

        attackerYaw = attacker.yaw;
    }

    public void Update() {
        victimVelocity = Vector3.SmoothDamp(victimVelocity, Vector3.zero, ref victimVelocityVel, 1f);

        victim.transform.position = victim.transform.position + victimVelocity * Time.deltaTime;

        Vector3 target = attacker.GetMeleeAttackPos(victim);

        Vector3 dir = (victim.transform.position - attacker.transform.position).normalized;
        float targetYaw = -Mathf.Atan2(dir.z, dir.x) * Mathf.Rad2Deg + 90;

        if (t < approachTime) {
            attackerPos = Vector3.SmoothDamp(attackerPos, target, ref attackerPosVel, 0.05f);
            attacker.transform.position = attackerPos;

            attackerYaw = Mathf.SmoothDampAngle(attackerYaw, targetYaw, ref attackerYawVel, 0.05f);
            attacker.transform.eulerAngles = new Vector3(0, attackerYaw, 0);
        }
        else {
            attacker.transform.position = target;
            attacker.transform.eulerAngles = new Vector3(0, targetYaw, 0);

            approached = true;
        }

        t += Time.deltaTime;
    }
}
