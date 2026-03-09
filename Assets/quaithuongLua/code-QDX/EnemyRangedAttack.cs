using UnityEngine;
using System.Collections;

public class EnemyRangedAttack : MonoBehaviour
{
    [Header("Detect Box (Attack Range)")]
    public float detectWidth = 6f;   // ngang
    public float detectHeight = 2f;  // dọc
    public LayerMask playerLayer;

    [Header("Shoot")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float attackCooldown = 2f;

    [Header("Attack Animation")]
    public Sprite[] attackFrames;
    public float frameRate = 0.05f;
    public int[] fireFrameIndexes;

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

        // ⭐ chỉ phía trước
        float dirX = transform.localScale.x > 0 ? 1 : -1;

        bool isFront = (playerPos.x - transform.position.x) * dirX > 0;

        if (!isFront)
            return;

        PlayerInRange = true;
        PlayerPosition = playerPos;

        attackRoutine = StartCoroutine(AttackRoutine());
    }

    // ================= ATTACK =================
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;

        for (int i = 0; i < attackFrames.Length; i++)
        {
            sr.sprite = attackFrames[i];

            foreach (int index in fireFrameIndexes)
            {
                if (i == index)
                {
                    FireArrow();
                    break;
                }
            }

            yield return new WaitForSeconds(frameRate);
        }

        isAttacking = false;
    }

    // ================= FIRE =================
    void FireArrow()
    {
        Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, Quaternion.identity);
        ArrowBullet bullet = arrow.GetComponent<ArrowBullet>();

        if (bullet != null)
            bullet.Init(dir, GetComponent<Collider2D>());
    }
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
