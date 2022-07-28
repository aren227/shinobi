using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Terminal : MonoBehaviour
{
    float lastInteractAt;

    Interactable interactable;

    void Awake() {
        interactable = GetComponent<Interactable>();

        interactable.onInteract.AddListener(OnInteract);

        interactable.mainText = "Ready";
        interactable.subText = "[F] Begin boarding process";
    }

    void OnInteract() {
        interactable.disableUntil = Time.time + 3;

        if (GameManager.Instance.state == GameState.PREPARE) {
            GameManager.Instance.BeginState(GameState.FIGHT);
        }
        else if (GameManager.Instance.state == GameState.FIGHT) {
            GameManager.Instance.BeginState(GameState.LEAVE);
        }
    }

    void Update() {
        if (GameManager.Instance.state == GameState.FIGHT) {
            interactable.mainText = "Boarding...";
            if (interactable.isDisabled) interactable.subText = "";
            else interactable.subText = "[F] Launch";
        }
        if (GameManager.Instance.state == GameState.LEAVE) {
            interactable.mainText = "Launching...";
            interactable.subText = "";
        }
        if (GameManager.Instance.state == GameState.FAILED || GameManager.Instance.state == GameState.KILLED) {
            interactable.enabled = false;
        }
    }
}
