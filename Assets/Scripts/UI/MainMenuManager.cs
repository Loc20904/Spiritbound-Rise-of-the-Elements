using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject resumeButton;
    public GameObject firstSelectedButton;

    private string lastSceneKey = "LastScene";

    void Start()
    {
        // Nếu chưa từng vào game → ẩn Resume
        if (!PlayerPrefs.HasKey(lastSceneKey))
        {
            resumeButton.SetActive(false);
        }
        SelectButton(firstSelectedButton);
    }

    public void NewGame()
    {
        Time.timeScale = 1f; // 🔥 đảm bảo game không bị freeze
        AudioListener.pause = false;

        PlayerPrefs.SetString(lastSceneKey, "GameScene"); // tên scene gameplay
        SceneManager.LoadScene("GameScene");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SceneManager.LoadScene("GameScene");
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