using UnityEngine;

public enum GamePath { None, PathOfMercy, PathOfDestruction } // Chưa chọn, Thu phục, hoặc Kết liễu

public class GameProgress : MonoBehaviour
{
    public static GameProgress Instance;

    public GamePath SelectedPath { get; private set; } = GamePath.None;
    public int BossesDefeatedCount { get; private set; } = 0;

    private void Awake() { Instance = this; }

    public void MakeFirstChoice(GamePath path)
    {
        if (SelectedPath == GamePath.None)
        {
            SelectedPath = path;
            Debug.Log("Con đường đã được chọn: " + path);
            // Gửi sự kiện để Team UI hiển thị thông báo "Số phận đã an bài"
            //GameEvents.TriggerPathLocked(path);
        }
    }

    public void IncrementBossCount()
    {
        BossesDefeatedCount++;
    }
}