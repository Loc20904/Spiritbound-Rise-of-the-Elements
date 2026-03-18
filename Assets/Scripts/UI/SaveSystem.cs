using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public float playerX;
    public float playerY;
    public int playerHP;
}

public class SaveSystem
{
    private static string path = Application.persistentDataPath + "/save.json";

    public static void SaveGame(Vector3 playerPos, int hp)
    {
        SaveData data = new SaveData();
        data.playerX = playerPos.x;
        data.playerY = playerPos.y;
        data.playerHP = hp;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);

        Debug.Log("Game Saved!");
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(path))
        {
            Debug.Log("No Save Found!");
            return null;
        }

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static bool HasSave()
    {
        return File.Exists(path);
    }
}