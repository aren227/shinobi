using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleRoot : MonoBehaviour
{
    ParticleSystem[] particleSystems;

    void Awake() {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    void Update() {
        bool canBeRemoved = true;
        foreach (ParticleSystem particleSystem in particleSystems) {
            if (!particleSystem.isStopped) {
                canBeRemoved = false;
                break;
            }
        }

        if (canBeRemoved) {
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}
