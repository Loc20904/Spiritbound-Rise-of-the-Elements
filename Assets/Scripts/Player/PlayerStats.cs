using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Combat Stats")]
    public int damage = 50;     // ✅ sức đánh
    public int armor = 5;       // ✅ giáp (giảm sát thương)

    [Header("Health")]
    public int maxHP = 100;
    public int currentHP;

    [Header("Respawn")]
    public float respawnDelay = 5f;

    [Header("Hurt / i-frames")]
    [SerializeField] private float hurtInvincibleTime = 0.35f; // 0.25~0.5 tuỳ bạn
    private float nextHurtTime;


    private Animator anim;
    private bool dead = false;

    private PlayerController controller;

    private void Start()
    {
        currentHP = maxHP;
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
    }

    // Enemy gọi hàm này để gây sát thương lên player
    public void TakeDamage(int incomingDamage)
    {
        if (dead) return;

        // ✅ chỉ nhận hit 1 lần trong khoảng i-frame
        if (Time.time < nextHurtTime) return;
        nextHurtTime = Time.time + hurtInvincibleTime;

        int finalDamage = Mathf.Max(incomingDamage - armor, 0);
        currentHP -= finalDamage;
        Debug.Log($"[PlayerStats] -{finalDamage} HP => {currentHP}/{maxHP}");

        // ✅ trigger hit anim ngay lập tức
        controller?.TakeHit();

        if (currentHP <= 0) Die();
    }

    private void Die()
    {
        dead = true;
        currentHP = 0;

        // bật anim chết
        if (anim != null)
            anim.SetBool("Isdead", true);

        // tắt điều khiển
        if (controller != null)
            controller.enabled = false;

        // bắt đầu hồi sinh sau 5s
        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(respawnDelay);

        // hồi sinh
        dead = false;
        currentHP = maxHP;

        // tắt trạng thái chết để quay lại idle/run
        if (anim != null)
            anim.SetBool("Isdead", false);

        // bật lại điều khiển
        if (controller != null)
            controller.enabled = true;

        // reset vận tốc nếu dùng Rigidbody2D
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
