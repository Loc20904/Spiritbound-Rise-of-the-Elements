using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public InventorySlot[] slots;

    void Start()
    {
        Invoke(nameof(Init), 0.1f); // delay nhẹ
    }

    void Init()
    {
        slots = GetComponentsInChildren<InventorySlot>();
        Refresh();
    }

    public void Refresh()
    {
        int startIndex = InventoryManager.Instance.currentPage * 20;

        for (int i = 0; i < slots.Length; i++)
        {
            int itemIndex = startIndex + i;

            slots[i].slotIndex = itemIndex;

            if (itemIndex < InventoryManager.Instance.items.Count)
            {
                slots[i].SetItem(InventoryManager.Instance.items[itemIndex]);
            }
            else
            {
                slots[i].SetItem(null);
            }
        }
    }

    public void NextPage()
    {
        int totalPages = InventoryManager.Instance.TotalPages();

        if (InventoryManager.Instance.currentPage < totalPages - 1)
        {
            InventoryManager.Instance.currentPage++;
            Refresh();
        }
    }

    public void PrevPage()
    {
        if (InventoryManager.Instance.currentPage > 0)
        {
            InventoryManager.Instance.currentPage--;
            Refresh();
        }
    }
}