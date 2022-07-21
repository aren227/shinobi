using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryCanvas2 : MonoBehaviour
{
    public ItemView left, right;

    public RenderTextureImage playerImage;

    public GameObject uiSphere;

    Canvas canvas;

    public bool open = false;

    bool closeAfterSelect = false;

    Item picked;

    Camera rtCamera;

    Dictionary<Inventory.Slot, GameObject> uiSpheres = new Dictionary<Inventory.Slot, GameObject>();

    List<Inventory.Slot> selectableSlots = new List<Inventory.Slot>();

    Inventory.Slot cursor;

    void Awake() {
        canvas = GetComponent<Canvas>();

        rtCamera = GameObject.Find("RT Camera").GetComponent<Camera>();
    }

    void Update() {
        if (open) {
            if (Input.GetKeyDown(KeyCode.W)) {
                SetCursor(selectableSlots[(selectableSlots.IndexOf(cursor)+1)%selectableSlots.Count]);
            }

            if (Input.GetKeyDown(KeyCode.X)) {
                if (GameManager.Instance.player.inventory.GetItem(cursor) != null) {
                    GameManager.Instance.player.Unequip(cursor);
                }
            }
            if (picked != null && Input.GetKeyDown(KeyCode.Space)) {
                if (GameManager.Instance.player.inventory.GetItem(cursor) != null) {
                    GameManager.Instance.player.Unequip(cursor);
                }
                GameManager.Instance.player.Equip(picked, cursor);
                picked = null;

                if (closeAfterSelect) {
                    Close();
                    return;
                }
            }

            // Render textures.
            rtCamera.enabled = true;

            // @Todo: Refactor
            {
                GameObject playerModel = GameManager.Instance.player.model;

                Vector3 prevPos = playerModel.transform.localPosition;
                Quaternion prevRot = playerModel.transform.localRotation;
                Vector3 prevScale = playerModel.transform.localScale;

                // @Hardcoded
                playerModel.transform.position = rtCamera.transform.position + new Vector3(0, -1.5f, 6);
                playerModel.transform.rotation = Quaternion.Euler(0, 180, 0);

                rtCamera.targetTexture = playerImage.renderTexture;

                // @Todo: For performance reason, we have to render this object ONLY.
                // But idk just do it for now.
                rtCamera.Render();

                playerModel.transform.localPosition = prevPos;
                playerModel.transform.localRotation = prevRot;
                playerModel.transform.localScale = prevScale;
            }
            if (picked != null) {
                Item item = picked;

                GameObject itemModel = item.model;

                Vector3 prevPos = itemModel.transform.localPosition;
                Quaternion prevRot = itemModel.transform.localRotation;
                Vector3 prevScale = itemModel.transform.localScale;

                // @Hardcoded
                itemModel.transform.position = rtCamera.transform.position + new Vector3(0, 0, 3);
                itemModel.transform.rotation = Quaternion.Euler(0, 90, 0);

                rtCamera.targetTexture = left.itemImage.renderTexture;

                // @Todo: For performance reason, we have to render this object ONLY.
                // But idk just do it for now.
                rtCamera.Render();

                itemModel.transform.localPosition = prevPos;
                itemModel.transform.localRotation = prevRot;
                itemModel.transform.localScale = prevScale;
            }
            if (GameManager.Instance.player.inventory.GetItem(cursor) != null) {
                Item item = GameManager.Instance.player.inventory.GetItem(cursor);

                GameObject itemModel = item.model;

                Vector3 prevPos = itemModel.transform.localPosition;
                Quaternion prevRot = itemModel.transform.localRotation;
                Vector3 prevScale = itemModel.transform.localScale;

                // @Hardcoded
                itemModel.transform.position = rtCamera.transform.position + new Vector3(0, 0, 3);
                itemModel.transform.rotation = Quaternion.Euler(0, 90, 0);

                rtCamera.targetTexture = right.itemImage.renderTexture;

                // @Todo: For performance reason, we have to render this object ONLY.
                // But idk just do it for now.
                rtCamera.Render();

                itemModel.transform.localPosition = prevPos;
                itemModel.transform.localRotation = prevRot;
                itemModel.transform.localScale = prevScale;
            }

            rtCamera.enabled = false;
        }

        if (Input.GetKeyDown(KeyCode.Tab) && !open) {
            Open(null);
        }
        else if ((Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape)) && open) {
            Close();
        }
    }

    public void Open(Item picked) {
        Time.timeScale = 0;

        open = true;

        canvas.enabled = true;

        if (picked != null) left.SetItem(picked);
        else left.SetItem(null);

        right.SetItem(null);

        closeAfterSelect = (picked != null);

        Mech mech = GameManager.Instance.player;

        selectableSlots.Clear();

        foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
            if (picked == null
                || picked.equipAt == EquipAt.HANDHELD && (
                    slot == Inventory.Slot.LEFT_HAND || slot == Inventory.Slot.RIGHT_HAND
                )
                || picked.equipAt == EquipAt.AUXILIARY && (
                    slot == Inventory.Slot.LEFT_SHOULDER || slot == Inventory.Slot.LEFT_ARM
                    || slot == Inventory.Slot.LEFT_LEG || slot == Inventory.Slot.RIGHT_SHOULDER
                    || slot == Inventory.Slot.RIGHT_ARM || slot == Inventory.Slot.RIGHT_LEG
                )) selectableSlots.Add(slot);
        }

        this.picked = picked;

        foreach (Inventory.Slot slot in selectableSlots) {
            Transform pivot = mech.skeleton.GetPivot(slot);

            GameObject cloned = Instantiate(uiSphere, pivot.position, Quaternion.identity);
            cloned.transform.parent = pivot;

            uiSpheres[slot] = cloned;
        }

        // @Temp: Just for resetting colors.
        // @Inefficient
        foreach (Inventory.Slot slot in selectableSlots) {
            SetCursor(slot);
        }

        SetCursor(selectableSlots[0]);
    }

    void SetCursor(Inventory.Slot next) {
        if (uiSpheres.ContainsKey(cursor)) {
            // @Hardcoded, @Inefficient
            uiSpheres[cursor].GetComponent<MeshRenderer>().material.color = Color.blue;
        }
        if (uiSpheres.ContainsKey(next)) {
            // @Hardcoded, @Inefficient
            uiSpheres[next].GetComponent<MeshRenderer>().material.color = Color.red;
        }
        cursor = next;

        Inventory inventory = GameManager.Instance.player.inventory;

        right.SetItem(inventory.GetItem(next));
    }

    public void Close() {
        Time.timeScale = 1;

        open = false;

        canvas.enabled = false;

        foreach (GameObject obj in uiSpheres.Values) {
            Destroy(obj);
        }

        uiSpheres.Clear();
    }
}
