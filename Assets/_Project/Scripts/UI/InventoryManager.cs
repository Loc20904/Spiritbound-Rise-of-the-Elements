using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;
    public List<Item> items = new List<Item>();
    public int itemsPerPage = 20;
    public int currentPage;
    public event System.Action Changed;
    private void Awake() { Instance = this; items = GameSession.Inventory; }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    public void AddItem(Item item)
    {
        if (item == null) return;
        int empty = items.FindIndex(value => value == null);
        if (empty >= 0) items[empty] = item;
        else items.Add(item);
        Changed?.Invoke();
    }
    public int TotalPages() => Mathf.Max(1, Mathf.CeilToInt((float)items.Count / Mathf.Max(1, itemsPerPage)));
    public void UseItem(Item item) { if (item != null) UseItem(items.IndexOf(item)); }
    public void UseItem(int index) => TryUseItem(index);
    public bool TryUseItem(int index)
    {
        if (index < 0 || index >= items.Count || items[index] == null) return false;
        var item = items[index];
        var health = FindFirstObjectByType<PlayerHealth>();
        if (health == null || health.IsDead) return false;
        // Unimplemented material/equipment/rune effects must never consume an item.
        if (item.itemType != Item.ItemType.Consumable || item.healAmount <= 0 || health.CurrentHP >= health.MaxHP) return false;
        health.Heal(item.healAmount);
        items[index] = null;
        Changed?.Invoke();
        return true;
    }
    public bool MoveItem(int from, int to)
    {
        if (from < 0 || from >= items.Count || items[from] == null || to < 0 || to >= TotalPages() * Mathf.Max(1, itemsPerPage)) return false;
        while (items.Count <= to) items.Add(null);
        var item = items[to]; items[to] = items[from]; items[from] = item;
        Changed?.Invoke();
        return true;
    }
}
