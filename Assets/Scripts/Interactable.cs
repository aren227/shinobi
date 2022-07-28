using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    public static HashSet<Interactable> interactables = new HashSet<Interactable>();

    public UnityEvent onInteract;

    public float disableUntil = 0;

    public bool isDisabled => disableUntil > Time.time;

    Collider coll;
    const float interactableRadius = 6f;

    public string mainText;
    public string subText;

    void Awake() {
        coll = GetComponentInChildren<Collider>();
    }

    void OnEnable() {
        interactables.Add(this);
    }

    void OnDisable() {
        interactables.Remove(this);
    }

    public void Interact() {
        if (isDisabled) return;
        onInteract.Invoke();
    }

    public static Interactable GetInteractable(Vector3 pos) {
        float minDist = interactableRadius;
        Interactable min = null;
        foreach (Interactable interactable in interactables) {
            Vector3 closest = interactable.coll.ClosestPoint(pos);
            float dist = Vector3.Distance(pos, closest);

            if (minDist > dist) {
                minDist = dist;
                min = interactable;
            }
        }
        return min;
    }
}
