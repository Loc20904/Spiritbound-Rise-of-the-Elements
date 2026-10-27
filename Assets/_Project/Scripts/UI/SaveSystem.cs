using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class StoryFlagData { public string name; public bool value; }
[System.Serializable]
public class SavedItem { public string id; public string name; public int healAmount; public Item.ItemType type; }
[System.Serializable]
public class SaveData
{
    public int version;
    public float playerX;
    public float playerY;
    public int playerHP;
    public string currentScene;
    public int tarotIndex = -1;
    public List<StoryFlagData> storyFlags = new List<StoryFlagData>();
    public List<SavedItem> inventory = new List<SavedItem>();
}

public static class SaveSystem
{
    private static SaveRepository Repository => new SaveRepository(Path.Combine(Application.persistentDataPath, "save.json"));
    public static bool SaveGame(Vector3 playerPos, int hp, string sceneName)
    {
        var data = GameSession.Capture(playerPos, hp, sceneName);
        bool saved = Repository.TrySave(data, out string error);
        if (!saved) Debug.LogWarning("Save failed: " + error);
        return saved;
    }
    public static SaveData LoadGame()
    {
        if (!Repository.TryLoad(out var data, out string error))
        {
            if (error != null) Debug.LogWarning("Cannot load save: " + error);
            return null;
        }
        if (!Application.CanStreamedLevelBeLoaded(data.currentScene))
        { Debug.LogWarning("Saved scene is unavailable: " + data.currentScene); return null; }
        if (error != null) Debug.LogWarning(error);
        return data;
    }
    public static bool HasSave() => LoadGame() != null;
    public static bool DeleteSave()
    {
        bool deleted = Repository.TryDelete(out string error);
        if (!deleted) Debug.LogWarning("Cannot delete save: " + error);
        return deleted;
    }
}
