using System.Collections.Generic;
using UnityEngine;

public static class GameSession
{
    public static readonly List<Item> Inventory = new List<Item>();
    private static SaveData pendingRestore;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeState() { Inventory.Clear(); pendingRestore = null; }

    public static void NewGame()
    {
        pendingRestore = null;
        Inventory.Clear();
        GameProgressManager.Instance?.ResetProgress();
        PlayerStatManager.ResetSavedStat();
        PlayerPrefs.DeleteKey("LastScene");
        PlayerPrefs.Save();
    }

    public static void Restore(SaveData data)
    {
        pendingRestore = data;
        GameProgressManager.EnsureInstance().RestoreFlags(data.storyFlags);
        PlayerStatManager.EnsureInstance().RestoreStat(data.version == 0 ? PlayerPrefs.GetInt("PlayerStatIndex", -1) : data.tarotIndex);
        Inventory.Clear();
        var content = GameContent.Load();
        foreach (var saved in data.inventory ?? new List<SavedItem>())
        {
            if (saved == null || string.IsNullOrEmpty(saved.id)) { Inventory.Add(null); continue; }
            var definition = content != null ? content.FindItem(saved.id) : null;
            Inventory.Add(definition != null ? definition.Clone() : new Item {
                itemId = saved.id, itemName = saved.name, healAmount = saved.healAmount, itemType = saved.type });
        }
    }

    // Resume is one-shot. Ordinary scene changes must never reapply stale saved HP or position.
    public static bool TryConsumePlayerRestore(string sceneName, out SaveData data)
    {
        data = null;
        if (pendingRestore == null || pendingRestore.currentScene != sceneName) return false;
        data = pendingRestore;
        pendingRestore = null;
        return true;
    }

    public static SaveData Capture(Vector3 position, int hp, string sceneName)
    {
        var data = new SaveData { version = SaveRepository.CurrentVersion, playerX = position.x,
            playerY = position.y, playerHP = hp, currentScene = sceneName,
            tarotIndex = PlayerStatManager.Instance != null ? PlayerStatManager.Instance.ActiveIndex : -1,
            storyFlags = GameProgressManager.Instance != null ? GameProgressManager.Instance.CaptureFlags() : new List<StoryFlagData>() };
        foreach (var item in Inventory)
            data.inventory.Add(item == null ? null : new SavedItem { id = item.Id, name = item.itemName, healAmount = item.healAmount, type = item.itemType });
        return data;
    }
}
