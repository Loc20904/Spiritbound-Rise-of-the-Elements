using UnityEngine;

public class ItemTester : MonoBehaviour
{
    [Header("Potion")]
    public Sprite potionIcon;

    [Header("Rune Animation")]
    public Sprite[] runeFrames;

    [SerializeField] private bool populateOnStart;
    void Start()
    {
        if (!populateOnStart || InventoryManager.Instance == null || InventoryManager.Instance.items.Count > 0) return;
        // 👉 Tạo Potion
        Item potion = new Item();
        potion.itemName = "Potion";
        potion.healAmount = 20;
        potion.icon = potionIcon;

        // 👉 Tạo Rune
        Item rune = new Item();
        rune.itemName = "Rune";
        rune.itemType = Item.ItemType.Material;
        rune.animationFrames = runeFrames;
        rune.animationSpeed = 0.15f;

        // 👉 Add vào inventory
        InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune);

        InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune); InventoryManager.Instance.AddItem(potion);
        InventoryManager.Instance.AddItem(rune);
    }
}
