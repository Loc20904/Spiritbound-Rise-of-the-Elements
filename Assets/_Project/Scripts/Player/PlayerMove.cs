using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;

    [Header("Environment Modifiers")]
    public float speedMultiplier = 1f; // Dùng cho Mud (đầm lầy)
    public bool isSlippery = false;    // Dùng cho Ice (mặt băng trơn trượt)
    public bool isReversed = false;    // Dùng cho Void (đảo ngược điều khiển)

    private Rigidbody2D rb;
    private Vector2 movement;
    private Vector2 currentVelocity; // Variable hệ thống cần cho SmoothDamp khi trượt băng

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Nhận input từ bàn phím / gamepad
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        // Nếu dính hiệu ứng của Boss Void -> Đảo ngược nút bấm
        if (isReversed)
        {
            inputX = -inputX;
            inputY = -inputY;
        }

        // Chuẩn hóa vector để đi chéo không bị nhanh hơn đi thẳng
        movement = new Vector2(inputX, inputY).normalized;
    }

    void FixedUpdate()
    {
        // Tính toán tốc độ gốc = (Hướng) * (Tốc độ cơ bản) * (Hệ số làm chậm/tăng tốc)
        Vector2 targetVelocity = movement * moveSpeed * speedMultiplier;

        if (isSlippery)
        {
            // Trượt băng: Dùng SmoothDamp để từ từ đạt được tốc độ mục tiêu hoặc từ từ dừng lại
            Vector2 newVelocity = Vector2.SmoothDamp(rb.linearVelocity, targetVelocity, ref currentVelocity, 0.5f);
            rb.linearVelocity = newVelocity;
        }
        else
        {
            // Đi trên mặt đất bình thường
            rb.linearVelocity = targetVelocity;
        }
    }
}