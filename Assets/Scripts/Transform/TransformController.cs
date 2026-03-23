using UnityEngine;
using UnityEngine.InputSystem;

public class TransformController : MonoBehaviour
{
    [Header("Input Actions")]
    public InputAction moveAction;
    public InputAction jumpAction;
    public InputAction dashAction;

    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;

    [Header("Dash")]
    [SerializeField] private float dashForce = 15f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private float normalGravity = 3f;

    private bool isGrounded;
    private bool isDashing;
    private float dashTimer;
    private int airDashesLeft;
    private float facingDirection = 1f;

    private Animator animator;
    private Rigidbody2D rb;

    [Header("Transform Anim")]
    public float transformAnimTime = 0.4f; // Sửa 40f thành 0.4f (40s là quá lâu, gây kẹt input 40 giây)
    private bool isTransforming = false;

    private void Awake()
    {
        // Ghi đè lại nếu trong Unity Editor người dùng đã lưu sẵn thời gian 40s cực lâu
        transformAnimTime = Mathf.Min(transformAnimTime, 1f); 

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        attackController = GetComponent<AttackController>();
        if (rb != null) normalGravity = rb.gravityScale;

        if (moveAction == null || moveAction.bindings.Count == 0 || moveAction.expectedControlType != "Axis")
        {
            // Bắt buộc set expectedControlType = "Axis" cho 1D Axis
            moveAction = new InputAction("Move", type: InputActionType.Value, expectedControlType: "Axis");
            moveAction.AddCompositeBinding("1DAxis")
                .With("negative", "<Keyboard>/a")
                .With("negative", "<Keyboard>/leftArrow")
                .With("positive", "<Keyboard>/d")
                .With("positive", "<Keyboard>/rightArrow");
        }

        if (jumpAction == null || jumpAction.bindings.Count == 0)
        {
            jumpAction = new InputAction("Jump", binding: "<Keyboard>/space");
        }

        if (dashAction == null || dashAction.bindings.Count == 0)
        {
            dashAction = new InputAction("Dash", binding: "<Keyboard>/leftShift");
        }
    }

    private void OnEnable()
    {
        // Khi form được kích hoạt lại (transform thành form này), 
        // cần phải Enable lại các action để nhận Input
        if (moveAction != null) moveAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
        if (dashAction != null) dashAction.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        jumpAction?.Disable();
        dashAction?.Disable();

        // When the GameObject is disabled, coroutines are killed. Reset flags to avoid getting stuck upon re-enabling.
        isTransforming = false;
        isDashing = false;
        if (rb != null) rb.gravityScale = normalGravity;
    }

    public void PlayTransformAnimation()
    {
        StartCoroutine(CoTransformAnimation());
    }

    private System.Collections.IEnumerator CoTransformAnimation()
    {
        isTransforming = true;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (animator != null) animator.SetTrigger("tTransform");
        yield return new WaitForSeconds(Mathf.Min(transformAnimTime, 1f));
        isTransforming = false;
    }
    private void HandleDash()
    {
        // Nhấn Shift khi không lướt thì lướt
        if (dashAction != null && dashAction.triggered && !isDashing)
        {
            StartCoroutine(PerformDash());
        }
    }

    private System.Collections.IEnumerator PerformDash()
    {
        isDashing = true;

        if (rb != null)
        {
            // Vô hiệu hóa trọng lực
            float originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;

            // Lấy hướng nhân vật (+1 hoặc -1)
            float direction = transform.localScale.x > 0 ? 1f : -1f;

            // Vòng lặp Dash bằng Coroutine
            float timePassed = 0f;
            while (timePassed < dashTime)
            {
                // Liên tục ghi đè vận tốc để bay thẳng
                rb.linearVelocity = new Vector2(direction * dashForce, 0f);
                timePassed += Time.deltaTime;

                // Đợi đến frame kế tiếp
                yield return null;
            }

            // Dừng xe lại một tí sau khi lướt (tùy chọn)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            // Bật lại trọng lực gốc
            rb.gravityScale = normalGravity;
        }
        else 
        {
            yield return new WaitForSeconds(dashTime);
        }

        isDashing = false;
    }
    private AttackController attackController;

    // Update is called once per frame
    void Update()
    {
        if (isTransforming) return; // Khoá điều khiển khi đang chạy animation

        // Khóa điều khiển nếu đang trong thời gian xài RunAttack
        if (attackController != null && attackController.IsAttackLocked())
        {
            UpdateAnimation();
            return;
        }

        HandleMovement();
        HandleJump();
        HandleDash();

        UpdateAnimation();
    }

    private void HandleMovement()
    {
        float moveInput = moveAction != null ? moveAction.ReadValue<float>() : 0f;
        if (!isDashing && rb != null) // Prevent movement input during dash
        {
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);

            if (Mathf.Abs(moveInput) > 0.01f)
            {
                Vector3 s = transform.localScale;
                // Chỉ đổi dấu trên trục X dựa theo hướng di chuyển
                s.x = Mathf.Abs(s.x) * Mathf.Sign(moveInput);
                transform.localScale = s;
            }
        }
    }

    private void HandleJump()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        }
        else
        {
            // Fallback nếu quên kéo groundCheck vào inspector
            isGrounded = true; // Tạm cho phép nhảy để test nếu k có ground check
        }

        if (jumpAction != null && jumpAction.triggered && isGrounded && rb != null)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }


    private void UpdateAnimation()
    {
        if (animator == null) return; // Tránh lỗi NullReference nếu chưa có Animator
        bool isRunning = rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f;
        bool isJumping = !isGrounded;

        animator.SetBool("isRunning", isRunning);
        animator.SetBool("isJumping", isJumping);
        animator.SetBool("isDashing", isDashing);

        // Chuẩn hoá yVelocity để truyền đúng mức -1 (Fall) và 1 (Jump) hoặc 0 vào BlendTree
        // Việc này ngăn lỗi trị số vật lý quá lớn (vd: rơi với -15) làm hỏng tính năng nội suy của BlendTree.
        float normalizedY = 0f;
        if (rb != null)
        {
            if (rb.linearVelocity.y > 0.1f) normalizedY = 1f;
            else if (rb.linearVelocity.y < -0.1f) normalizedY = -1f;
        }

        animator.SetFloat("yVelocity", normalizedY);
    }
}
