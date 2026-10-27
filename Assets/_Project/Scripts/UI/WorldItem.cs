using UnityEngine;

public class WorldItem : MonoBehaviour
{
    public Item itemData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Pickup();
        }
    }

    void Pickup()
    {
        if (InventoryManager.Instance == null || itemData == null || string.IsNullOrEmpty(itemData.Id)) return;
        InventoryManager.Instance.AddItem(itemData.Clone());

        InventoryUI ui = FindFirstObjectByType<InventoryUI>();
        if (ui != null)
            ui.Refresh();

        Destroy(gameObject);
    }
}
