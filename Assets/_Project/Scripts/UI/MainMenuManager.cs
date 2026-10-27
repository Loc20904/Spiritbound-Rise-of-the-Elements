using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject resumeButton;
    public GameObject firstSelectedButton;


    void Start()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        // Nếu chưa từng vào game hoạt không có save → ẩn Resume
        if (resumeButton != null) resumeButton.SetActive(SaveSystem.HasSave());
        SelectButton(firstSelectedButton);
    }

    public void NewGame()
    {
        Time.timeScale = 1f; // 🔥 đảm bảo game không bị freeze
        AudioListener.pause = false;

        if (!SaveSystem.DeleteSave()) return;
        GameSession.NewGame();
        SceneManager.LoadScene("IntroScene");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SaveData data = SaveSystem.LoadGame();
        if (data == null) return;
        GameSession.Restore(data);
        SceneManager.LoadScene(data.currentScene);
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit Game"); // để test trong editor
    }

    void SelectButton(GameObject target)
    {
        if (EventSystem.current == null || target == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);

        Debug.Log("Selected: " + target.name);
    }
}
