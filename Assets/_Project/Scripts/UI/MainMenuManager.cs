using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Buttons")]
    public GameObject resumeButton;
    public GameObject firstSelectedButton;

    private string lastSceneKey = "LastScene";

    void Start()
    {
        // Nếu chưa từng vào game hoạt không có save → ẩn Resume
        if (!SaveSystem.HasSave())
        {
            resumeButton.SetActive(false);
        }
        else
        {
            resumeButton.SetActive(true);
        }
        SelectButton(firstSelectedButton);
    }

    public void NewGame()
    {
        Time.timeScale = 1f; // 🔥 đảm bảo game không bị freeze
        AudioListener.pause = false;

        SaveSystem.DeleteSave();
        PlayerPrefs.DeleteKey(lastSceneKey);
        
        // Cần reset các stat phụ nếu có
        PlayerStatManager.Instance?.ResetStat();

        SceneManager.LoadScene("IntroScene");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        SaveData data = SaveSystem.LoadGame();
        string sceneToLoad = "IntroScene";
        
        if (data != null && !string.IsNullOrEmpty(data.currentScene))
        {
            sceneToLoad = data.currentScene;
        }
        else if (PlayerPrefs.HasKey(lastSceneKey))
        {
            sceneToLoad = PlayerPrefs.GetString(lastSceneKey);
        }

        SceneManager.LoadScene(sceneToLoad);
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