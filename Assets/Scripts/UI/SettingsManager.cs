using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [HideInInspector] public SettingsData current = new SettingsData();
    [HideInInspector] public SettingsData temp = new SettingsData();

    // 🔥 DEFAULT VALUES FOR AUDIO PANEL (khi Reset về mặc định sẽ dùng những giá trị này, và cũng là giá trị mặc định khi lần đầu chạy game)
    private const float DEFAULT_MASTER = 1f;
    private const float DEFAULT_MUSIC = 1f;
    private const float DEFAULT_SFX = 1f;

    // 🔥 DEFAULT VALUES FOR GRAPHIC PANEL (khi Reset về mặc định sẽ dùng những giá trị này, và cũng là giá trị mặc định khi lần đầu chạy game)
    private const int DEFAULT_RESOLUTION = 0;
    private const int DEFAULT_QUALITY = 1;
    private const int DEFAULT_FULLSCREEN = 1;
    private const int DEFAULT_VSYNC = 1;

    private void Awake()
    {
        Debug.Log("SettingsManager Awake: " + gameObject.scene.name);
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region LOAD & SAVE

    public void Load()
    {
        current.masterVolume = PlayerPrefs.GetFloat("MasterVol", DEFAULT_MASTER);
        current.musicVolume = PlayerPrefs.GetFloat("MusicVol", DEFAULT_MUSIC);
        current.sfxVolume = PlayerPrefs.GetFloat("SFXVol", DEFAULT_SFX);

        // ===== GRAPHICS =====
        current.resolutionIndex = PlayerPrefs.GetInt("ResolutionIndex", DEFAULT_RESOLUTION);
        current.qualityIndex = PlayerPrefs.GetInt("QualityIndex", DEFAULT_QUALITY);
        current.fullscreen = PlayerPrefs.GetInt("Fullscreen", DEFAULT_FULLSCREEN) == 1;
        current.vSync = PlayerPrefs.GetInt("VSync", DEFAULT_VSYNC) == 1;

        temp = Clone(current);

        Apply(current);
        Debug.Log("Loaded Master: " + current.masterVolume);
    }

    public void Save()
    {
        Debug.Log("Saving Master: " + current.masterVolume);
        //Audio
        PlayerPrefs.SetFloat("MasterVol", current.masterVolume);
        PlayerPrefs.SetFloat("MusicVol", current.musicVolume);
        PlayerPrefs.SetFloat("SFXVol", current.sfxVolume);

        // ===== GRAPHICS =====
        PlayerPrefs.SetInt("ResolutionIndex", current.resolutionIndex);
        PlayerPrefs.SetInt("QualityIndex", current.qualityIndex);
        PlayerPrefs.SetInt("Fullscreen", current.fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("VSync", current.vSync ? 1 : 0);

        PlayerPrefs.Save();
    }

    #endregion

    #region APPLY LOGIC

    // 🔥 Apply bất kỳ data nào vào mixer (Audio)
    public void Apply(SettingsData data)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(data.masterVolume, 0.0001f)) * 20);
        mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(data.musicVolume, 0.0001f)) * 20);
        mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(data.sfxVolume, 0.0001f)) * 20);
    }

    // 🔥 Apply (Graphics)
    public void ApplyGraphics(SettingsData data)
    {
        // Resolution
        Vector2Int[] fixedResolutions = new Vector2Int[]
    {
        new Vector2Int(1280, 720),
        new Vector2Int(1366, 768),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080),
        new Vector2Int(2560, 1440),
        new Vector2Int(3840, 2160)
    };

        Vector2Int res = fixedResolutions[data.resolutionIndex];

        Screen.SetResolution(res.x, res.y, data.fullscreen);

        // Quality
        QualitySettings.SetQualityLevel(data.qualityIndex);

        // VSync
        QualitySettings.vSyncCount = data.vSync ? 1 : 0;

        // Fullscreen Mode (modern way)
        Screen.fullScreenMode = data.fullscreen ?
            FullScreenMode.FullScreenWindow :
            FullScreenMode.Windowed;
    }

    // 🔥 Apply TEMP (dùng khi preview nếu sau này bạn muốn)
    public void ApplyTemp()
    {
        Apply(temp);
    }

    // 🔥 Apply CURRENT (khi nhấn Apply)
    public void ApplySaved()
    {
        Apply(current);
        ApplyGraphics(current);
    }

    #endregion

    #region APPLY BUTTON FLOW

    // 🔥 Khi nhấn APPLY
    public void SaveTempToSaved()
    {
        current = Clone(temp);
        Save();
        ApplySaved();
        temp = Clone(current);
    }

    #endregion

    #region RESET

    // 🔥 Reset chỉ ảnh hưởng TEMP (chưa Save)
    public void ResetAudioToDefault()
    {
        temp.masterVolume = DEFAULT_MASTER;
        temp.musicVolume = DEFAULT_MUSIC;
        temp.sfxVolume = DEFAULT_SFX;
    }

    public void BeginEditing()
    {
        temp = Clone(current);
    }

    #endregion

    SettingsData Clone(SettingsData s)
    {
        return new SettingsData
        {
            masterVolume = s.masterVolume,
            musicVolume = s.musicVolume,
            sfxVolume = s.sfxVolume,

            resolutionIndex = s.resolutionIndex,
            qualityIndex = s.qualityIndex,
            fullscreen = s.fullscreen,
            vSync = s.vSync
        };
    }
}