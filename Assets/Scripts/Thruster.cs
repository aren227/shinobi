using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    Mech mech;

    public List<ParticleSystem> fireParticleSystems;
    public List<ParticleSystem> smokeParticleSystems;

    public int durability = 50;

    float initialFireRateOverTime;
    float initialSmokeRateOverTime;

    public int health { get; private set; }

    void Awake() {
        mech = GetComponentInParent<Mech>();

        initialFireRateOverTime = fireParticleSystems[0].emission.rateOverTimeMultiplier;
        initialSmokeRateOverTime = smokeParticleSystems[0].emission.rateOverTimeMultiplier;

        health = durability;
    }

    public void Hit(int damage) {
        health = Mathf.Max(health - damage, 0);
    }

    void Update() {
        if (mech.isKilled) {
            Destroy(gameObject);
        }
        else {
            float globalEmissionRate = Mathf.Lerp(0.2f, 1f, mech.velocity.magnitude / 10f);

            foreach (ParticleSystem ps in fireParticleSystems) {
                ParticleSystem.EmissionModule em = ps.emission;
                em.rateOverTimeMultiplier = initialFireRateOverTime * globalEmissionRate * (float)health / durability;
            }

            foreach (ParticleSystem ps in smokeParticleSystems) {
                ParticleSystem.EmissionModule em = ps.emission;
                em.rateOverTimeMultiplier = initialSmokeRateOverTime * globalEmissionRate * (1 - (float)health / durability);
            }
        }
    }
}
