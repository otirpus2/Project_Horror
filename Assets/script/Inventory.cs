using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    public List<Item> items = new List<Item>();
    public int maxItems = 20;
    public Item equippedItem;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Update()
    {
        if (items.Count > 0 && Input.GetKeyDown(KeyCode.Alpha1))
            EquipItem(0);
        if (items.Count > 1 && Input.GetKeyDown(KeyCode.Alpha2))
            EquipItem(1);
        if (items.Count > 2 && Input.GetKeyDown(KeyCode.Alpha3))
            EquipItem(2);
    }

    public void AddItem(Item newItem)
    {
        if (items.Count >= maxItems)
        {
            Debug.Log("Inventory full!");
            return;
        }

        items.Add(newItem);
        Debug.Log($"Picked up: {newItem.itemName}");

        if (equippedItem == null)
            EquipItem(items.Count - 1);
    }

    void EquipItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        equippedItem = items[index];
        Debug.Log($"Equipped: {equippedItem.itemName}");
    }
}
