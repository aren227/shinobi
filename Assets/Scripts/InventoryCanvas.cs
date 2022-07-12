using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCanvas : MonoBehaviour
{
    public GameObject itemBadgePrefab;

    public Transform groundListPivot;
    public Transform leftShoulderPivot;
    public Transform rightShoulderPivot;
    public Transform leftArmPivot;
    public Transform rightArmPivot;
    public Transform leftLegPivot;
    public Transform rightLegPivot;

    Dictionary<Inventory.Slot, Transform> pivots = new Dictionary<Inventory.Slot, Transform>();

    Canvas canvas;

    List<Item> groundItems;
    Inventory inventory;

    int groundSelectedIndex = -1;

    bool open = false;
    bool dirty = true;

    void Awake() {
        canvas = FindObjectOfType<Canvas>();

        pivots.Add(Inventory.Slot.LEFT_SHOULDER, leftShoulderPivot);
        pivots.Add(Inventory.Slot.RIGHT_SHOULDER, rightShoulderPivot);
        pivots.Add(Inventory.Slot.LEFT_ARM, leftArmPivot);
        pivots.Add(Inventory.Slot.RIGHT_ARM, rightArmPivot);
        pivots.Add(Inventory.Slot.LEFT_LEG, leftLegPivot);
        pivots.Add(Inventory.Slot.RIGHT_LEG, rightLegPivot);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.E) && !open) {
            // @Todo: Find near items.
            Open(new List<Item>(), Mech.Player.inventory);
        }
        else if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)) && open) {
            Close();
        }

        if (open && dirty) {
            Render();
            dirty = false;
        }
    }

    public void Open(List<Item> groundItems, Inventory inventory) {
        open = true;

        canvas.enabled = true;

        this.groundItems = groundItems;
        this.inventory = inventory;
    }

    void Render() {
        for (int i = groundListPivot.childCount-1; i >= 0; i--) {
            Destroy(groundListPivot.GetChild(i).gameObject);
        }

        foreach (Transform transform in pivots.Values) {
            for (int i = transform.childCount-1; i >= 0; i--) {
                // @Hardcoded
                if (transform.GetChild(i).gameObject.name == "BG") continue;
                Destroy(transform.GetChild(i).gameObject);
            }
        }

        for (int i = 0; i < groundItems.Count; i++) {
            ItemBadge itemBadge = CreateItemBadge(groundItems[i]);

            itemBadge.transform.SetParent(groundListPivot);
            itemBadge.transform.localPosition = Vector3.zero;

            if (i == groundSelectedIndex) {
                itemBadge.SetHighlightColor(Color.red);
            }

            itemBadge.SetOnClick(() => {
                if (groundSelectedIndex == i) {
                    groundSelectedIndex = -1;
                }
                else {
                    groundSelectedIndex = i;
                }
            });
        }

        foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
            ItemBadge itemBadge = CreateItemBadge(inventory.GetItem(slot));
            if (itemBadge) {
                itemBadge.transform.SetParent(pivots[slot]);
                itemBadge.transform.localPosition = Vector3.zero;
                itemBadge.SetOnClick(() => {
                    if (groundSelectedIndex != -1) {
                        inventory.SetItem(groundItems[groundSelectedIndex], slot);
                        groundItems.RemoveAt(groundSelectedIndex);
                    }
                    else {
                        inventory.SetItem(null, slot);
                    }

                    dirty = true;
                });
            }
        }
    }

    ItemBadge CreateItemBadge(Item item) {
        if (item == null) return null;

        GameObject cloned = Instantiate(itemBadgePrefab);
        ItemBadge itemBadge = cloned.GetComponent<ItemBadge>();
        itemBadge.SetItem(item);
        return itemBadge;
    }

    List<Item> Close() {
        open = false;

        canvas.enabled = false;

        return groundItems;
    }
}
