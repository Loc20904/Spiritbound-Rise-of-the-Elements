using UnityEngine;

public class ItemTester : MonoBehaviour
{
    [Header("Potion")]
    public Sprite potionIcon;

    [Header("Rune Animation")]
    public Sprite[] runeFrames;

    void Start()
    {
        // 👉 Tạo Potion
        Item potion = new Item();
        potion.itemName = "Potion";
        potion.icon = potionIcon;

        // 👉 Tạo Rune
        Item rune = new Item();
        rune.itemName = "Rune";
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