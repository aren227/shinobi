using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundRoot : MonoBehaviour
{
    public AudioSource audioSource;

    void Awake() {
        audioSource = GetComponent<AudioSource>();
    }

    void Update() {
        if (!audioSource.isPlaying) {
            PoolManager.Instance.Despawn(gameObject);
        }
    }
}
