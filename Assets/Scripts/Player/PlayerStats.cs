using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Combat Stats")]
    public int damage = 50;
    public int armor = 5;

    [Header("Respawn")]
    public float respawnDelay = 2f;
    [SerializeField] private Transform respawnPoint;

    [Header("Hurt / i-frames")]
    [SerializeField] private float hurtInvincibleTime = 0.35f;
    private float nextHurtTime;

    private Animator anim;
    private PlayerController controller;
    private Rigidbody2D rb;

    private PlayerHealth health;

    private bool dead = false;
    private bool respawning = false;

    private void Start()
    {
        health = GetComponent<PlayerHealth>();
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        if (health == null)
            Debug.LogError("[PlayerStats] Missing PlayerHealth on this GameObject!");
    }

    private void Update()
    {
        if (dead || respawning) return;
        if (health == null) return;

        // ✅ chết bởi bất kỳ nguồn nào miễn HP về 0
        if (health.CurrentHP <= 0)
            OnDead();
    }

    public void TakeDamage(int incomingDamage)
    {
        if (dead || respawning) return;
        if (health == null) return;

        // i-frame chống hit liên tục
        if (Time.time < nextHurtTime) return;
        nextHurtTime = Time.time + hurtInvincibleTime;

        int finalDamage = Mathf.Max(incomingDamage - armor, 0);

        // ✅ trừ máu qua PlayerHealth (không dùng currentHP trong PlayerStats nữa)
        if (finalDamage > 0)
            health.TakeDamage(finalDamage, DamageType.Boss);

        Debug.Log($"[PlayerStats] Incoming:{incomingDamage} Armor:{armor} Final:{finalDamage} => HP {health.CurrentHP}/{health.MaxHP}");

        controller?.TakeHit();
    }

    private void OnDead()
    {
        if (dead) return;
        dead = true;

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

        // teleport về checkpoint
        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        // reset velocity
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // ✅ hồi máu từ PlayerHealth
        if (health != null)
            health.ResetHealth();

        // thoát animation chết
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
}