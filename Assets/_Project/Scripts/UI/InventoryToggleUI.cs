using UnityEngine;

public class InventoryToggleUI : MonoBehaviour
{
    public GameObject inventoryPanel;
    public GameObject inventoryButton;
    public GameObject player;
    private bool isOpen;
    private PlayerHealth health;

    private void Start()
    {
        health = player != null ? PlayerHealth.Resolve(player.transform) : FindFirstObjectByType<PlayerHealth>();
        SetOpen(false);
    }
    private void Update()
    {
        if (Time.timeScale > 0f && Input.GetKeyDown(KeyCode.E)) ToggleInventory();
    }
    public void ToggleInventory()
    {
        if (Time.timeScale <= 0f || (health != null && health.IsDead)) return;
        SetOpen(!isOpen);
    }
    private void SetOpen(bool open)
    {
        isOpen = open;
        if (inventoryPanel != null) inventoryPanel.SetActive(open);
        if (inventoryButton != null) inventoryButton.SetActive(!open);
        health?.SetControlsBlocked(this, open);
    }
    private void OnDisable()
    {
        health?.SetControlsBlocked(this, false);
        isOpen = false;
    }
}
