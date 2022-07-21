using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thruster : MonoBehaviour
{
    Mech mech;

    Damagable damagable;

    public List<ParticleSystem> fireParticleSystems;
    public List<ParticleSystem> smokeParticleSystems;

    float initialFireRateOverTime;
    float initialSmokeRateOverTime;

    Collider collider;

    void Awake() {
        mech = GetComponentInParent<Mech>();

        damagable = GetComponent<Damagable>();

        initialFireRateOverTime = fireParticleSystems[0].emission.rateOverTimeMultiplier;
        initialSmokeRateOverTime = smokeParticleSystems[0].emission.rateOverTimeMultiplier;

        collider = GetComponent<Collider>();
    }

    void Update() {
        if (mech.isKilled) {
            Destroy(gameObject);
        }
        else {
            float globalEmissionRate = Mathf.Lerp(0.2f, 1f, mech.velocity.magnitude / 10f);

            foreach (ParticleSystem ps in fireParticleSystems) {
                ParticleSystem.EmissionModule em = ps.emission;
                em.rateOverTimeMultiplier = initialFireRateOverTime * globalEmissionRate * (float)damagable.health / damagable.maxHealth;
            }

            foreach (ParticleSystem ps in smokeParticleSystems) {
                ParticleSystem.EmissionModule em = ps.emission;
                em.rateOverTimeMultiplier = initialSmokeRateOverTime * globalEmissionRate * (1 - (float)damagable.health / damagable.maxHealth);
            }
        }

        if (damagable.health <= 0) {
            collider.enabled = false;
        }
    }

    public int GetSteminaRequiredToBoost() {
        return Mathf.RoundToInt(Mathf.Lerp(Mech.maxSteminaRequiredToBoost, Mech.minSteminaRequiredToBoost, (float)damagable.health / damagable.maxHealth));
    }
}
