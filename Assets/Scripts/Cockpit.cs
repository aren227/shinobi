using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cockpit : MonoBehaviour
{
    Mech mech;

    public int durability = 100;
    int health;

    UiManager uiManager;

    void Awake() {
        mech = GetComponentInParent<Mech>();

        health = durability;

        uiManager = FindObjectOfType<UiManager>();
    }

    public void Hit(int damage) {
        health = Mathf.Max(health - damage, 0);

        if (mech == Mech.Player) uiManager.SetCockpitHealth((float) health / durability);

        if (health <= 0) {
            mech.Kill();
        }
    }
}
