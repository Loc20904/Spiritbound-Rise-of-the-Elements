using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    public List<Item> items = new List<Item>();

    public int itemsPerPage = 20;
    public int currentPage = 0;

    void Awake()
    {
        Instance = this;
    }

    public void AddItem(Item item)
    {
        items.Add(item);
    }

    public int TotalPages()
    {
        return Mathf.CeilToInt((float)items.Count / itemsPerPage);
    }

    public void UseItem(Item item)
    {
        if (item.itemType == Item.ItemType.Consumable || item.itemName.Contains("Potion"))
        {
            PlayerHealth playerHealth = FindObjectOfType<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.Heal(item.healAmount > 0 ? item.healAmount : 20); // Default to 20 if healAmount is not set
                items.Remove(item);
                Debug.Log($"Used {item.itemName}. Remaining items: {items.Count}");
            }
        }
    }

    public void UseItem(int index)
    {
        if (index >= items.Count) return;

        Item item = items[index];
        if (item == null) return;

        Debug.Log("Use item: " + item.itemName);

        // 👉 xử lý effect
        ApplyItemEffect(item);

        // 👉 xóa item
        items[index] = null;
    }

    void ApplyItemEffect(Item item)
    {
        if (item.itemName == "Potion")
        {
            // 👉 heal player
            PlayerHealth player = FindObjectOfType<PlayerHealth>();
            if (player != null)
            {
                player.ResetHealth(); // hoặc + máu
            }
        }
        else if (item.itemName == "Rune")
        {
            Debug.Log("Player can Transform!");
            // 👉 thêm skill ở đây
        }
    }
}