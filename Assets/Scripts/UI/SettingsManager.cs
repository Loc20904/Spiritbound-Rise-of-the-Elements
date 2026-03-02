using UnityEngine;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mixer;

    [HideInInspector] public SettingsData current = new SettingsData();
    [HideInInspector] public SettingsData temp = new SettingsData();

    // 🔥 DEFAULT VALUES
    private const float DEFAULT_MASTER = 1f;
    private const float DEFAULT_MUSIC = 1f;
    private const float DEFAULT_SFX = 1f;

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

        temp = Clone(current);

        Apply(current);
        Debug.Log("Loaded Master: " + current.masterVolume);
    }

    public void Save()
    {
        Debug.Log("Saving Master: " + current.masterVolume);

        PlayerPrefs.SetFloat("MasterVol", current.masterVolume);
        PlayerPrefs.SetFloat("MusicVol", current.musicVolume);
        PlayerPrefs.SetFloat("SFXVol", current.sfxVolume);
        PlayerPrefs.Save();
    }

    #endregion

    #region APPLY LOGIC

    // 🔥 Apply bất kỳ data nào vào mixer
    public void Apply(SettingsData data)
    {
        mixer.SetFloat("MasterVolume", Mathf.Log10(Mathf.Max(data.masterVolume, 0.0001f)) * 20);
        mixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(data.musicVolume, 0.0001f)) * 20);
        mixer.SetFloat("SFXVolume", Mathf.Log10(Mathf.Max(data.sfxVolume, 0.0001f)) * 20);
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
    }

    #endregion

    #region APPLY BUTTON FLOW

    // 🔥 Khi nhấn APPLY
    public void SaveTempToSaved()
    {
        current = Clone(temp);
        Save();
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
            sfxVolume = s.sfxVolume
        };
    }
}