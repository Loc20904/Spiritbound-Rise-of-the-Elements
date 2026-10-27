using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spiritbound/Game Content")]
public class GameContent : ScriptableObject
{
    public PlayerStatDatabase tarotDatabase;
    public List<Item> items = new List<Item>();
    public static GameContent Load() => Resources.Load<GameContent>("SpiritboundContent");
    public Item FindItem(string id) => items.Find(item => item != null && item.Id == id);
}
