using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Spaceship : MonoBehaviour
{
    public const int maxHealth = 2000;

    int health;

    public List<GameObject> parts;

    Vector3 farAway;
    Vector3 dest;

    void Awake() {
        health = maxHealth;

        farAway = transform.position + new Vector3(0, 10000, 0);
        dest = transform.position;
    }

    public void Hit(int damage) {
        if (GameManager.Instance.state != GameState.FIGHT) return;

        health = Mathf.Max(health - damage, 0);

        UpdateStatus();

        if (health <= 0) {
            if (GameManager.Instance.state == GameState.FIGHT) {
                GameManager.Instance.BeginState(GameState.FAILED);
            }
        }
    }

    public void UpdateStatus() {
        int step = maxHealth / parts.Count;
        for (int i = 0; i < parts.Count; i++) {
            parts[i].SetActive(health > step * i);
        }
    }

    public void Arrive() {
        transform.position = farAway;

        transform.DOMove(dest, 10).SetEase(Ease.OutCirc);
    }

    public void Depart() {
        transform.position = dest;

        transform.DOMove(farAway, 10).SetEase(Ease.InCirc);
    }

    public void Explode() {
        GameObject fx = Instantiate(ParticleManager.Instance.hugeExplosion);

        fx.transform.position = transform.position;

        PartName[] partNames = new PartName[] {
            PartName.HEAD,
            PartName.LOWER_LEFT_ARM,
            PartName.LOWER_LEFT_LEG,
            PartName.LOWER_RIGHT_ARM,
            PartName.LOWER_RIGHT_LEG,
            PartName.UPPER_LEFT_ARM,
            PartName.UPPER_LEFT_LEG,
            PartName.UPPER_RIGHT_ARM,
            PartName.UPPER_RIGHT_LEG,
            PartName.BODY,
        };

        foreach (Mech mech in GameManager.Instance.meches) {
            foreach (PartName partName in partNames) {
                Part part = mech.skeleton.GetPart(partName);
                part.Hit(100000);
            }
        }

        foreach (Rigidbody rigidbody in FindObjectsOfType<Rigidbody>()) {
            rigidbody.AddExplosionForce(1000, transform.position, 300);
        }
    }
}
