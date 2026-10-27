using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

public class CombatReworkTests
{
    private PlayerStatProfile previousStat;
    private PlayerStatManager statManager;
    private GameObject player;
    private GameObject fire;
    private PlayerHealth health;
    private PlayerStats stats;
    private PlayerTransformManager forms;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        Time.timeScale = 1f;
        statManager = PlayerStatManager.EnsureInstance();
        previousStat = statManager.ActiveStat;
        typeof(PlayerStatManager).GetProperty("ActiveStat").SetValue(statManager, null);
        player = new GameObject("Combat test player");
        player.SetActive(false);
        player.transform.position = new Vector3(3, 4, 0);
        player.AddComponent<Rigidbody2D>().gravityScale = 0;
        health = player.AddComponent<PlayerHealth>();
        health.showDamageLog = false;
        health.respawnDelay = 0.05f;
        stats = player.AddComponent<PlayerStats>();
        fire = new GameObject("Combat test fire form");
        fire.SetActive(false);
        fire.transform.localScale = new Vector3(2, 3, 1);
        fire.AddComponent<Rigidbody2D>().gravityScale = 0;
        forms = player.AddComponent<PlayerTransformManager>();
        forms.normalTarget = player.transform;
        forms.fireForm = fire;
        player.SetActive(true);
        yield return null;
        health.ResetHealth();
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        Time.timeScale = 1f;
        typeof(PlayerStatManager).GetProperty("ActiveStat").SetValue(statManager, previousStat);
        Object.Destroy(player);
        if (fire != null) Object.Destroy(fire);
        yield return null;
    }

    [Test]
    public void DirectDamage_SubtractsArmorOnce()
    {
        health.TakeDamage(20);
        Assert.That(health.CurrentHP, Is.EqualTo(85));
    }

    [Test]
    public void LegacyStatsDamage_SubtractsArmorOnce()
    {
        stats.TakeDamage(20);
        Assert.That(health.CurrentHP, Is.EqualTo(85));
    }

    [Test]
    public void LegacyAndDirectDamage_ShareInvulnerabilityWindow()
    {
        stats.TakeDamage(20);
        health.TakeDamage(20);
        Assert.That(health.CurrentHP, Is.EqualTo(85));
    }

    [Test]
    public void InvalidDamageAndHealing_DoNotChangeHP()
    {
        health.TakeDamage(-20);
        health.Heal(-30);
        Assert.That(health.CurrentHP, Is.EqualTo(100));
        health.Heal(int.MaxValue);
        Assert.That(health.CurrentHP, Is.EqualTo(100));
    }

    [UnityTest]
    public IEnumerator LethalHitThenKnockback_RespawnsAndCanTakeDamageAgain()
    {
        player.transform.position = new Vector3(10, 10, 0);
        stats.TakeDamage(1000);
        health.ApplyKnockback(Vector2.right * 5, 1f);
        health.ResetHealth();
        Assert.That(health.CurrentHP, Is.Zero, "Healing must not bypass the death lifecycle.");
        yield return new WaitForSeconds(0.1f);
        Assert.That(health.IsDead, Is.False);
        Assert.That(health.CurrentHP, Is.EqualTo(100));
        Assert.That(player.transform.position, Is.EqualTo(new Vector3(3, 4, 0)));
        Assert.That(health.isKnockback, Is.False);
        yield return new WaitForSeconds(0.55f);
        health.TakeDamage(20);
        Assert.That(health.CurrentHP, Is.EqualTo(85), "Respawn must not leave permanent immunity.");
    }

    [UnityTest]
    public IEnumerator RepeatedKnockback_ThenDeath_DoesNotCancelRespawn()
    {
        health.ApplyKnockback(Vector2.right, 1f);
        health.ApplyKnockback(Vector2.left, 1f);
        health.TakeDamage(1000);
        yield return new WaitForSeconds(0.1f);
        Assert.That(health.IsDead, Is.False);
        Assert.That(health.isKnockback, Is.False);
    }

    [Test]
    public void ControlLocks_ReleaseOnlyTheirOwnOwner()
    {
        object inventory = new object();
        object dialogue = new object();
        health.SetControlsBlocked(inventory, true);
        health.SetControlsBlocked(dialogue, true);
        health.SetControlsBlocked(inventory, false);
        Assert.That(health.CanAct, Is.False);
        health.SetControlsBlocked(dialogue, false);
        Assert.That(health.CanAct, Is.True);
    }

    [Test]
    public void DetachedFormAndChildCollider_ResolveSharedHealth()
    {
        var child = new GameObject("Child collider");
        child.transform.SetParent(fire.transform);
        var collider = child.AddComponent<BoxCollider2D>();
        Assert.That(PlayerHealth.Resolve(collider), Is.SameAs(health));
        Assert.That(PlayerStats.Resolve(collider), Is.SameAs(stats));
        PlayerHealth.Resolve(collider).TakeDamage(20);
        Assert.That(health.CurrentHP, Is.EqualTo(85));
    }

    [UnityTest]
    public IEnumerator TransformationDeath_ReturnsToNormalAndRespawns()
    {
        forms.SwitchToFire();
        yield return null;
        Assert.That(forms.IsTransformed, Is.True);
        fire.transform.position = new Vector3(7, 8, 0);
        Assert.That(forms.ActiveBody, Is.SameAs(fire.GetComponent<Rigidbody2D>()));
        health.TakeDamage(1000);
        Assert.That(forms.IsTransformed, Is.False);
        Assert.That(fire.activeSelf, Is.False);
        yield return new WaitForSeconds(0.1f);
        Assert.That(health.IsDead, Is.False);
        Assert.That(health.CurrentHP, Is.EqualTo(100));
        Assert.That(player.GetComponent<Rigidbody2D>().simulated, Is.True);
        Assert.That(player.transform.position, Is.EqualTo(new Vector3(3, 4, 0)));
    }

    [Test]
    public void InventoryLock_PreventsSwitch_AndDoesNotChangeFormOnRelease()
    {
        forms.SwitchToFire();
        health.SetControlsBlocked(this, true);
        forms.RevertToNormal();
        Assert.That(forms.IsTransformed, Is.True);
        health.SetControlsBlocked(this, false);
        Assert.That(forms.IsTransformed, Is.True);
    }

    [Test]
    public void NormalAndFireScales_SurviveRoundTrip()
    {
        player.transform.localScale = new Vector3(-1, 1, 1);
        forms.SwitchToFire();
        Assert.That(fire.transform.localScale, Is.EqualTo(new Vector3(-2, 3, 1)));
        forms.RevertToNormal();
        Assert.That(player.transform.localScale, Is.EqualTo(new Vector3(-1, 1, 1)));
    }

    [Test]
    public void DashHitsChildCollider_OncePerDash()
    {
        var controllerObject = new GameObject("Dash controller");
        controllerObject.SetActive(false);
        controllerObject.AddComponent<Rigidbody2D>().gravityScale = 0;
        controllerObject.AddComponent<Animator>();
        var controller = controllerObject.AddComponent<PlayerController>();
        var actions = ScriptableObject.CreateInstance<InputActionAsset>();
        var map = actions.AddActionMap("Player");
        foreach (var name in new[] { "Move", "Jump", "Dash", "Attack1", "Attack2" }) map.AddAction(name);
        SetField(controller, "actions", actions);
        controllerObject.SetActive(true);
        var enemy = new GameObject("Dash boss");
        var boss = enemy.AddComponent<BossHealth>();
        boss.currentHP = 100;
        var child = new GameObject("Boss hurt collider");
        child.transform.SetParent(enemy.transform);
        try
        {
            SetField(controller, "isDashing", true);
            SetField(controller, "dashDamage", 20);
            controller.OnDashHit(child);
            controller.OnDashHit(child);
            Assert.That(boss.currentHP, Is.EqualTo(80));
        }
        finally
        {
            Object.Destroy(controllerObject);
            Object.Destroy(enemy);
            Object.Destroy(actions);
        }
    }

    [Test]
    public void AllEnemyHealthFamilies_ExposeTypedDamageReceiver()
    {
        Assert.That(typeof(IDamageable).IsAssignableFrom(typeof(EnemyHealth)), Is.True);
        Assert.That(typeof(IDamageable).IsAssignableFrom(typeof(EnemyBase)), Is.True);
        Assert.That(typeof(IDamageable).IsAssignableFrom(typeof(BossHealth)), Is.True);
        Assert.That(typeof(IDamageable).IsAssignableFrom(typeof(BossPillar)), Is.True);
    }

    [Test]
    public void MeleeHitbox_CanHitAgainOnNextAttack_WithoutDoubleHittingOneSwing()
    {
        health.damage = 10;
        var hitboxObject = new GameObject("Melee hitbox"); hitboxObject.transform.SetParent(player.transform);
        var collider = hitboxObject.AddComponent<BoxCollider2D>();
        var hitbox = hitboxObject.AddComponent<PlayerAttackHitbox>(); hitbox.enemyLayer = ~0;
        var attack = player.AddComponent<PlayerAttack>(); SetField(attack, "hitboxJ", collider);
        var enemy = new GameObject("Melee boss"); var boss = enemy.AddComponent<BossHealth>(); boss.currentHP = 100;
        var enemyCollider = enemy.AddComponent<BoxCollider2D>();
        try
        {
            attack.PlayAttack(AttackType.J);
            hitbox.SendMessage("OnTriggerEnter2D", enemyCollider);
            hitbox.SendMessage("OnTriggerEnter2D", enemyCollider);
            Assert.That(boss.currentHP, Is.EqualTo(90));
            attack.PlayAttack(AttackType.J);
            hitbox.SendMessage("OnTriggerEnter2D", enemyCollider);
            Assert.That(boss.currentHP, Is.EqualTo(80));
        }
        finally { Object.Destroy(enemy); }
    }

    [Test]
    public void Projectile_HitsMultiColliderEnemyOnce()
    {
        var projectileObject = new GameObject("Test projectile"); var projectile = projectileObject.AddComponent<SlashProjectile>();
        var enemy = new GameObject("Projectile boss"); var boss = enemy.AddComponent<BossHealth>(); boss.currentHP = 100;
        var collider = enemy.AddComponent<BoxCollider2D>();
        try
        {
            projectile.SendMessage("OnTriggerEnter2D", collider);
            projectile.SendMessage("OnTriggerEnter2D", collider);
            Assert.That(boss.currentHP, Is.EqualTo(98));
        }
        finally { Object.Destroy(enemy); Object.Destroy(projectileObject); }
    }

    [Test]
    public void BossDeath_IsNotDispatchedAgainForHitsOnCorpse()
    {
        var enemy = new GameObject("Dying boss"); var boss = enemy.AddComponent<BossHealth>(); boss.currentHP = 10;
        int deaths = 0; boss.OnDeath += () => deaths++;
        try { boss.TakeDamage(20f); boss.TakeDamage(20f); Assert.That(deaths, Is.EqualTo(1)); }
        finally { Object.Destroy(enemy); }
    }

    [UnityTest]
    public IEnumerator LethalBurn_DoesNotContinueAfterRespawn()
    {
        var burn = player.AddComponent<PlayerFireDOT>(); burn.ApplyBurn(1000, 2f, 0.6f);
        yield return new WaitForSeconds(0.8f);
        Assert.That(health.IsDead, Is.False);
        Assert.That(health.CurrentHP, Is.EqualTo(100));
    }

    [Test]
    public void ForcedDeath_DuringInvulnerabilityStillEntersRespawnLifecycle()
    {
        health.TakeDamage(20);
        health.Kill();
        Assert.That(health.IsDead, Is.True);
        Assert.That(health.CurrentHP, Is.Zero);
    }

    private static void SetField(object target, string name, object value)
    {
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    }
}
