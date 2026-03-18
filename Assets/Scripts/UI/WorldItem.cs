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
        InventoryManager.Instance.AddItem(itemData);

        InventoryUI ui = FindObjectOfType<InventoryUI>();
        if (ui != null)
            ui.Refresh();

        Destroy(gameObject);
    }
}