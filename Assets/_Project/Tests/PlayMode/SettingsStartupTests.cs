using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class SettingsStartupTests
{
    [UnityTest]
    public IEnumerator Startup_AppliesSavedQualityAndVSyncWithoutOpeningSettingsUi()
    {
        string[] keys = { "ResolutionIndex", "QualityIndex", "VSync" };
        bool[] had = System.Array.ConvertAll(keys, PlayerPrefs.HasKey);
        int[] values = System.Array.ConvertAll(keys, key => PlayerPrefs.GetInt(key));
        int previousQuality = QualitySettings.GetQualityLevel();
        int previousVSync = QualitySettings.vSyncCount;
        var managerObject = new GameObject("Settings startup test");
        managerObject.SetActive(false);
        try
        {
            int targetQuality = Mathf.Max(0, QualitySettings.names.Length - 1);
            PlayerPrefs.SetInt("ResolutionIndex", int.MaxValue);
            PlayerPrefs.SetInt("QualityIndex", targetQuality);
            PlayerPrefs.SetInt("VSync", 0);
            var manager = managerObject.AddComponent<SettingsManager>();
            managerObject.SetActive(true);
            yield return null;
            Assert.That(QualitySettings.GetQualityLevel(), Is.EqualTo(targetQuality));
            Assert.That(QualitySettings.vSyncCount, Is.Zero);
            Assert.That(manager.current.resolutionIndex, Is.EqualTo(SettingsManager.Resolutions.Count - 1));
        }
        finally
        {
            Object.Destroy(managerObject);
            for (int i = 0; i < keys.Length; i++)
                if (had[i]) PlayerPrefs.SetInt(keys[i], values[i]); else PlayerPrefs.DeleteKey(keys[i]);
            PlayerPrefs.Save();
            QualitySettings.SetQualityLevel(previousQuality);
            QualitySettings.vSyncCount = previousVSync;
        }
    }
}
