using System;
using System.IO;
using System.Text;
using UnityEngine;

// File persistence is independent of scene objects so failure/recovery can be tested in isolation.
public sealed class SaveRepository
{
    public const int CurrentVersion = 1;
    private readonly string path;
    public SaveRepository(string path) { this.path = path; }
    public bool TrySave(SaveData data, out string error)
    {
        error = null;
        if (!IsValid(data)) { error = "Save data is incomplete or unsupported."; return false; }
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, JsonUtility.ToJson(data, true), new UTF8Encoding(false));
            if (File.Exists(path)) File.Replace(temporary, path, path + ".bak");
            else File.Move(temporary, path);
            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
        { error = ex.Message; return false; }
    }
    public bool TryLoad(out SaveData data, out string error)
    {
        if (TryRead(path, out data, out error)) return true;
        if (TryRead(path + ".bak", out data, out string backupError))
        { error = "Recovered the previous save from backup."; return true; }
        if (error == null) error = backupError;
        return false;
    }
    private static bool TryRead(string file, out SaveData data, out string error)
    {
        data = null; error = null;
        try
        {
            if (!File.Exists(file)) return false;
            data = JsonUtility.FromJson<SaveData>(File.ReadAllText(file));
            if (IsValid(data)) return true;
            data = null; error = "Save file is incomplete or has an unsupported version.";
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
        { error = ex.Message; }
        return false;
    }
    public bool TryDelete(out string error)
    {
        error = null;
        try
        {
            foreach (string suffix in new[] { "", ".bak", ".tmp" })
                if (File.Exists(path + suffix)) File.Delete(path + suffix);
            return true;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        { error = ex.Message; return false; }
    }
    public static bool IsValid(SaveData data)
    {
        return data != null && data.version >= 0 && data.version <= CurrentVersion
            && !string.IsNullOrWhiteSpace(data.currentScene) && data.playerHP > 0
            && !float.IsNaN(data.playerX) && !float.IsInfinity(data.playerX)
            && !float.IsNaN(data.playerY) && !float.IsInfinity(data.playerY);
    }
}
