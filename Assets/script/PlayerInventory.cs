using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<Item> inventory = new List<Item>();

    public void AddItem(Item newItem)
    {
        if (!inventory.Contains(newItem))
        {
            inventory.Add(newItem);
            Debug.Log("Picked up: " + newItem.itemName);
        }
    }
}
