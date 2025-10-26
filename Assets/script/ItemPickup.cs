using UnityEngine;
using TMPro;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public float pickupRange = 3f;
    public TextMeshProUGUI pickupText;

    private Transform player;
    private bool isInRange;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= pickupRange)
        {
            if (!isInRange)
            {
                isInRange = true;
                if (pickupText != null)
                {
                    pickupText.text = $"Press E to pick up {item.itemName}";
                    pickupText.gameObject.SetActive(true);
                }
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                Pickup();
            }
        }
        else if (isInRange)
        {
            isInRange = false;
            if (pickupText != null)
                pickupText.gameObject.SetActive(false);
        }
    }

    void Pickup()
    {
        if (pickupText != null)
            pickupText.gameObject.SetActive(false);

        Inventory.instance.AddItem(item);
        Destroy(gameObject);
    }
}
