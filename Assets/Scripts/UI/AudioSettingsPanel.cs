using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsPanel : MonoBehaviour
{
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    void OnEnable()
    {
        // 🔥 Nếu SettingsManager chưa sẵn sàng thì đợi 1 frame
        if (SettingsManager.Instance == null)
        {
            StartCoroutine(DelayedLoad());
            return;
        }
        SettingsManager.Instance.BeginEditing();
        LoadFromTemp();
    }

    System.Collections.IEnumerator DelayedLoad()
    {
        yield return null;

        if (SettingsManager.Instance != null)
        {
            LoadFromTemp();
        }
    }

    void LoadFromTemp()
    {
        if (SettingsManager.Instance == null)
        {
            Debug.LogError("SettingsManager Instance is NULL!");
            return;
        }

        if (masterSlider == null || musicSlider == null || sfxSlider == null)
        {
            Debug.LogError("One of the sliders is NOT assigned!");
            return;
        }

        masterSlider.value = SettingsManager.Instance.temp.masterVolume * 100f;
        musicSlider.value = SettingsManager.Instance.temp.musicVolume * 100f;
        sfxSlider.value = SettingsManager.Instance.temp.sfxVolume * 100f;
    }

    public void OnMasterChanged(float value)
    {
        if (SettingsManager.Instance == null) return;
        SettingsManager.Instance.temp.masterVolume = value / 100f;
    }

    public void OnMusicChanged(float value)
    {
        if (SettingsManager.Instance == null) return;
        SettingsManager.Instance.temp.musicVolume = value / 100f;
    }

    public void OnSFXChanged(float value)
    {
        if (SettingsManager.Instance == null) return;
        SettingsManager.Instance.temp.sfxVolume = value / 100f;
    }

    public void ApplyAudio()
    {
        if (SettingsManager.Instance == null) return;

        SettingsManager.Instance.SaveTempToSaved();
        SettingsManager.Instance.ApplySaved();

        Debug.Log("Audio Applied");
    }

    public void ResetToDefault()
    {
        if (SettingsManager.Instance == null) return;

        SettingsManager.Instance.ResetAudioToDefault();
        LoadFromTemp();

        Debug.Log("Audio Reset To Default (Not Saved Yet)");
    }
}