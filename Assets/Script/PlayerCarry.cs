using UnityEngine;
using UnityEngine.UI;

public class PlayerCarry : MonoBehaviour
{
    public Transform dropPoint;

    // UI
    public Image heldItemIcon;
    public Text heldItemName;

    private CarryableItem nearbyItem;
    private CarryableItem heldItem;
    private PlayerAudio playerAudio;

    void Start()
    {
        playerAudio = GetComponent<PlayerAudio>();

        if (heldItemIcon != null)
            heldItemIcon.gameObject.SetActive(false);

        if (heldItemName != null)
            heldItemName.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        return;

        // F = Ambil / Tukar Item
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (nearbyItem != null)
            {
                if (heldItem != null)
                {
                    heldItem.transform.position = dropPoint.position;
                    heldItem.gameObject.SetActive(true);
                }

                PickupItem();
            }
        }

        // G = Drop Item
        if (Input.GetKeyDown(KeyCode.G))
        {
            if (heldItem != null)
            {
                DropItem();
            }
        }
    }

    void PickupItem()
    {
        if (nearbyItem == null)
            return;

        heldItem = nearbyItem;

        heldItem.HidePrompt();

        // Tampilkan icon
        if (heldItemIcon != null)
        {
            heldItemIcon.sprite = heldItem.itemIcon;
            heldItemIcon.gameObject.SetActive(true);
        }

        // Tampilkan nama item
        if (heldItemName != null)
        {
            heldItemName.text = heldItem.itemName;
            heldItemName.gameObject.SetActive(true);
        }

        heldItem.gameObject.SetActive(false);

        nearbyItem = null;

        if (playerAudio != null)
            playerAudio.PlayPickup();

        Debug.Log("Mengambil item");
    }

    void DropItem()
    {
        heldItem.transform.position = dropPoint.position;

        heldItem.gameObject.SetActive(true);

        // Sembunyikan icon
        if (heldItemIcon != null)
            heldItemIcon.gameObject.SetActive(false);

        // Sembunyikan nama
        if (heldItemName != null)
            heldItemName.gameObject.SetActive(false);

        if (playerAudio != null)
            playerAudio.PlayPickup();

        Debug.Log("Menjatuhkan item");

        heldItem = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CarryableItem item = other.GetComponent<CarryableItem>();

        if (item != null)
        {
            nearbyItem = item;
            item.ShowPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        CarryableItem item = other.GetComponent<CarryableItem>();

        if (item != null)
        {
            item.HidePrompt();

            if (nearbyItem == item)
                nearbyItem = null;
        }
    }
}
