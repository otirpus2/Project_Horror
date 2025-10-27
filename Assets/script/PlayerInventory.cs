using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public List<Item> inventory = new List<Item>();

    [Header("Item Holding")]
    public Transform handPosition;       // 👈 assign the empty object under the hand bone
    private GameObject currentItemModel; // the visible item in hand
    public Item currentItem;             // reference to the currently equipped item

    public void AddItem(Item newItem)
    {
        if (!inventory.Contains(newItem))
        {
            inventory.Add(newItem);
            Debug.Log("Picked up: " + newItem.itemName);
        }
    }

    public void EquipItem(Item item)
    {
        if (item == null) return;

        // Remove old item in hand
        if (currentItemModel != null)
            Destroy(currentItemModel);

        currentItem = item;
        Debug.Log("Equipped: " + currentItem.itemName);

        // Spawn the new item prefab in hand
        if (item.itemPrefab != null && handPosition != null)
        {
            currentItemModel = Instantiate(
                item.itemPrefab,
                handPosition.position,
                handPosition.rotation,
                handPosition
            );
        }
        else
        {
            Debug.LogWarning("Missing item prefab or hand position!");
        }
    }
}
