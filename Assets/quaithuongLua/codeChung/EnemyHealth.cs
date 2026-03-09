using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHP = 200;
    int currentHP;

    [Header("Death Animation")]
    public Sprite[] deathFrames;
    public float deathFrameRate = 0.08f;

    bool isDead;

    SpriteRenderer sr;
    Rigidbody2D rb;
    Collider2D col;

    QuaiAI ai;
    EnemyChaseAI chase;
    EnemyMeleeAttack melee;
    EnemyRangedAttack ranged;

    void Awake()
    {
        currentHP = maxHP;

        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        ai = GetComponent<QuaiAI>();
        chase = GetComponent<EnemyChaseAI>();
        melee = GetComponent<EnemyMeleeAttack>();
        ranged = GetComponent<EnemyRangedAttack>();
    }

    public void TakeDamage(int dmg)
    {
        if (isDead) return;

        currentHP -= dmg;

        // ⭐ bị đánh → quay mặt player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player && ai)
        {
            bool playerRight = player.transform.position.x > transform.position.x;
            if (playerRight != ai.FacingRight)
                ai.FlipPublic();
        }

        if (currentHP <= 0)
        {
            DieImmediate();
        }
    }

    // =====================================================
    // DIE
    // =====================================================
    void DieImmediate()
    {
        isDead = true;

        // ⭐ CẮT TOÀN BỘ ATTACK NGAY
        if (melee)
        {
            melee.StopAllCoroutines();
            melee.enabled = false;
        }

        if (ranged)
        {
            ranged.StopAllCoroutines();
            ranged.enabled = false;
        }

        // ⭐ CẮT AI + CHASE
        if (ai) ai.enabled = false;
        if (chase) chase.enabled = false;

        // ⭐ VẬT LÝ
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.simulated = false;
        }

        if (col) col.enabled = false;

        // ⭐ ĐẢM BẢO KHÔNG CÒN SCRIPT NÀO SET SPRITE
        StopAllCoroutines();

        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        for (int i = 0; i < deathFrames.Length; i++)
        {
            sr.sprite = deathFrames[i];
            yield return new WaitForSeconds(deathFrameRate);
        }

        Destroy(gameObject); // tuỳ bạn
    }
}
