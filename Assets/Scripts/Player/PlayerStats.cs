using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Combat Stats")]
    public int damage = 50;
    public int armor = 5;

    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Respawn")]
    public float respawnDelay = 2f; // ✅ delay sau khi chết
    [SerializeField] private Transform respawnPoint;

    [Header("Hurt / i-frames")]
    [SerializeField] private float hurtInvincibleTime = 0.35f;
    private float nextHurtTime;

    private Animator anim;
    private PlayerController controller;
    private bool dead = false;

    private void Start()
    {
        currentHP = maxHP;

        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
    }

    public void TakeDamage(int incomingDamage)
    {
        if (dead) return;

        // i-frame chống hit liên tục
        if (Time.time < nextHurtTime) return;
        nextHurtTime = Time.time + hurtInvincibleTime;

        int finalDamage = Mathf.Max(incomingDamage - armor, 0);
        currentHP -= finalDamage;

        Debug.Log($"[PlayerStats] -{finalDamage} HP => {currentHP}/{maxHP}");

        controller?.TakeHit();

        if (currentHP <= 0)
            Die();
    }

    private void Die()
    {
        if (dead) return;
        dead = true;
        currentHP = 0;

        // bật animation chết
        if (anim != null)
            anim.SetBool("Isdead", true);

        // tắt điều khiển
        if (controller != null)
            controller.enabled = false;

        // bắt đầu respawn sau delay
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        // ✅ chờ chết xong
        yield return new WaitForSeconds(respawnDelay);

        // teleport về checkpoint
        if (respawnPoint != null)
            transform.position = respawnPoint.position;

        // reset velocity
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        // hồi sinh lại máu
        dead = false;
        currentHP = maxHP;

        // thoát animation chết
        if (anim != null)
        {
            anim.SetBool("Isdead", false);

            // ép về idle tránh kẹt state
            anim.Play("Player_Idle", 0, 0f);
        }

        // bật lại điều khiển
        if (controller != null)
            controller.enabled = true;

        // miễn nhiễm 0.5s sau spawn
        nextHurtTime = Time.time + 0.5f;
    }
}