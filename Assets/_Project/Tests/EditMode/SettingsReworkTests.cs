using NUnit.Framework;
using UnityEngine;
public class SettingsReworkTests
{
    [Test]
    public void InvalidPreferences_AreClampedToSupportedGraphicsAndVolumeRanges()
    {
        var data = new SettingsData { resolutionIndex = int.MaxValue, qualityIndex = -100,
            masterVolume = float.NaN, musicVolume = -5, sfxVolume = 100 };
        SettingsManager.Sanitize(data);
        Assert.That(data.resolutionIndex, Is.EqualTo(SettingsManager.Resolutions.Count - 1));
        Assert.That(data.qualityIndex, Is.Zero);
        Assert.That(data.masterVolume, Is.EqualTo(1));
        Assert.That(data.musicVolume, Is.Zero);
        Assert.That(data.sfxVolume, Is.EqualTo(1));
    }
}
