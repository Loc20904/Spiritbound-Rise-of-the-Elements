using UnityEngine;
using System.Collections;

public class EnemyMeleeAttack : MonoBehaviour
{
    [Header("Detect Box (Melee Range)")]
    public float detectWidth = 2.2f;
    public float detectHeight = 1.6f;
    public LayerMask playerLayer;

    [Header("Earth Knockback")]
    public float earthKnockbackForce = 8f;
    public float earthKnockbackDuration = 0.2f;
    [Header("Damage")]
    public int damage = 20;
    public enum ElementType
    {
        None,
        Fire,
        Earth
    }
    [Header("Element")]
    public ElementType elementType = ElementType.None;
    [Header("Fire Damage ")]
    public int fireDamagePerTick = 2;
    public float fireBurnDuration = 3f;
    public float fireTickRate = 0.5f;

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
        Collider2D hit = Physics2D.OverlapBox(center, new Vector2(detectWidth, detectHeight), 0, playerLayer);

        if (!hit) return;

        if (CheckIfTargetInRange(hit.gameObject))
        {
            PlayerInRange = true;
            PlayerPosition = hit.transform.position;
            attackRoutine = StartCoroutine(AttackRoutine(hit.gameObject));
        }
    }

    bool CheckIfTargetInRange(GameObject target)
    {
        if (target == null) return false;

        Vector2 center = transform.position;
        Collider2D hit = Physics2D.OverlapBox(center, new Vector2(detectWidth, detectHeight), 0, playerLayer);

        // Phải trúng collider của chính target đó
        if (!hit || hit.gameObject != target) return false;

        // Phải ở phía trước mặt
        float dirX = transform.localScale.x > 0 ? 1 : -1;
        bool isFront = (target.transform.position.x - transform.position.x) * dirX > 0;
        
        return isFront;
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
        // ⭐ Kiểm tra lại tầm đánh TẠI THỜI ĐIỂM gây sát thương
        if (!CheckIfTargetInRange(player)) return;

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
        if (elementType == ElementType.Fire)
        {
            PlayerFireDOT fire = player.GetComponent<PlayerFireDOT>();
            if (fire != null)
                fire.ApplyBurn(fireDamagePerTick, fireBurnDuration, fireTickRate);
        }

        if (elementType == ElementType.Earth)
        {
            if (health != null)
            {
                Vector2 dir = (player.transform.position - transform.position).normalized;
                Vector2 force = dir * earthKnockbackForce;

                health.ApplyKnockback(force, earthKnockbackDuration);
            }
        }
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
