using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class SaveRepositoryTests
{
    private string directory;
    private string path;
    private SaveRepository repository;
    [SetUp]
    public void SetUp()
    {
        directory = Path.Combine(Path.GetTempPath(), "SpiritboundTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        path = Path.Combine(directory, "save.json");
        repository = new SaveRepository(path);
    }
    [TearDown]
    public void TearDown() { Directory.Delete(directory, true); }
    private static SaveData Data(int hp = 75) => new SaveData {
        version = SaveRepository.CurrentVersion, currentScene = "MapForest", playerHP = hp,
        playerX = 12.5f, playerY = -7f, tarotIndex = 3,
        storyFlags = new List<StoryFlagData> { new StoryFlagData { name = "GlobalPath_Mercy", value = true } },
        inventory = new List<SavedItem> { new SavedItem { id = "Potion", name = "Potion", healAmount = 20 }, null }
    };
    [Test]
    public void SaveRoundTrip_PreservesPositionStoryTarotAndInventorySlots()
    {
        Assert.That(repository.TrySave(Data(), out var error), Is.True, error);
        Assert.That(repository.TryLoad(out var loaded, out error), Is.True, error);
        Assert.That(loaded.playerHP, Is.EqualTo(75));
        Assert.That(loaded.playerX, Is.EqualTo(12.5f));
        Assert.That(loaded.playerY, Is.EqualTo(-7f));
        Assert.That(loaded.tarotIndex, Is.EqualTo(3));
        Assert.That(loaded.storyFlags[0].value, Is.True);
        Assert.That(loaded.inventory.Count, Is.EqualTo(2));
        Assert.That(loaded.inventory[0].id, Is.EqualTo("Potion"));
        Assert.That(loaded.inventory[1] == null || string.IsNullOrEmpty(loaded.inventory[1].id), Is.True);
    }
    [TestCase("")]
    [TestCase("{broken json")]
    [TestCase("{}")]
    [TestCase("null")]
    public void InvalidSave_ReturnsFailureWithoutThrowing(string json)
    {
        File.WriteAllText(path, json);
        Assert.That(repository.TryLoad(out var loaded, out _), Is.False);
        Assert.That(loaded, Is.Null);
    }
    [Test]
    public void CorruptPrimary_RecoversPreviousValidSave()
    {
        repository.TrySave(Data(75), out _);
        repository.TrySave(Data(50), out _);
        File.WriteAllText(path, "corrupt");
        Assert.That(repository.TryLoad(out var loaded, out string warning), Is.True);
        Assert.That(loaded.playerHP, Is.EqualTo(75));
        Assert.That(warning, Does.Contain("backup"));
    }
    [Test]
    public void InvalidWrite_DoesNotOverwriteExistingSave()
    {
        repository.TrySave(Data(), out _);
        var invalid = Data(0);
        Assert.That(repository.TrySave(invalid, out _), Is.False);
        Assert.That(repository.TryLoad(out var loaded, out _), Is.True);
        Assert.That(loaded.playerHP, Is.EqualTo(75));
    }
    [Test]
    public void LegacySave_StillLoads()
    {
        File.WriteAllText(path, "{\"playerX\":1,\"playerY\":2,\"playerHP\":40,\"currentScene\":\"MapForest\"}");
        Assert.That(repository.TryLoad(out var loaded, out _), Is.True);
        Assert.That(loaded.version, Is.Zero);
        Assert.That(loaded.playerHP, Is.EqualTo(40));
    }
    [Test]
    public void FutureSaveVersion_IsRejected()
    {
        var data = Data(); data.version = 100;
        File.WriteAllText(path, JsonUtility.ToJson(data));
        Assert.That(repository.TryLoad(out _, out _), Is.False);
    }
    [Test]
    public void Delete_RemovesPrimaryBackupAndTemporary()
    {
        repository.TrySave(Data(75), out _);
        repository.TrySave(Data(50), out _);
        File.WriteAllText(path + ".tmp", "unfinished");
        Assert.That(repository.TryDelete(out _), Is.True);
        Assert.That(repository.TryLoad(out _, out _), Is.False);
    }
    [Test]
    public void InaccessibleDestination_ReturnsFailure()
    {
        File.WriteAllText(Path.Combine(directory, "not-a-directory"), "blocker");
        var blocked = new SaveRepository(Path.Combine(directory, "not-a-directory", "save.json"));
        Assert.That(blocked.TrySave(Data(), out var error), Is.False);
        Assert.That(error, Is.Not.Empty);
    }
}
