using UnityEngine;

[System.Serializable]
public class Item
{
    public string itemName;

    // dùng cho item tĩnh (Potion)
    public Sprite icon;

    // dùng cho item động (Rune)
    public Sprite[] animationFrames;

    public float animationSpeed = 0.1f;

    public enum ItemType { Consumable, Equipment, Material }
    public ItemType itemType;
    public int healAmount;
}