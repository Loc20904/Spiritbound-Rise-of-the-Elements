using UnityEngine;

[System.Serializable]
public class SettingsData
{
    // ===== AUDIO =====
    public float masterVolume = 1f;
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    // ===== GRAPHICS =====
    public int resolutionIndex = 0;
    public int qualityIndex = 1;     // Balanced mặc định
    public bool fullscreen = true;
    public bool vSync = true;
}