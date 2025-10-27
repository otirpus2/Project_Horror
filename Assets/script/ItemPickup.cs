using UnityEngine;
using TMPro;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    public Item item;
    public float pickupRange = 3f;
    public bool isInteractable = false;

    [Header("UI")]
    public TextMeshProUGUI pickupText;

    [Header("Glow Effect")]
    public Color glowColor = new Color(1f, 0.8f, 0.2f, 1f);
    public float glowIntensity = 2f;
    public float pulseSpeed = 2f;

    [Header("Inspect Settings")]
    public float inspectDistance = 2f;
    public float inspectRotationSpeed = 100f;
    public float zoomSpeed = 5f;

    private Transform player;
    private Camera playerCamera;
    private PlayerController playerController;
    private bool isInRange;
    private bool isInspecting;

    private Material glowMaterial;
    private Renderer[] renderers;
    private Material[][] originalMaterials;
    private float glowTimer;

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Transform originalParent;
    private Rigidbody rb;
    private Collider col;
    private bool hadGravity;
    private bool wasKinematic;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerCamera = Camera.main;
        playerController = player.GetComponent<PlayerController>();

        if (pickupText != null)
            pickupText.gameObject.SetActive(false);

        SetupGlowEffect();

        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    void SetupGlowEffect()
    {
        renderers = GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
            originalMaterials[i] = renderers[i].materials;

        glowMaterial = new Material(Shader.Find("Standard"));
        glowMaterial.EnableKeyword("_EMISSION");
        glowMaterial.SetColor("_EmissionColor", glowColor * glowIntensity);
    }

    void Update()
    {
        if (player == null) return;

        if (isInspecting)
        {
            HandleInspectMode();
            return;
        }

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= pickupRange)
        {
            if (!isInRange)
            {
                isInRange = true;
                EnableGlow(true);
                if (pickupText != null)
                {
                    pickupText.text = $"[E] Pick up {item.itemName}  |  [Q] Inspect";
                    pickupText.gameObject.SetActive(true);
                }
            }

            AnimateGlow();

            if (Input.GetKeyDown(KeyCode.E))
                Pickup();
            else if (Input.GetKeyDown(KeyCode.Q))
                StartInspect();
        }
        else if (isInRange)
        {
            isInRange = false;
            EnableGlow(false);
            if (pickupText != null)
                pickupText.gameObject.SetActive(false);
        }
    }

    void AnimateGlow()
    {
        glowTimer += Time.deltaTime * pulseSpeed;
        float pulse = (Mathf.Sin(glowTimer) + 1f) / 2f;
        float currentIntensity = Mathf.Lerp(glowIntensity * 0.5f, glowIntensity * 1.5f, pulse);

        if (glowMaterial != null)
            glowMaterial.SetColor("_EmissionColor", glowColor * currentIntensity);
    }

    void EnableGlow(bool enable)
    {
        if (renderers == null || glowMaterial == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (enable)
            {
                Material[] mats = new Material[originalMaterials[i].Length + 1];
                originalMaterials[i].CopyTo(mats, 0);
                mats[mats.Length - 1] = glowMaterial;
                renderers[i].materials = mats;
            }
            else
                renderers[i].materials = originalMaterials[i];
        }
    }

    void StartInspect()
    {
        isInspecting = true;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalParent = transform.parent;

        if (rb != null)
        {
            hadGravity = rb.useGravity;
            wasKinematic = rb.isKinematic;
            rb.useGravity = false;
            rb.isKinematic = true;
        }

        if (col != null)
            col.enabled = false;

        if (pickupText != null)
            pickupText.text = isInteractable
                ? "[Drag] Rotate  |  [Click] Interact  |  [E] Pick up  |  [Q] Exit"
                : "[Drag] Rotate  |  [E] Pick up  |  [Q] Exit";

        if (playerController != null)
            playerController.SetInspectMode(true);

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    void HandleInspectMode()
    {
        Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * inspectDistance;
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * zoomSpeed);

        if (Input.GetMouseButton(0))
        {
            float rotX = Input.GetAxis("Mouse X") * inspectRotationSpeed * Time.deltaTime;
            float rotY = Input.GetAxis("Mouse Y") * inspectRotationSpeed * Time.deltaTime;

            transform.Rotate(playerCamera.transform.up, -rotX, Space.World);
            transform.Rotate(playerCamera.transform.right, rotY, Space.World);
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        inspectDistance -= scroll * zoomSpeed;
        inspectDistance = Mathf.Clamp(inspectDistance, 1f, 5f);

        AnimateGlow();

        if (Input.GetKeyDown(KeyCode.Q))
            ExitInspect();

        if (Input.GetKeyDown(KeyCode.E))
            Pickup();

        if (Input.GetMouseButtonDown(0) && isInteractable)
            OnItemInteract();
    }

    void OnItemInteract()
    {
        Debug.Log($"Interacted with {item.itemName}!");
    }

    void ExitInspect()
    {
        isInspecting = false;
        StartCoroutine(ReturnToOriginalPosition());

        if (rb != null)
        {
            rb.isKinematic = wasKinematic;
            rb.useGravity = hadGravity;
        }

        if (col != null)
            col.enabled = true;

        if (pickupText != null)
            pickupText.text = $"[E] Pick up {item.itemName}  |  [Q] Inspect";

        if (playerController != null)
            playerController.SetInspectMode(false);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    IEnumerator ReturnToOriginalPosition()
    {
        float elapsed = 0f;
        float duration = 0.3f;
        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            transform.position = Vector3.Lerp(startPos, originalPosition, t);
            transform.rotation = Quaternion.Slerp(startRot, originalRotation, t);

            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.parent = originalParent;
    }

    void Pickup()
{
    if (pickupText != null)
        pickupText.gameObject.SetActive(false);

    EnableGlow(false);

    if (isInspecting && playerController != null)
        playerController.SetInspectMode(false);

    // DEBUG: print what item we are about to add
    Debug.Log($"ItemPickup.Pickup() -> adding Item asset: {item.itemName} | prefab: {(item.itemPrefab ? item.itemPrefab.name : "NULL")}, assetPath: {GetItemAssetPath(item)}");

    Inventory.instance.AddItem(item);
    Destroy(gameObject);
}

string GetItemAssetPath(Item i)
{
#if UNITY_EDITOR
    return UnityEditor.AssetDatabase.GetAssetPath(i);
#else
    return i != null ? i.name : "NULL";
#endif
}


    void OnDestroy()
    {
        if (glowMaterial != null)
            Destroy(glowMaterial);

        if (isInspecting && playerController != null)
            playerController.SetInspectMode(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickupRange);
    }
}
