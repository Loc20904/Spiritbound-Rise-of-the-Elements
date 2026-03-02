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

        // ✅ tính sát thương sau khi trừ giáp
        int finalDamage = Mathf.Max(incomingDamage - armor, 0);

        currentHP -= finalDamage;
        Debug.Log($"Player nhận damage: {finalDamage} | HP còn: {currentHP}");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        dead = true;
        currentHP = 0;

        // bật anim chết
        if (anim != null)
            anim.SetBool("IsDead", true);

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
            anim.SetBool("IsDead", false);

        // bật lại điều khiển
        if (controller != null)
            controller.enabled = true;

        // reset vận tốc nếu dùng Rigidbody2D
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }
}
