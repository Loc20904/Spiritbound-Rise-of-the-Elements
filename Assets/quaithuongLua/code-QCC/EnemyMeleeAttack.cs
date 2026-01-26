using UnityEngine;
using System.Collections;

public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Detect Box (Melee Range)")]
    public float detectWidth = 2.2f;
    public float detectHeight = 1.6f;
    public LayerMask playerLayer;

    [Header("Damage")]
    public int damage = 20;

    [Header("Fire Damage (Giống boss đánh xa)")]
    public int damagePerTick = 2;
    public float burnDuration = 3f;
    public float tickRate = 0.5f;

    [Header("Attack")]
    public float attackCooldown = 1.2f;

    [Header("Attack Animation")]
    public Sprite[] attackFrames;
    public float frameRate = 0.05f;
    public int[] hitFrameIndexes; // frame gây damage

    public bool PlayerInRange { get; private set; }
    public Vector2 PlayerPosition { get; private set; }
    public bool IsAttacking => isAttacking;

    Coroutine attackRoutine;
    SpriteRenderer sr;

    bool isAttacking;
    float cooldownTimer;

    // ================= START =================
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // ================= UPDATE =================
    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (isAttacking || cooldownTimer > 0)
            return;

        DetectPlayer();
    }

    // ================= DETECT (BOX) =================
    void DetectPlayer()
    {
        PlayerInRange = false;

        Vector2 center = transform.position;

        Collider2D hit = Physics2D.OverlapBox(
            center,
            new Vector2(detectWidth, detectHeight),
            0,
            playerLayer
        );

        if (!hit)
            return;

        Vector2 playerPos = hit.transform.position;

        // ⭐ chỉ phía trước (GIỮ Y NGUYÊN LOGIC CŨ)
        float dirX = transform.localScale.x > 0 ? 1 : -1;

        bool isFront = (playerPos.x - transform.position.x) * dirX > 0;

        if (!isFront)
            return;

        PlayerInRange = true;
        PlayerPosition = playerPos;

        attackRoutine = StartCoroutine(AttackRoutine(hit.gameObject));
    }

    // ================= ATTACK =================
    IEnumerator AttackRoutine(GameObject player)
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;

        for (int i = 0; i < attackFrames.Length; i++)
        {
            sr.sprite = attackFrames[i];

            foreach (int index in hitFrameIndexes)
            {
                if (i == index)
                {
                    HitPlayer(player);
                    break;
                }
            }

            yield return new WaitForSeconds(frameRate);
        }

        isAttacking = false;
    }

    // ================= HIT =================
    void HitPlayer(GameObject player)
    {
        if (!player) return;

        // ⭐ Gửi damage với type Boss
        PlayerHealth health = player.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage, DamageType.Boss);
        }
        else
        {
            // Fallback nếu không có PlayerHealth
            player.SendMessage(
                "TakeDamage",
                damage,
                SendMessageOptions.DontRequireReceiver
            );
        }

        // ⭐ Gọi PlayerFireDOT giống boss đánh xa
        PlayerFireDOT fire = player.GetComponent<PlayerFireDOT>();
        if (fire != null)
            fire.ApplyBurn(damagePerTick, burnDuration, tickRate);
    }

    // ================= STOP ATTACK (cho EnemyHealth) =================
    public void StopAttack()
    {
        isAttacking = false;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);
    }

    // ================= GIZMO =================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 center = transform.position;

        Gizmos.DrawWireCube(center, new Vector2(detectWidth, detectHeight));
    }
}
