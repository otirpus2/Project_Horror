using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    public List<Item> items = new List<Item>();
    public int maxItems = 20;

    public void AddItem(Item newItem)
    {
        if (items.Count >= maxItems)
        {
            Debug.Log("Inventory full!");
            return;
        }

        items.Add(newItem);
        Debug.Log($"Picked up: {newItem.itemName}");
    }
}
