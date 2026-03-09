using System.Collections;
using UnityEngine;

public class FinalBossMovement : MonoBehaviour
{
    [Header("Movement Logic")]
    public float moveTime = 1.5f;           // Thời gian ước tính để tới điểm đích
    public float minNextMoveDist = 5f;     // Khoảng cách tối thiểu giữa 2 điểm
    public Vector2 arenaMin = new Vector2(-10, 0f);
    public Vector2 arenaMax = new Vector2(10, 3.8f);

    [Header("Dependencies")]
    private FinalBossAttack attackScript;
    private Rigidbody2D rb;
    private Vector3 currentVelocity = Vector3.zero; // Biến phụ cho SmoothDamp
    private bool isBehaviorRunning = true;
    public Animator anim;
    public Transform player;

    void Awake()
    {
        attackScript = GetComponent<FinalBossAttack>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        StartCoroutine(BossMasterRoutine());
    }

    private IEnumerator BossMasterRoutine()
    {
        while (isBehaviorRunning)
        {
            // 1. DI CHUYỂN ĐẾN ĐIỂM NGẪU NHIÊN
            yield return StartCoroutine(MoveToNewLocation());

            // 2. NGHỈ NHẸ TRƯỚC KHI CAST (Cho người chơi chuẩn bị)
            yield return new WaitForSeconds(0.5f);

            // 3. THỰC HIỆN ATTACK
            if (attackScript != null)
            {
                // Gọi hàm PerformAttackRoutine từ script cũ của bạn
                // Lưu ý: Đảm bảo script Attack không có vòng lặp vô tận bên trong
                yield return StartCoroutine(attackScript.TriggerBossAction());
            }

            // 4. COOLDOWN SAU KHI CAST SKILL
            //yield return new WaitForSeconds(0f);
        }
    }

    private IEnumerator MoveToNewLocation()
    {
        anim.SetBool("IsRun", true);
        Vector3 targetPos = GetValidPoint();

        // Quay mặt về hướng mục tiêu
        FaceTarget(player.position);

        // Chạy cho đến khi gần sát đích
        while (Vector3.Distance(transform.position, targetPos) > 0.1f)
        {
            // SmoothDamp giúp tăng tốc và giảm tốc mượt mà
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPos,
                ref currentVelocity,
                moveTime
            );
            yield return null;
        }
        currentVelocity = Vector3.zero; // Reset vận tốc khi dừng
        anim.SetBool("IsRun", false); ;
    }

    private Vector3 GetValidPoint()
    {
        Vector3 point;
        int safetyNet = 0;
        do
        {
            float x = Random.Range(arenaMin.x, arenaMax.x);
            float y = Random.Range(arenaMin.y, arenaMax.y);
            point = new Vector3(x, y, 0);
            safetyNet++;
        } while (Vector3.Distance(transform.position, point) < minNextMoveDist && safetyNet < 10);
        return point;
    }

    // Hàm quay mặt phụ trợ cho di chuyển
    public void FaceTarget(Vector3 target)
    {
        if (target.x > transform.position.x)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    public void stopMove()
    {
        // 1. Dừng NGAY LẬP TỨC các Coroutine di chuyển đang chạy
        StopAllCoroutines();

        // 2. Reset các thông số di chuyển
        currentVelocity = Vector3.zero;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        isBehaviorRunning = false;

        // 3. Ép Boss về trạng thái đứng im (nếu có animation)
        anim.SetBool("IsRun", false);
    }

    // Thêm hàm này để gọi khi Cutscene kết thúc
    public void startMove()
    {
        if (!isBehaviorRunning)
        {
            isBehaviorRunning = true;
            StartCoroutine(BossMasterRoutine());
        }
    }
}