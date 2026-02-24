using UnityEngine;

public class EnemyChaseAI : MonoBehaviour
{
    [Header("Detect Box (Horizontal Only)")]
    public float detectWidth = 7f;
    public float detectHeight = 1.5f;
    public LayerMask playerLayer;

    [Header("Direction Chase")]
    public float chaseTime = 2.5f;
    public float chaseSpeedMultiplier = 1.5f;

    QuaiAI ai;
    EnemyRangedAttack ranged;
    Transform player;

    bool wasPlayerInRange;

    float chaseTimer;
    int chaseDir;
    bool isChasing;
    public float SpeedMultiplier => chaseSpeedMultiplier;
    public bool IsChasing => isChasing;
    public int ChaseDirection { get; private set; }

    // ================= INIT =================
    void Awake()
    {
        ai = GetComponent<QuaiAI>();
        ranged = GetComponent<EnemyRangedAttack>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    // ================= TICK =================
    public void Tick()
    {
        if (player == null) return;

        bool inRange = ranged != null && ranged.PlayerInRange;

        // ⭐ mất mục tiêu → chase
        if (wasPlayerInRange && !inRange && !isChasing)
        {
            chaseDir = ai.FacingRight ? 1 : -1;
            chaseTimer = chaseTime;
            isChasing = true;
        }

        wasPlayerInRange = inRange;

        // ⭐ đang bắn → dừng chase
        if (inRange)
        {
            Stop();
            isChasing = false;
            return;
        }

        // ⭐ đang chase
        if (isChasing)
        {
            UpdateChase();
            return;
        }

        // ⭐ detect phía trước
        TryDetectForChase();
    }

    // ================= DETECT =================
    void TryDetectForChase()
    {
        Vector2 center = transform.position;

        Collider2D hit = Physics2D.OverlapBox(
            center,
            new Vector2(detectWidth, detectHeight),
            0,
            playerLayer
        );

        if (!hit) return;

        Vector2 playerPos = hit.transform.position;

        float faceDir = ai.FacingRight ? 1 : -1;
        bool inFront = (playerPos.x - transform.position.x) * faceDir > 0;

        if (!inFront) return;

        chaseDir = playerPos.x > transform.position.x ? 1 : -1;

        chaseTimer = chaseTime;
        isChasing = true;
    }

    // ================= UPDATE =================
    void UpdateChase()
    {
        chaseTimer -= Time.deltaTime;

        ChaseDirection = chaseDir;

        if (chaseTimer <= 0)
        {
            Stop();
            isChasing = false;
        }
    }
    public void StopChase()
    {
        isChasing = false;
        ChaseDirection = 0;
        chaseTimer = 0;
    }
    void Stop()
    {
        ChaseDirection = 0;
    }

    // ================= GIZMO =================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector2(detectWidth, detectHeight));
    }
}
