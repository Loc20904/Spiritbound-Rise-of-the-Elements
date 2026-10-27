using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public class SessionAndUiReworkTests
{
    private GameObject player, inventoryObject, pauseObject;
    private PlayerHealth health;
    private InventoryManager inventory;
    private PlayerStatManager stats;
    private PlayerStatProfile previousStat;
    private bool hadTarot, hadLastScene;
    private int tarot;
    private string lastScene;
    private GameProgressManager progress;
    private List<StoryFlagData> previousFlags;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Time.timeScale = 1f; AudioListener.pause = false;
        hadTarot = PlayerPrefs.HasKey("PlayerStatIndex"); tarot = PlayerPrefs.GetInt("PlayerStatIndex", -1);
        hadLastScene = PlayerPrefs.HasKey("LastScene"); lastScene = PlayerPrefs.GetString("LastScene", "");
        stats = PlayerStatManager.EnsureInstance(); previousStat = stats.ActiveStat;
        SetActiveStat(null);
        progress = GameProgressManager.EnsureInstance(); previousFlags = progress.CaptureFlags();
        progress.ResetProgress(); GameSession.Inventory.Clear();
        player = new GameObject("Inventory test player");
        player.SetActive(false);
        player.AddComponent<Rigidbody2D>().gravityScale = 0;
        health = player.AddComponent<PlayerHealth>(); health.showDamageLog = false;
        player.SetActive(true);
        inventoryObject = new GameObject("Test inventory"); inventory = inventoryObject.AddComponent<InventoryManager>();
        yield return null; health.ResetHealth();
    }
    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Object.Destroy(player); Object.Destroy(inventoryObject);
        if (pauseObject != null) Object.Destroy(pauseObject);
        progress.RestoreFlags(previousFlags); SetActiveStat(previousStat);
        if (hadTarot) PlayerPrefs.SetInt("PlayerStatIndex", tarot); else PlayerPrefs.DeleteKey("PlayerStatIndex");
        if (hadLastScene) PlayerPrefs.SetString("LastScene", lastScene); else PlayerPrefs.DeleteKey("LastScene");
        PlayerPrefs.Save();
        GameSession.Inventory.Clear();
        Time.timeScale = 1f; AudioListener.pause = false;
        yield return null;
    }
    private void SetActiveStat(PlayerStatProfile value) => typeof(PlayerStatManager).GetProperty("ActiveStat").SetValue(stats, value);
    [Test]
    public void Potion_HealsConfiguredAmountAndConsumesOnlyOnce()
    {
        health.TakeDamage(45);
        inventory.AddItem(new Item { itemName = "Potion", healAmount = 20 });
        Assert.That(inventory.TryUseItem(0), Is.True);
        Assert.That(health.CurrentHP, Is.EqualTo(80));
        Assert.That(inventory.TryUseItem(0), Is.False);
    }
    [Test]
    public void UnknownItemsAndFullHealthPotion_AreNotConsumed()
    {
        var rune = new Item { itemName = "Rune", itemType = Item.ItemType.Material };
        var potion = new Item { itemName = "Potion", healAmount = 20 };
        inventory.AddItem(rune); inventory.AddItem(potion);
        Assert.That(inventory.TryUseItem(0), Is.False);
        Assert.That(inventory.TryUseItem(1), Is.False);
        Assert.That(inventory.items[0], Is.SameAs(rune));
        Assert.That(inventory.items[1], Is.SameAs(potion));
        Assert.That(inventory.TryUseItem(-1), Is.False);
    }
    [Test]
    public void EmptySlots_AreReusedAndAcceptDragMoves()
    {
        var potion = new Item { itemName = "Potion", healAmount = 20 };
        inventory.items.Add(null); inventory.AddItem(potion);
        Assert.That(inventory.items.Count, Is.EqualTo(1));
        Assert.That(inventory.MoveItem(0, 19), Is.True);
        Assert.That(inventory.items[0], Is.Null);
        Assert.That(inventory.items[19], Is.SameAs(potion));
        inventory.AddItem(new Item { itemName = "Rune" });
        Assert.That(inventory.items.Count, Is.EqualTo(20));
    }
    [UnityTest]
    public IEnumerator Inventory_SurvivesManagerRecreationOnSceneChange()
    {
        var potion = new Item { itemName = "Potion", healAmount = 20 };
        inventory.AddItem(potion);
        Object.Destroy(inventoryObject); yield return null;
        inventoryObject = new GameObject("Next scene inventory"); inventory = inventoryObject.AddComponent<InventoryManager>();
        Assert.That(inventory.items[0], Is.SameAs(potion));
    }
    [Test]
    public void TarotModifiers_ApplyToHealthDamageArmorAndIncomingDamage()
    {
        SetActiveStat(new PlayerStatProfile { hpModifier = 25, damageModifier = 20, weaponDamageModifier = 50,
            armorDefenseModifier = 100, damageTakenModifier = -50, skillDamageModifier = 100 });
        health.ResetHealth();
        Assert.That(health.MaxHP, Is.EqualTo(125));
        Assert.That(health.ScaleDamage(10, false), Is.EqualTo(18));
        Assert.That(health.ScaleDamage(10, true), Is.EqualTo(24));
        health.TakeDamage(30);
        Assert.That(health.CurrentHP, Is.EqualTo(115));
    }
    [Test]
    public void TarotAssignment_UpdatesExistingPlayerHealthWithoutStackingModifiers()
    {
        int index = stats.database.stats.FindIndex(profile => profile.hpModifier != 0f);
        Assert.That(index, Is.GreaterThanOrEqualTo(0));
        stats.RestoreStat(index);
        int expected = Mathf.Max(1, Mathf.RoundToInt(100 * stats.GetHPMultiplier()));
        Assert.That(health.MaxHP, Is.EqualTo(expected));
        Assert.That(health.CurrentHP, Is.EqualTo(expected));
        stats.RestoreStat(index);
        Assert.That(health.MaxHP, Is.EqualTo(expected));
        stats.RestoreStat(-1);
        Assert.That(health.CurrentHP, Is.EqualTo(100));
    }

    [Test]
    public void NewGame_ClearsStoryInventoryTarotAndPendingResume()
    {
        GameSession.Restore(new SaveData { version = 1, currentScene = "MapForest", playerHP = 50, tarotIndex = 3,
            storyFlags = new List<StoryFlagData> { new StoryFlagData { name = "GlobalPath_Kill", value = true } } });
        inventory.AddItem(new Item { itemName = "Potion" });
        GameSession.NewGame();
        Assert.That(progress.GetFlag("GlobalPath_Kill"), Is.False);
        Assert.That(inventory.items, Is.Empty);
        Assert.That(stats.HasReceivedStat, Is.False);
        Assert.That(GameSession.TryConsumePlayerRestore("MapForest", out _), Is.False);
    }
    [Test]
    public void StoryBranch_KillAndMercyAreMutuallyExclusive()
    {
        progress.SetFlag("GlobalPath_Kill", true);
        progress.SetFlag("GlobalPath_Mercy", true);
        Assert.That(progress.GetFlag("GlobalPath_Kill"), Is.False);
        Assert.That(progress.GetFlag("GlobalPath_Mercy"), Is.True);
    }
    [Test]
    public void Resume_RestoresStoryTarotInventory_AndPlayerStateOnlyOnce()
    {
        GameSession.Restore(new SaveData { version = 1, currentScene = "MapForest", playerHP = 50, playerX = 9, playerY = 4, tarotIndex = 3,
            storyFlags = new List<StoryFlagData> { new StoryFlagData { name = "Boss1_Dead", value = true } },
            inventory = new List<SavedItem> { null, new SavedItem { id = "Potion", name = "Potion", healAmount = 20 } } });
        Assert.That(progress.GetFlag("Boss1_Dead"), Is.True);
        Assert.That(stats.ActiveIndex, Is.EqualTo(3));
        Assert.That(inventory.items[0], Is.Null);
        Assert.That(inventory.items[1].icon, Is.Not.Null, "Catalog must restore real sprite references, not instance IDs.");
        Assert.That(GameSession.TryConsumePlayerRestore("MapIce", out _), Is.False);
        Assert.That(GameSession.TryConsumePlayerRestore("MapForest", out var restored), Is.True);
        Assert.That(restored.playerX, Is.EqualTo(9));
        Assert.That(GameSession.TryConsumePlayerRestore("MapForest", out _), Is.False);
    }
    [UnityTest]
    public IEnumerator Resume_PlayerStartRestoresPositionAndClampsHP()
    {
        var scene = SceneManager.GetActiveScene().name;
        GameSession.Restore(new SaveData { version = 1, currentScene = scene, playerHP = 999, playerX = 9, playerY = 4 });
        Object.Destroy(player); yield return null;
        player = new GameObject("Resumed player"); player.SetActive(false);
        health = player.AddComponent<PlayerHealth>(); health.showDamageLog = false; player.SetActive(true);
        yield return null;
        Assert.That(player.transform.position, Is.EqualTo(new Vector3(9, 4, 0)));
        Assert.That(health.CurrentHP, Is.EqualTo(health.MaxHP));
    }
    private PauseMenuManager PauseMenu()
    {
        pauseObject = new GameObject("Pause test root");
        var pause = pauseObject.AddComponent<PauseMenuManager>(); pause.player = player;
        pause.pausePanel = new GameObject("Pause panel"); pause.pausePanel.transform.SetParent(pauseObject.transform);
        pause.settingsPanel = new GameObject("Settings panel"); pause.settingsPanel.transform.SetParent(pauseObject.transform);
        return pause;
    }
    [Test]
    public void PauseClose_RestoresTimeAudioAndPlayerControl()
    {
        var pause = PauseMenu(); Time.timeScale = 0.5f;
        pause.OpenPause(); pause.OpenPause();
        Assert.That(Time.timeScale, Is.Zero); Assert.That(AudioListener.pause, Is.True);
        Assert.That(health.CanAct, Is.False);
        pause.ClosePause();
        Assert.That(Time.timeScale, Is.EqualTo(0.5f)); Assert.That(AudioListener.pause, Is.False);
        Assert.That(health.CanAct, Is.True);
    }
    [Test]
    public void DisablePausedMenu_DoesNotLeaveGlobalPauseBehind()
    {
        var pause = PauseMenu(); pause.OpenPause(); pause.enabled = false;
        Assert.That(Time.timeScale, Is.EqualTo(1f)); Assert.That(AudioListener.pause, Is.False);
        Assert.That(health.CanAct, Is.True);
    }
}
