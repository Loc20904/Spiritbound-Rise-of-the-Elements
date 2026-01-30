using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossWaterMovement : MonoBehaviour
{
    [Header("Hover Settings")]
    public float hoverAmplitude = 0.5f; // Độ nhấp nhô
    public float hoverFrequency = 1f;   // Tốc độ nhấp nhô

    [Header("Teleport Settings")]
    public Transform[] teleportPoints;  // Các điểm Boss có thể xuất hiện
    public float disappearDuration = 1f; // Thời gian biến mất
    public float reappearDuration = 1f;  // Thời gian hiện lại

    [Header("Setup")]
    public bool spriteFacesRight = true;

    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D col;

    private Vector3 startPosition;
    private bool isTeleporting = false;

    // Biến này giúp khóa Boss đứng im khi cần (ví dụ lúc đang gồng chiêu)
    private bool canMove = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        //rb.gravityScale = 0;
    }

    void Start()
    {
        if (GameObject.FindGameObjectWithTag("Player"))
            player = GameObject.FindGameObjectWithTag("Player").transform;

        startPosition = transform.position;
    }

    void Update()
    {
        // Thêm điều kiện !canMove: Nếu đang bị khóa thì không chạy logic bên dưới
        if (player == null || isTeleporting || !canMove) return;

        // 1. Luôn quay mặt về phía Player
        FacePlayer();

        //FloatEffect();
    }

    void FloatEffect()
    {
        // Nhấp nhô nhẹ theo hình sin
        float newY = startPosition.y + Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    public void FacePlayer()
    {
        if (player == null) return;
        float dirX = (player.position.x > transform.position.x) ? 1f : -1f;
        HandleFlip(dirX);
    }

    void HandleFlip(float dirX)
    {
        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x);

        if (dirX > 0)
            currentScale.x = spriteFacesRight ? currentScale.x : -currentScale.x;
        else
            currentScale.x = spriteFacesRight ? -currentScale.x : currentScale.x;

        transform.localScale = currentScale;
    }

    // --- LOGIC DỊCH CHUYỂN (TELEPORT) ---
    public IEnumerator TeleportRandomly()
    {
        if (teleportPoints.Length == 0) yield break;

        isTeleporting = true;

        if (anim) anim.SetTrigger("teleportOut");
        if (col) col.enabled = false;

        yield return new WaitForSeconds(disappearDuration);

        Transform targetPoint = GetRandomPointFarFromPlayer();
        transform.position = targetPoint.position;
        startPosition = transform.position; // Reset tâm nhấp nhô tại vị trí mới

        if (anim) anim.SetTrigger("teleportIn");

        yield return new WaitForSeconds(reappearDuration);

        if (col) col.enabled = true;
        isTeleporting = false;
    }

    Transform GetRandomPointFarFromPlayer()
    {
        if (teleportPoints.Length == 0) return transform;

        Transform bestPoint = teleportPoints[0];
        float maxDist = 0f;

        foreach (var point in teleportPoints)
        {
            float d = Vector2.Distance(point.position, player.position);
            if (d > maxDist)
            {
                maxDist = d;
                bestPoint = point;
            }
        }
        return bestPoint;
    }

    // --- CÁC HÀM ĐIỀU KHIỂN TRẠNG THÁI (STOP/MOVE) ---

    public void Stop()
    {
        // 1. Dừng vật lý ngay lập tức
        if (rb) rb.linearVelocity = Vector2.zero;
        // Lưu ý: Nếu Unity báo lỗi linearVelocity, hãy đổi thành rb.velocity

        // 2. Khóa logic nhấp nhô và quay mặt
        canMove = false;
    }

    public void SetMove(bool allow)
    {
        canMove = allow;

        if (!allow)
        {
            Stop();
        }
        else
        {
            // Khi cho phép di chuyển lại, cập nhật lại startPosition tại vị trí hiện tại
            // Để Boss không bị giật (snap) về vị trí cũ của hình sin
            startPosition = transform.position;
        }
    }
}