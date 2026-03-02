using UnityEngine;

public enum GameState { MainMenu, Playing, Dialogue, Paused, GameOver, Ending }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; }

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    public void UpdateState(GameState newState)
    {
        CurrentState = newState;
        // Gửi thông báo cho các hệ thống khác qua Event
        OnStateChanged?.Invoke(newState);
    }

    public static event System.Action<GameState> OnStateChanged;
}