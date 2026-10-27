using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemId;
    public string itemName;
    public string Id => string.IsNullOrEmpty(itemId) ? itemName : itemId;
    public Item Clone() => (Item)MemberwiseClone();

    // dùng cho item tĩnh (Potion)
    public Sprite icon;

    // dùng cho item động (Rune)
    public Sprite[] animationFrames;

    public float animationSpeed = 0.1f;

    public enum ItemType { Consumable, Equipment, Material }
    public ItemType itemType;
    public int healAmount;
}
