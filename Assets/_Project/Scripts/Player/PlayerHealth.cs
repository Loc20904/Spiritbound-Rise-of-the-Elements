using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Attack Stats")]
    public int damage = 50;
    [Header("Health")]
    public int maxHP = 100;
    [SerializeField] private int currentHP;
    [Header("Defense")]
    public int armor = 5;
    [Header("Hurt / i-frames")]
    [SerializeField] private float hurtInvincibleTime = 0.35f;
    private float nextHurtTime;
    [Header("Respawn")]
    public float respawnDelay = 2f;
    [SerializeField] private Transform respawnPoint;
    [Header("Debug")]
    public bool showDamageLog = true;

    private Rigidbody2D rb;
    private Animator anim;
    private PlayerController controller;
    private PlayerTransformManager forms;
    private Vector3 spawnPosition;
    private PlayerStatManager statSource;
    private int previousMaxHP;
    private Coroutine knockbackRoutine;
    private bool dead;
    private bool respawning;
    private readonly HashSet<object> controlBlockers = new HashSet<object>();

    public bool isKnockback { get; private set; }
    public bool IsDead => dead || respawning;
    public bool CanAct => !IsDead && controlBlockers.Count == 0;
    public int CurrentHP => currentHP;
    public int MaxHP => Mathf.Max(1, Mathf.RoundToInt(maxHP * (PlayerStatManager.Instance?.GetHPMultiplier() ?? 1f)));
    public float HealthPercent => (float)currentHP / MaxHP;
    public int Damage => ScaleDamage(damage, false);
    public int ScaleDamage(int baseDamage, bool skill)
    {
        var stats = PlayerStatManager.Instance;
        float multiplier = (stats?.GetDamageMultiplier() ?? 1f) *
            (skill ? stats?.GetSkillDamageMultiplier() ?? 1f : stats?.GetWeaponDamageMultiplier() ?? 1f);
        return Mathf.Max(0, Mathf.RoundToInt(baseDamage * multiplier));
    }
    public Transform ActiveTransform => forms != null ? forms.ActiveTarget : transform;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        forms = GetComponent<PlayerTransformManager>();
        spawnPosition = transform.position;
        currentHP = MaxHP;
    }

    private void Start()
    {
        spawnPosition = transform.position;
        statSource = PlayerStatManager.EnsureInstance();
        currentHP = MaxHP;
        previousMaxHP = MaxHP;
        statSource.OnStatAssigned += OnStatsChanged;
        LoadPlayer();
    }

    private void OnStatsChanged(PlayerStatProfile profile)
    {
        if (!IsDead && previousMaxHP > 0)
            currentHP = Mathf.Clamp(Mathf.RoundToInt((float)currentHP / previousMaxHP * MaxHP), 1, MaxHP);
        previousMaxHP = MaxHP;
    }
    private void OnDestroy()
    {
        if (statSource != null) statSource.OnStatAssigned -= OnStatsChanged;
    }

    public static PlayerHealth Resolve(GameObject source) => source != null ? Resolve(source.transform) : null;

    public static PlayerHealth Resolve(Component source)
    {
        if (source == null) return null;
        var owner = source.GetComponentInParent<PlayerHealth>(true);
        if (owner != null) return owner;
        return source.GetComponentInParent<PlayerHealthLink>(true)?.Owner;
    }

    public void SetControlsBlocked(object owner, bool blocked)
    {
        if (owner == null) return;
        if (blocked) controlBlockers.Add(owner);
        else controlBlockers.Remove(owner);
        if (blocked)
        {
            controller?.StopMove();
            forms?.StopActiveMovement();
        }
    }

    public void TakeDamage(int incomingDamage) => TakeDamage(incomingDamage, DamageType.Boss);

    public void TakeDamage(int incomingDamage, DamageType damageType)
    {
        if (IsDead || incomingDamage <= 0 || Time.time < nextHurtTime) return;
        var modifiers = PlayerStatManager.Instance;
        int effectiveArmor = Mathf.Max(0, Mathf.RoundToInt(armor * (modifiers?.GetArmorDefenseMultiplier() ?? 1f)));
        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(incomingDamage - effectiveArmor, 0) *
            Mathf.Max(0f, modifiers?.GetDamageTakenMultiplier() ?? 1f)));
        if (finalDamage == 0) return;
        nextHurtTime = Time.time + hurtInvincibleTime;
        currentHP = Mathf.Max(0, currentHP - finalDamage);
        controller?.TakeHit();
        SendMessage("OnTakeDamage", new DamageInfo { damage = finalDamage, damageType = damageType },
            SendMessageOptions.DontRequireReceiver);
        if (showDamageLog)
            Debug.Log($"[PlayerHealth] Damage: {finalDamage} | HP: {currentHP}/{MaxHP}");
        if (currentHP == 0) Die();
    }

    public void TakeDamage(int incomingDamage, Vector2 hitPoint) => TakeDamage(incomingDamage);

    public void ApplyKnockback(Vector2 force, float duration)
    {
        // Only replace knockback; never cancel the respawn coroutine.
        if (IsDead || duration <= 0f) return;
        StopKnockback();
        controller?.StopMove();
        forms?.StopActiveMovement();
        knockbackRoutine = StartCoroutine(KnockbackRoutine(force, duration));
    }

    private IEnumerator KnockbackRoutine(Vector2 force, float duration)
    {
        isKnockback = true;
        var body = forms != null ? forms.ActiveBody : rb;
        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
            body.AddForce(force, ForceMode2D.Impulse);
        }
        yield return new WaitForSeconds(duration);
        isKnockback = false;
        knockbackRoutine = null;
    }

    private void StopKnockback()
    {
        if (knockbackRoutine != null) StopCoroutine(knockbackRoutine);
        knockbackRoutine = null;
        isKnockback = false;
    }

    public void Kill() => Die();

    private void Die()
    {
        if (IsDead) return;
        dead = true;
        respawning = true;
        currentHP = 0;
        StopKnockback();
        GetComponent<PlayerFireDOT>()?.ClearBurn();
        forms?.ReturnToNormalImmediately();
        controller?.StopMove();
        if (controller != null) controller.enabled = false;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (anim != null) anim.SetBool("Isdead", true);
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(Mathf.Max(0, respawnDelay));
        transform.position = respawnPoint != null ? respawnPoint.position : spawnPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = true;
        }
        currentHP = MaxHP;
        dead = false;
        respawning = false;
        nextHurtTime = Time.time + 0.5f;
        if (anim != null)
        {
            anim.SetBool("Isdead", false);
            anim.Play("Player_Idle", 0, 0f);
        }
        if (controller != null) controller.enabled = true;
    }

    public void ResetHealth()
    {
        if (!IsDead) currentHP = MaxHP;
    }

    public void Heal(int amount)
    {
        if (!IsDead && amount > 0) currentHP = (int)System.Math.Min((long)currentHP + amount, MaxHP);
    }

    private void LoadPlayer()
    {
        if (!GameSession.TryConsumePlayerRestore(gameObject.scene.name, out var data)) return;
        transform.position = new Vector3(data.playerX, data.playerY, transform.position.z);
        spawnPosition = transform.position;
        currentHP = Mathf.Clamp(data.playerHP, 1, MaxHP);
    }
}

public enum DamageType { Boss, FireDOT }
public struct DamageInfo
{
    public int damage;
    public DamageType damageType;
}
