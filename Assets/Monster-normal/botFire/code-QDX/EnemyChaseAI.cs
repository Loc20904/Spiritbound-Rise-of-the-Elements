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
        if (player == null) return; /// Không tìm thấy player, không cần tiếp tục

        bool inRange = ranged != null && ranged.PlayerInRange; //kiểm tra nếu player đang trong tầm bắn của EnemyRangedAttack

        // Nếu vừa mất player → bắt đầu chase
        // trường hợp 3 cái này true khi player vừa rời khỏi tầm bắn: wasPlayerInRange = true, inRange = false, isChasing = false → bắt đầu chase
        if (wasPlayerInRange && !inRange && !isChasing)
        {
            chaseDir = ai.FacingRight ? 1 : -1;
            chaseTimer = chaseTime;
            isChasing = true;
        }

        wasPlayerInRange = inRange;

        // Nếu trong tầm bắn → dừng chase
        if (inRange)
        {
            Stop();
            isChasing = false;
            return;
        }

        // isChasing phải true thì mới update chase, nếu không thì sẽ tiếp tục detect phía trước để bắt đầu chase khi phát hiện player
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
        Vector2 center = transform.position;// lấy vị trí của Enemy làm tâm của box detect

        Collider2D hit = Physics2D.OverlapBox(
            center,
            new Vector2(detectWidth, detectHeight),
            0,
            playerLayer
        ); //Tạo một box detect với kích thước detectWidth x detectHeight và chỉ phát hiện các collider thuộc playerLayer

        if (!hit) return;//neu không phát hiện được player nào trong box thì dừng lại

        Vector2 playerPos = hit.transform.position;//lấy vị trí của player được phát hiện

        float faceDir = ai.FacingRight ? 1 : -1; //xác định hướng mà Enemy đang đối mặt (1 nếu đang đối mặt phải, -1 nếu đang đối mặt trái)
        bool inFront = (playerPos.x - transform.position.x) * faceDir > 0; //kiểm tra xem player có nằm ở phía trước của Enemy hay không bằng cách tính hiệu giữa vị trí player và vị trí Enemy, sau đó nhân với hướng đối mặt. Nếu kết quả lớn hơn 0, nghĩa là player nằm ở phía trước của Enemy.

        if (!inFront) return;//nếu player không nằm ở phía trước của Enemy thì dừng chạy

        chaseDir = playerPos.x > transform.position.x ? 1 : -1; //xác định hướng chase dựa trên vị trí của player so với Enemy. Nếu player nằm ở bên phải của Enemy, chaseDir sẽ là 1 (chase sang phải), ngược lại sẽ là -1 (chase sang trái)

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
