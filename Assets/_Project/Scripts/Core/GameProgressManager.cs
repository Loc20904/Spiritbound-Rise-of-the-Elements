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

    // Hàm để đánh dấu một sự kiện đã xảy ra
    public void SetFlag(string flagName, bool value)
    {
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