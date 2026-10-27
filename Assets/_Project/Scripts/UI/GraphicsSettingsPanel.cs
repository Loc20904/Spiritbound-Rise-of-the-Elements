using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GraphicsSettingsPanel : MonoBehaviour
{
    [Header("UI")]
    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public Toggle fullscreenToggle;
    public Toggle vSyncToggle;

    [Header("Resolution Sprites")]
    public Sprite sprite720;
    public Sprite sprite1366;
    public Sprite sprite900;
    public Sprite sprite1080;
    public Sprite sprite1440;
    public Sprite sprite4K;


    void OnEnable()
    {
        StartCoroutine(Initialize());
    }

    System.Collections.IEnumerator Initialize()
    {
        while (SettingsManager.Instance == null) yield return null;
        SettingsManager.Instance.BeginEditing();
        SetupResolutions();
        LoadFromTemp();
    }

    void SetupResolutions()
    {
        resolutionDropdown.ClearOptions();

        var options = new List<TMP_Dropdown.OptionData>();

        options.Add(new TMP_Dropdown.OptionData("", sprite720, Color.white));
        options.Add(new TMP_Dropdown.OptionData("", sprite1366, Color.white));
        options.Add(new TMP_Dropdown.OptionData("", sprite900, Color.white));
        options.Add(new TMP_Dropdown.OptionData("", sprite1080, Color.white));
        options.Add(new TMP_Dropdown.OptionData("", sprite1440, Color.white));
        options.Add(new TMP_Dropdown.OptionData("", sprite4K, Color.white));
        Debug.Log(sprite720);
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = 0;
        resolutionDropdown.RefreshShownValue();
    }

    Sprite GetSpriteByIndex(int index)
    {
        switch (index)
        {
            case 0: return sprite720;
            case 1: return sprite1366;
            case 2: return sprite900;
            case 3: return sprite1080;
            case 4: return sprite1440;
            case 5: return sprite4K;
        }

        return sprite720;
    }

    void LoadFromTemp()
    {
        var temp = SettingsManager.Instance.temp;

        SettingsManager.Sanitize(temp);
        resolutionDropdown.SetValueWithoutNotify(temp.resolutionIndex);
        qualityDropdown.SetValueWithoutNotify(temp.qualityIndex);
        fullscreenToggle.SetIsOnWithoutNotify(temp.fullscreen);
        vSyncToggle.SetIsOnWithoutNotify(temp.vSync);

        // set sprite đúng
        resolutionDropdown.captionImage.sprite = GetSpriteByIndex(temp.resolutionIndex);
    }

    // ===== UI EVENTS =====

    public void OnResolutionChanged(int index)
    {
        if (SettingsManager.Instance == null || index < 0 || index >= resolutionDropdown.options.Count) return;
        SettingsManager.Instance.temp.resolutionIndex = index;

        // đổi sprite hiển thị trên dropdown
        resolutionDropdown.captionImage.sprite = resolutionDropdown.options[index].image;
    }

    public void OnQualityChanged(int index)
    {
        SettingsManager.Instance.temp.qualityIndex = index;
    }

    public void OnFullscreenChanged(bool value)
    {
        SettingsManager.Instance.temp.fullscreen = value;
    }

    public void OnVSyncChanged(bool value)
    {
        SettingsManager.Instance.temp.vSync = value;
    }

    public void OnReset()
    {
        SettingsManager.Instance.temp.resolutionIndex = 0;
        SettingsManager.Instance.temp.qualityIndex = 1;
        SettingsManager.Instance.temp.fullscreen = true;
        SettingsManager.Instance.temp.vSync = true;

        LoadFromTemp();
    }

    // ===== DÙNG KHI APPLY =====

    public Vector2Int GetSelectedResolution()
    {
        return SettingsManager.Resolutions[Mathf.Clamp(SettingsManager.Instance.temp.resolutionIndex, 0, SettingsManager.Resolutions.Count - 1)];
    }
}
