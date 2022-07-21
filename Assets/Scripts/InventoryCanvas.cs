using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryCanvas : MonoBehaviour
{
    public GameObject itemBadgePrefab;

    public Transform pickedPivot;
    public Transform leftShoulderPivot;
    public Transform rightShoulderPivot;
    public Transform leftArmPivot;
    public Transform rightArmPivot;
    public Transform leftLegPivot;
    public Transform rightLegPivot;
    public Transform rightHandPivot;
    public Transform leftHandPivot;
    public Transform swordPivot;

    Dictionary<Inventory.Slot, Transform> pivots = new Dictionary<Inventory.Slot, Transform>();

    List<ItemBadge> badges = new List<ItemBadge>();

    Canvas canvas;

    int cursor = 0;
    int selected = -1;

    Item picked;
    Inventory inventory;

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
        pivots.Add(Inventory.Slot.LEFT_HAND, leftHandPivot);
        pivots.Add(Inventory.Slot.RIGHT_HAND, rightHandPivot);
        pivots.Add(Inventory.Slot.SWORD, swordPivot);

        foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
            ItemBadge itemBadge = CreateItemBadge(null);
            itemBadge.transform.SetParent(pivots[slot]);
            itemBadge.transform.localPosition = Vector3.zero;
            itemBadge.transform.localScale = Vector3.one;

            badges.Add(itemBadge);
        }

        {
            ItemBadge itemBadge = CreateItemBadge(null);
            itemBadge.transform.SetParent(pickedPivot);
            itemBadge.transform.localPosition = Vector3.zero;
            itemBadge.transform.localScale = Vector3.one;

            badges.Add(itemBadge);
        }
    }

    Rect RectTransformToScreenSpace(RectTransform transform)
    {
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        return new Rect((Vector2)transform.position - (size * 0.5f), size);
    }

    void MoveCursor(Vector2Int dir) {
        Rect currRect = RectTransformToScreenSpace(badges[cursor].GetComponent<RectTransform>());

        ItemBadge next = null;
        float minDist = float.PositiveInfinity;
        foreach (ItemBadge badge in badges) {
            if (badges[cursor] == badge) continue;

            Rect rect = RectTransformToScreenSpace(badge.GetComponent<RectTransform>());

            // Not x overlapped.
            if (dir.x != 0 && Mathf.Max(currRect.xMin, rect.xMin) > Mathf.Min(currRect.xMax, rect.xMax)) continue;
            if (dir.x != 0 && Mathf.Sign(rect.xMin - currRect.xMin) != dir.x) continue;
            // Not y overlapped.
            if (dir.y != 0 && Mathf.Max(currRect.yMin, rect.yMin) > Mathf.Min(currRect.yMax, rect.yMax)) continue;
            if (dir.y != 0 && Mathf.Sign(rect.yMin - currRect.yMin) != dir.y) continue;

            if (minDist > Vector2.Distance(new Vector2(currRect.xMin, currRect.yMin), new Vector2(rect.xMin, rect.yMin))) {
                next = badge;
                minDist = Vector2.Distance(new Vector2(currRect.xMin, currRect.yMin), new Vector2(rect.xMin, rect.yMin));
            }
        }

        if (next != null) {
            SetCursor(badges.IndexOf(next));
        }
    }

    void Update() {
        if (open) {
            if (Input.GetKeyDown(KeyCode.W)) MoveCursor(Vector2Int.up);
            else if (Input.GetKeyDown(KeyCode.S)) MoveCursor(Vector2Int.down);
            else if (Input.GetKeyDown(KeyCode.A)) MoveCursor(Vector2Int.left);
            else if (Input.GetKeyDown(KeyCode.D)) MoveCursor(Vector2Int.right);
        }

        if (Input.GetKeyDown(KeyCode.E) && !open) {
            Open(null, GameManager.Instance.player.inventory);
        }
        else if ((Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape)) && open) {
            Close();
        }

        // if (open && dirty) {
        //     Render();
        //     dirty = false;
        // }
    }

    public void Open(Item picked, Inventory inventory) {
        open = true;

        canvas.enabled = true;

        this.picked = picked;
        this.inventory = inventory;

        foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
            badges[(int)slot].SetItem(inventory.GetItem(slot));
        }
        badges[badges.Count-1].SetItem(picked);

        foreach (ItemBadge itemBadge in badges) {
            itemBadge.SetHovered(false);
            itemBadge.SetSelected(false);
        }

        cursor = -1;
        selected = -1;

        SetCursor(0);
    }

    void SetCursor(int next) {
        if (cursor != -1) {
            badges[cursor].SetHovered(false);
        }
        if (next != -1) {
            badges[next].SetHovered(true);
        }
        cursor = next;
    }

    void SetSelected(int next) {
        if (selected != -1) {
            badges[selected].SetSelected(false);
        }
        if (next != -1) {
            badges[next].SetSelected(true);
        }
        selected = next;
    }

    // void Render() {
    //     for (int i = groundListPivot.childCount-1; i >= 0; i--) {
    //         Destroy(groundListPivot.GetChild(i).gameObject);
    //     }

    //     foreach (Transform transform in pivots.Values) {
    //         for (int i = transform.childCount-1; i >= 0; i--) {
    //             // @Hardcoded
    //             if (transform.GetChild(i).gameObject.name == "BG") continue;
    //             Destroy(transform.GetChild(i).gameObject);
    //         }
    //     }

    //     for (int i = 0; i < groundItems.Count; i++) {
    //         ItemBadge itemBadge = CreateItemBadge(groundItems[i]);

    //         itemBadge.transform.SetParent(groundListPivot);
    //         itemBadge.transform.localPosition = Vector3.zero;

    //         if (i == groundSelectedIndex) {
    //             itemBadge.SetHighlightColor(Color.red);
    //         }

    //         itemBadge.SetOnClick(() => {
    //             if (groundSelectedIndex == i) {
    //                 groundSelectedIndex = -1;
    //             }
    //             else {
    //                 groundSelectedIndex = i;
    //             }
    //         });
    //     }

    //     foreach (Inventory.Slot slot in System.Enum.GetValues(typeof(Inventory.Slot))) {
    //         ItemBadge itemBadge = CreateItemBadge(inventory.GetItem(slot));
    //         if (itemBadge) {
    //             itemBadge.transform.SetParent(pivots[slot]);
    //             itemBadge.transform.localPosition = Vector3.zero;
    //             itemBadge.SetOnClick(() => {
    //                 if (groundSelectedIndex != -1) {
    //                     inventory.SetItem(groundItems[groundSelectedIndex], slot);
    //                     groundItems.RemoveAt(groundSelectedIndex);
    //                 }
    //                 else {
    //                     inventory.SetItem(null, slot);
    //                 }

    //                 dirty = true;
    //             });
    //         }
    //     }
    // }

    ItemBadge CreateItemBadge(Item item) {
        GameObject cloned = Instantiate(itemBadgePrefab);
        ItemBadge itemBadge = cloned.GetComponent<ItemBadge>();
        itemBadge.SetItem(item);
        return itemBadge;
    }

    Item Close() {
        open = false;

        canvas.enabled = false;

        return picked;
    }
}
