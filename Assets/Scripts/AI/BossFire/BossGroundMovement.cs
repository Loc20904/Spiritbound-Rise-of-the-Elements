using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossGroundMovement : MonoBehaviour
{
    [Header("Movement Stats")]
    public float moveSpeed = 4f;
    public float dashSpeed = 15f;
    public float stopDistance = 0.5f; // Khoảng cách dừng để tránh rung lắc

    [Header("Dash Settings")]
    public float dashStunDuration = 2f;
    public float dashKnockback = 10f;

    [Header("Settings - QUAN TRỌNG")]
    [Tooltip("Tích vào nếu hình gốc Boss vẽ mặt hướng sang Phải. Bỏ tích nếu hình gốc hướng sang Trái.")]
    public bool spriteFacesRight = true;

    private Rigidbody2D rb;
    private Transform player;
    private bool isDashing = false;
    private bool canMove = true;
    private float facingDirection = 1f;
    private Animator anim;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.freezeRotation = true;

        // Giúp va chạm khi Dash chính xác hơn (tránh xuyên tường/xuyên player)
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void Start()
    {
        if (GameObject.FindGameObjectWithTag("Player"))
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        // Cập nhật animation đi bộ
        if (anim != null)
        {
            // Chỉ tính là đang di chuyển nếu vận tốc X đáng kể và KHÔNG phải đang dash
            // (Dùng rb.velocity nếu Unity bản cũ, dùng rb.linearVelocity nếu Unity 6)
            bool moving = Mathf.Abs(rb.linearVelocity.x) > 0.1f && !isDashing;
            anim.SetBool("isMoving", moving);
        }
    }

    // --- LOGIC DI CHUYỂN ---
    public void MoveToPlayer()
    {
        if (player == null || isDashing || !canMove) return;

        // Tính khoảng cách theo trục X
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);

        // [FIX MƯỢT] Nếu Boss đã đứng rất gần Player rồi thì cho dừng lại
        if (distanceX < stopDistance)
        {
            Stop();
            return;
        }

        // 1. Tính hướng di chuyển
        float dirX = (player.position.x > transform.position.x) ? 1f : -1f;

        // Lưu hướng hiện tại để dùng cho Dash (để Dash không bị đổi hướng giữa chừng)
        facingDirection = dirX;

        // 2. Di chuyển vật lý
        // Lưu ý: Nếu Unity báo lỗi linearVelocity, hãy đổi thành rb.velocity
        rb.linearVelocity = new Vector2(dirX * moveSpeed, rb.linearVelocity.y);

        // 3. Xử lý quay mặt (Flip)
        HandleFlip(dirX);
    }

    void HandleFlip(float dirX)
    {
        Vector3 currentScale = transform.localScale;
        currentScale.x = Mathf.Abs(currentScale.x); // Reset về dương

        if (dirX > 0) // Player bên Phải
            currentScale.x = spriteFacesRight ? currentScale.x : -currentScale.x;
        else // Player bên Trái
            currentScale.x = spriteFacesRight ? -currentScale.x : currentScale.x;

        transform.localScale = currentScale;
    }

    public void Stop()
    {
        if (isDashing) return;
        // Lưu ý: Nếu Unity báo lỗi linearVelocity, hãy đổi thành rb.velocity
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    // --- LOGIC DASH & STUN ---
    public void DashForward(float duration)
    {
        StartCoroutine(DashRoutine(duration));
    }

    System.Collections.IEnumerator DashRoutine(float duration)
    {
        isDashing = true;

        // Khi Dash, ta ép vận tốc thẳng theo hướng mặt đang nhìn
        rb.linearVelocity = new Vector2(facingDirection * dashSpeed, rb.linearVelocity.y);

        yield return new WaitForSeconds(duration);

        // Kết thúc Dash: Dừng lại ngay lập tức để không bị trượt
        rb.linearVelocity = Vector2.zero;
        isDashing = false;
    }

    // XỬ LÝ VA CHẠM KHI DASH (GÂY CHOÁNG)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Chỉ xử lý nếu Boss ĐANG DASH và va chạm với PLAYER
        if (isDashing && collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Boss húc trúng Player!");

            Rigidbody2D playerRb = collision.gameObject.GetComponent<Rigidbody2D>();

            // 1. Đẩy lùi Player
            if (playerRb != null)
            {
                Vector2 knockDir = (collision.transform.position - transform.position).normalized;
                // Đẩy lên một chút để tạo cảm giác bị hất tung (y = 0.5f)
                knockDir.y = 0.5f;
                playerRb.linearVelocity = Vector2.zero; // Reset vận tốc cũ để đẩy cho mạnh
                playerRb.AddForce(knockDir * dashKnockback, ForceMode2D.Impulse);
            }

            // 2. Gây choáng (Stun)
            // Cách 1: Dùng SendMessage (Nhanh, gọn, không cần biết tên script Player)
            collision.gameObject.SendMessage("Stun", dashStunDuration, SendMessageOptions.DontRequireReceiver);

            // Cách 2: Gọi trực tiếp (Tốt hơn về hiệu năng, nhưng bạn phải thay đúng tên script Player của bạn)
            /*
            var pController = collision.gameObject.GetComponent<PlayerController>();
            if (pController != null)
            {
                pController.Stun(dashStunDuration);
            }
            */
        }
    }

    public void SetMove(bool allow)
    {
        canMove = allow;
        if (!allow) Stop();
    }
}