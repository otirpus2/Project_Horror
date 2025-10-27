using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory instance;

    [Header("Inventory Settings")]
    public List<Item> items = new List<Item>();
    public int maxItems = 20;
    public Item equippedItem;

    [Header("References")]
    public Transform handPosition; // try to assign in inspector (but we also auto-find)
    private GameObject currentEquippedObject; // stores the spawned object

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            Debug.Log("✅ Inventory instance set to: " + gameObject.name);
        }
        else if (instance != this)
        {
            Debug.LogWarning("⚠️ Duplicate Inventory found on: " + gameObject.name + " — destroying this one.");
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // If handPosition wasn't assigned in the inspector, try to find one automatically.
        if (handPosition == null)
        {
            Debug.LogWarning("handPosition not assigned in inspector on " + gameObject.name + ". Attempting auto-find...");
            TryAutoFindHandPoint();
        }
        else
        {
            Debug.Log("handPosition assigned to: " + handPosition.name + " on " + gameObject.name);
        }
    }

    void TryAutoFindHandPoint()
    {
        // 1) Try to find GameObject tagged "Player"
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("Auto-find failed: No GameObject tagged 'Player' found in scene.");
            return;
        }

        // 2) list candidate names to search for (common Mixamo / humanoid names)
        string[] candidates =
        {
            "HandPoint",
            "hand_R",
            "hand_r",
            "RightHand",
            "Mixamorig:RightHand",
            "mixamorig:RightHand",
            "mixamorig:RightHand",
            "mixamorig_RightHand"
        };

        foreach (var name in candidates)
        {
            Transform found = FindChildRecursive(player.transform, name);
            if (found != null)
            {
                handPosition = found;
                Debug.Log("Auto-find success: handPosition set to '" + found.name + "' (path: " + GetTransformPath(found) + ")");
                return;
            }
        }

        // 3) fallback: create a temporary HandPoint under player (so system can still function)
        Transform fallback = new GameObject("HandPoint_Auto").transform;
        fallback.SetParent(player.transform, false);
        fallback.localPosition = new Vector3(0.4f, 1.0f, 0.4f); // a reasonable default in front/right of player
        fallback.localRotation = Quaternion.identity;
        handPosition = fallback;
        Debug.LogWarning("Auto-find fallback: created HandPoint_Auto under Player at " + GetTransformPath(handPosition) +
                         ". You should replace this with a proper bone (RightHand) for accurate placement.");
    }

    static Transform FindChildRecursive(Transform parent, string name)
    {
        if (parent.name == name) return parent;
        foreach (Transform child in parent)
        {
            Transform found = FindChildRecursive(child, name);
            if (found != null) return found;
        }
        return null;
    }

    static string GetTransformPath(Transform t)
    {
        string path = t.name;
        Transform cur = t.parent;
        while (cur != null)
        {
            path = cur.name + "/" + path;
            cur = cur.parent;
        }
        return path;
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
        Debug.Log($"Picked up: {newItem.itemName} (Item asset = {GetObjectPath(newItem)})");

        if (equippedItem == null)
            EquipItem(items.Count - 1);

        FindObjectOfType<InventoryUI>()?.RefreshUI();

    }

    public void EquipItem(int index)
    {
        if (index < 0 || index >= items.Count) return;

        Debug.Log("⚙️ EquipItem() called on object: " + gameObject.name + " for index: " + index);

        equippedItem = items[index];

        // destroy old equipped model if any
        if (currentEquippedObject != null)
            Destroy(currentEquippedObject);

        // detailed diagnostics
        string handName = handPosition != null ? GetTransformPath(handPosition) : "NULL";
        string itemName = equippedItem != null ? equippedItem.itemName : "NULL";
        string prefabName = (equippedItem != null && equippedItem.itemPrefab != null) ? equippedItem.itemPrefab.name : "NULL";

        Debug.Log($"-> handPosition = {handName} | equippedItem = {itemName} | itemPrefab = {prefabName}");

        if (handPosition == null || equippedItem == null || equippedItem.itemPrefab == null)
        {
            Debug.LogWarning("No itemPrefab or handPosition assigned! (Equip aborted)\n" +
                             $"Details -> handPosition: {(handPosition == null ? "NULL" : handPosition.name)}, " +
                             $"equippedItem: {(equippedItem == null ? "NULL" : equippedItem.itemName)}, " +
                             $"itemPrefab: {(equippedItem == null || equippedItem.itemPrefab == null ? "NULL" : equippedItem.itemPrefab.name)}");
            return;
        }

        // Spawn new item in player hand (parented)
        currentEquippedObject = Instantiate(equippedItem.itemPrefab, handPosition);
        currentEquippedObject.transform.localPosition = Vector3.zero;
        currentEquippedObject.transform.localRotation = Quaternion.identity;
        Debug.Log($"Equipped: {equippedItem.itemName} -> Spawned prefab: {equippedItem.itemPrefab.name} at {GetTransformPath(handPosition)}");
    FindObjectOfType<InventoryUI>()?.RefreshUI();
}

    // helper to show path of ScriptableObject in project for logs (best effort)
    static string GetObjectPath(Object obj)
    {
        if (obj == null) return "NULL";
#if UNITY_EDITOR
        return UnityEditor.AssetDatabase.GetAssetPath(obj);
#else
        return obj.name;
#endif
    }
}
