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
}