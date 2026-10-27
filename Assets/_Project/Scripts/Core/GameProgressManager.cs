using System.Collections.Generic;
using UnityEngine;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance;

    // Từ điển lưu trữ các "Cờ" trạng thái trong game (Ví dụ: "Boss1_Dead" -> true)
    private Dictionary<string, bool> storyFlags = new Dictionary<string, bool>();

    private void Awake()
    {
        // Thiết lập Singleton để các script khác gọi dễ dàng: GameProgressManager.Instance...
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ nguyên không bị xóa khi chuyển Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static GameProgressManager EnsureInstance()
    {
        if (Instance == null) new GameObject("GameProgressManager").AddComponent<GameProgressManager>();
        return Instance;
    }
    public void ResetProgress() => storyFlags.Clear();
    public List<StoryFlagData> CaptureFlags()
    {
        var data = new List<StoryFlagData>();
        foreach (var flag in storyFlags) data.Add(new StoryFlagData { name = flag.Key, value = flag.Value });
        return data;
    }
    public void RestoreFlags(List<StoryFlagData> flags)
    {
        storyFlags.Clear();
        if (flags == null) return;
        foreach (var flag in flags)
            if (flag != null && !string.IsNullOrEmpty(flag.name)) storyFlags[flag.name] = flag.value;
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    // Hàm để đánh dấu một sự kiện đã xảy ra
    public void SetFlag(string flagName, bool value)
    {
        if (value && flagName == "GlobalPath_Kill") storyFlags["GlobalPath_Mercy"] = false;
        if (value && flagName == "GlobalPath_Mercy") storyFlags["GlobalPath_Kill"] = false;
        if (storyFlags.ContainsKey(flagName))
            storyFlags[flagName] = value;
        else
            storyFlags.Add(flagName, value);

        Debug.Log($"[Story Progress] Đã cập nhật cờ: {flagName} = {value}");
    }

    // Hàm kiểm tra xem một sự kiện đã xảy ra chưa
    public bool GetFlag(string flagName)
    {
        if (storyFlags.ContainsKey(flagName))
            return storyFlags[flagName];

        return false; // Mặc định nếu chưa set thì là false
    }
}
