using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause")]
    public GameObject pausePanel;
    public GameObject pauseButton;

    [Header("Settings Root")]
    public GameObject settingsPanel;
    public GameObject panelCategoryMenu;

    [Header("Sub Panels")]
    public GameObject panelAudio;
    public GameObject panelGraphics;
    public GameObject panelControls;
    public GameObject panelGameplay;

    [Header("Bottom Bar")]
    public GameObject panelBottomBar;

    [Header("First Selected")]
    public GameObject firstSelectedButton;
    public GameObject firstSelectedSetting;

    [Header("Last Selectable Per Panel")]
    [SerializeField] private GameObject audioLastSelectable;     // Slider_SFX
    [SerializeField] private GameObject graphicsLastSelectable;  //
    [SerializeField] private GameObject controlsLastSelectable;
    [SerializeField] private GameObject gameplayLastSelectable;
    //để set slider master thành target đầu tiên khi mở subpanel audio, tránh trường hợp focus vào button Apply/Reset/Back của bottom bar
    [SerializeField] private GameObject sliderMaster;

    [SerializeField] private UnityEngine.UI.Selectable dropdownResolution;

    [Header("Audio Panel Script")]
    [SerializeField] private AudioSettingsPanel audioSettingsPanel;

    private bool isPaused = false;
    private bool isInSettings = false;

    private GameObject currentSubPanel = null;

    private float escCooldown = 0.2f;
    private float lastEscTime = -1f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.unscaledTime - lastEscTime < escCooldown)
                return;

            lastEscTime = Time.unscaledTime;

            HandleEscape();
        }
    }

    private void HandleEscape()
    {
        // 1️⃣ Nếu đang ở SubPanel → đóng SubPanel trước
        if (currentSubPanel != null)
        {
            CloseSubPanel();
            return;
        }

        // 2️⃣ Nếu đang ở Settings (Category)
        if (isInSettings)
        {
            CloseSettings();
            return;
        }

        // 3️⃣ Nếu đang Pause
        if (isPaused)
        {
            ClosePause();
        }
        else
        {
            OpenPause();
        }
    }

    // =========================
    // PAUSE CONTROL
    // =========================

    public void OpenPause()
    {
        isPaused = true;
        isInSettings = false;

        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.SetActive(false);

        Time.timeScale = 0f;

        Select(firstSelectedButton);
    }

    public void ClosePause()
    {
        isPaused = false;
        isInSettings = false;

        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);

        if (pauseButton != null)
            pauseButton.SetActive(true);

        Time.timeScale = 1f;

        EventSystem.current?.SetSelectedGameObject(null);
    }

    public void Resume()
    {
        ClosePause();
    }

    // =========================
    // SETTINGS CONTROL
    // =========================

    public void OpenSettings()
    {
        isInSettings = true;

        settingsPanel.SetActive(true);

        panelCategoryMenu.SetActive(true);
        panelBottomBar.SetActive(false);

        CloseAllSubPanels();

        Select(firstSelectedSetting);
    }

    public void CloseSettings()
    {
        isInSettings = false;

        CloseAllSubPanels();

        settingsPanel.SetActive(false);

        Select(firstSelectedButton);
    }

    // =========================
    // SUB PANEL CONTROL
    // =========================

    public void OpenSubPanel(GameObject panel)
    {
        CloseAllSubPanels();

        // 🔥 CLEAR selection trước khi disable
        EventSystem.current.SetSelectedGameObject(null);

        panelCategoryMenu.SetActive(false);

        panel.SetActive(true);
        panelBottomBar.SetActive(true);

        currentSubPanel = panel;

        SetupBottomNavigation(); //hàm để thiết lập navigation cho bottom bar dựa trên currentSubPanel
        // 🔥 Focus đúng sau khi panel bật
        StartCoroutine(ForceSelectForPanel());
    }

    public void CloseSubPanel()
    {
        if (currentSubPanel != null)
        {
            currentSubPanel.SetActive(false);
            currentSubPanel = null;
        }

        panelBottomBar.SetActive(false);
        panelCategoryMenu.SetActive(true);

        Select(firstSelectedSetting);
    }

    private void CloseAllSubPanels()
    {
        panelAudio.SetActive(false);
        panelGraphics.SetActive(false);
        panelControls.SetActive(false);
        panelGameplay.SetActive(false);

        panelBottomBar.SetActive(false);

        currentSubPanel = null;
    }

    // =========================
    // HELPER
    // =========================

    private void Select(GameObject target)
    {
        if (EventSystem.current == null || target == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(target);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    IEnumerator ForceSelectForPanel()
    {
        yield return null;
        yield return null;

        Canvas.ForceUpdateCanvases();

        EventSystem.current.SetSelectedGameObject(null);

        if (currentSubPanel == panelAudio)
        {
            sliderMaster.GetComponent<UnityEngine.UI.Selectable>().Select();
            EventSystem.current.SetSelectedGameObject(sliderMaster);
        }
        else if (currentSubPanel == panelGraphics)
        {
            dropdownResolution.Select();
            EventSystem.current.SetSelectedGameObject(dropdownResolution.gameObject);
        }

        Debug.Log("Now Selected: " + EventSystem.current.currentSelectedGameObject);
    }

    // =========================
    // BOTTOM BAR BUTTONS
    // =========================

    public void OnApplyClicked()
    {
        if (currentSubPanel == panelAudio)
        {
            audioSettingsPanel.ApplyAudio();
        }

        // Sau này thêm Graphics / Controls ở đây
    }

    public void OnResetClicked()
    {
        if (currentSubPanel == panelAudio)
        {
            audioSettingsPanel.ResetToDefault();
        }

        // Sau này thêm Graphics / Controls ở đây
    }

    private void SetupBottomNavigation()
    {
        GameObject lastSelectable = null;

        if (currentSubPanel == panelAudio)
            lastSelectable = audioLastSelectable;
        else if (currentSubPanel == panelGraphics)
            lastSelectable = graphicsLastSelectable;
        else if (currentSubPanel == panelControls)
            lastSelectable = controlsLastSelectable;
        else if (currentSubPanel == panelGameplay)
            lastSelectable = gameplayLastSelectable;

        if (lastSelectable == null)
            return;

        var last = lastSelectable.GetComponent<UnityEngine.UI.Selectable>();

        var apply = panelBottomBar.transform.Find("Btn_Apply").GetComponent<UnityEngine.UI.Selectable>();
        var reset = panelBottomBar.transform.Find("Btn_Reset").GetComponent<UnityEngine.UI.Selectable>();
        var back = panelBottomBar.transform.Find("Btn_Back").GetComponent<UnityEngine.UI.Selectable>();

        // Apply
        var navApply = apply.navigation;
        navApply.mode = UnityEngine.UI.Navigation.Mode.Explicit;
        navApply.selectOnUp = last;
        apply.navigation = navApply;

        // Reset
        var navReset = reset.navigation;
        navReset.mode = UnityEngine.UI.Navigation.Mode.Explicit;
        navReset.selectOnUp = last;
        reset.navigation = navReset;

        // Back
        var navBack = back.navigation;
        navBack.mode = UnityEngine.UI.Navigation.Mode.Explicit;
        navBack.selectOnUp = last;
        back.navigation = navBack;
    }
}