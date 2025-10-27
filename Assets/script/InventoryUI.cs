using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    public Inventory inventory;         // assign InventoryManager (or leave null to auto-find)
    public GameObject slotPrefab;       // assign prefab (SlotPrefab)
    public Transform slotContainer;     // assign SlotContainer (a RectTransform)

    private List<GameObject> currentSlots = new List<GameObject>();

    void Start()
    {
        if (inventory == null)
            inventory = Inventory.instance;

        if (inventory == null)
            Debug.LogWarning("InventoryUI: Inventory.instance is null. Assign Inventory in Inspector.");

        RefreshUI();
    }

    public void RefreshUI()
    {
        if (inventory == null || slotPrefab == null || slotContainer == null)
        {
            Debug.LogWarning("InventoryUI.RefreshUI: Missing references. inventory:" + (inventory==null) +
                             " slotPrefab:" + (slotPrefab==null) + " slotContainer:" + (slotContainer==null));
            return;
        }

        // Clear old slots
        foreach (GameObject s in currentSlots)
            Destroy(s);
        currentSlots.Clear();

        // Create slots
        for (int i = 0; i < inventory.items.Count; i++)
        {
            Item item = inventory.items[i];
            GameObject slot = Instantiate(slotPrefab, slotContainer);
            slot.SetActive(true);

            // Try find UI elements
            Image icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            TextMeshProUGUI nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
            Image bg = slot.GetComponent<Image>();

            if (icon != null) icon.sprite = item.icon;
            if (nameText != null) nameText.text = item.itemName;

            // Highlight equipped
            if (inventory.equippedItem == item)
            {
                if (bg != null) bg.color = new Color(0.2f, 0.6f, 1f, 0.9f); // highlight color
                slot.transform.localScale = Vector3.one * 1.05f;
            }
            else
            {
                if (bg != null) bg.color = Color.white;
                slot.transform.localScale = Vector3.one;
            }

            // Optionally give slot a button behavior to equip on click
            Button btn = slot.GetComponent<Button>();
            int index = i;
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => inventory.EquipItem(index));
            }

            currentSlots.Add(slot);
        }
    }
}
