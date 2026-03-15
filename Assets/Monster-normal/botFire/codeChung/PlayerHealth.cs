using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Attack Stats")]
    public int damage = 50; // ✅ damage cơ bản của player

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

    public bool isKnockback { get; private set; }

    private bool dead = false;
    private bool respawning = false;

    void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
    }

    // ================= TAKE DAMAGE =================
    public void TakeDamage(int incomingDamage)
    {
        TakeDamage(incomingDamage, DamageType.Boss);
    }

    public void TakeDamage(int incomingDamage, DamageType damageType)
    {
        if (dead || respawning) return;
        if (currentHP <= 0) return;

        // i-frame chống hit liên tục
        if (Time.time < nextHurtTime) return;
        nextHurtTime = Time.time + hurtInvincibleTime;

        int finalDamage = Mathf.Max(incomingDamage - armor, 0);

        // Trigger hit animation / effect
        controller?.TakeHit();

        // Trừ máu
        currentHP -= finalDamage;
        currentHP = Mathf.Max(0, currentHP);

        // Show damage popup/UI
        ShowDamageInGame(finalDamage, damageType);

        if (showDamageLog)
        {
            string src = damageType == DamageType.Boss ? "Boss" : "Fire DOT";
            Debug.Log($"[PlayerHealth] Incoming:{incomingDamage} Armor:{armor} Final:{finalDamage} From:{src} | HP: {currentHP}/{maxHP}");
        }

        if (currentHP <= 0)
            Die();
    }

    // Spike / hazard overload
    public void TakeDamage(int incomingDamage, Vector2 hitPoint)
    {
        TakeDamage(incomingDamage, DamageType.Boss);

        if (showDamageLog)
            Debug.Log($"[PlayerHealth] Spike hit at {hitPoint}");
    }

    // ================= KNOCKBACK =================
    public void ApplyKnockback(Vector2 force, float duration)
    {
        StopAllCoroutines();
        StartCoroutine(KnockbackRoutine(force, duration));
    }

    IEnumerator KnockbackRoutine(Vector2 force, float duration)
    {
        isKnockback = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(force, ForceMode2D.Impulse); //Nếu PlayerController không chặn, mỗi frame nó lại set velocity mới → lực bị hủy ngay.
        }

        yield return new WaitForSeconds(duration);

        isKnockback = false;
    }

    // ================= SHOW DAMAGE IN GAME =================
    void ShowDamageInGame(int damage, DamageType damageType)
    {
        SendMessage(
            "OnTakeDamage",
            new DamageInfo { damage = damage, damageType = damageType },
            SendMessageOptions.DontRequireReceiver
        );
    }

    // ================= DIE + RESPAWN =================
    void Die()
    {
        if (dead) return;
        dead = true;
        currentHP = 0;

        Debug.Log("[PlayerHealth] Player đã chết!");

        if (anim != null)
            anim.SetBool("Isdead", true);

        if (controller != null)
            controller.enabled = false;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        respawning = true;

        yield return new WaitForSeconds(respawnDelay);

        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        ResetHealth();

        if (anim != null)
        {
            anim.SetBool("Isdead", false);
            anim.Play("Player_Idle", 0, 0f);
        }

        if (controller != null)
            controller.enabled = true;

        dead = false;
        respawning = false;

        // miễn nhiễm 0.5s sau spawn
        nextHurtTime = Time.time + 0.5f;
    }

    // ================= PUBLIC =================
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public float HealthPercent => maxHP > 0 ? (float)currentHP / maxHP : 0f;

    // ✅ expose damage nếu script khác muốn lấy
    public int Damage => damage;

    public void ResetHealth()
    {
        currentHP = maxHP;

        if (showDamageLog)
            Debug.Log($"[PlayerHealth] Reset HP: {currentHP}/{maxHP}");
    }
}

// ================= DAMAGE TYPE =================
public enum DamageType
{
    Boss,
    FireDOT
}

// ================= DAMAGE INFO =================
public struct DamageInfo
{
    public int damage;
    public DamageType damageType;
}