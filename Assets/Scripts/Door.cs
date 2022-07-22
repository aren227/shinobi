using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Door : MonoBehaviour
{
    public Collider exitTrigger;
    public List<GameObject> barriers;

    public Transform leftDoor;
    public Transform rightDoor;

    Vector3 leftDoorClosed, leftDoorOpened;
    Vector3 rightDoorClosed, rightDoorOpened;

    void Awake() {
        leftDoorClosed = leftDoor.position;
        leftDoorOpened = leftDoorClosed + Vector3.left * 50;
        rightDoorClosed = rightDoor.position;
        rightDoorOpened = rightDoorClosed + Vector3.right * 50;
    }

    public void RemoveBarrier() {
        foreach (GameObject obj in barriers) {
            Destroy(obj);
        }
    }

    public void Open() {
        leftDoor.DOMove(leftDoorOpened, 5f);
        rightDoor.DOMove(rightDoorOpened, 5f);
    }

    public void Close() {
        leftDoor.DOMove(leftDoorClosed, 5f);
        rightDoor.DOMove(rightDoorClosed, 5f);
    }

    public void OnTriggerEnter(Collider other) {
        Mech mech = other.GetComponentInParent<Mech>();
        if (mech == GameManager.Instance.player) {
            if (GameManager.Instance.state != GameState.COMPLETED) {
                GameManager.Instance.BeginState(GameState.COMPLETED);
            }
        }
    }
}
