using UnityEngine;

public class InventoryToggleUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject inventoryButton;
    public GameObject player;

    private bool isOpen = false;

    void Start()
    {
        inventoryPanel.SetActive(false);
        inventoryButton.SetActive(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
{
    isOpen = !isOpen;

    inventoryPanel.SetActive(isOpen);
    inventoryButton.SetActive(!isOpen);

    // 👉 khóa di chuyển
    if (player != null)
        player.GetComponent<PlayerController>().enabled = !isOpen;
}
}