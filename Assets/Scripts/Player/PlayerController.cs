using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionAsset actions;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string attack1ActionName = "Attack1";
    [SerializeField] private string attack2ActionName = "Attack2";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.18f;

    [Header("Dash (fixed distance)")]
    [SerializeField] private float dashDistance = 2.5f;     // ✅ chỉnh khoảng lướt
    [SerializeField] private float dashLockTime = 0.12f;     // ✅ thời gian khóa movement khi dash
    [SerializeField] private float doubleTapTime = 0.25f;    // thời gian tối đa giữa 2 lần tap
    [SerializeField] private LayerMask dashBlockLayer;       // tick Ground/Wall để không xuyên
    [SerializeField] private float dashSkin = 0.05f;         // chừa 1 chút tránh kẹt
    [SerializeField] private bool dashSpriteOpposite = true; // ✅ bật nếu sprite dash bị ngược hướng

    [Header("Combo (J+K)")]
    [SerializeField] private float jkWindow = 0.08f;

    private Rigidbody2D rb;
    private Animator animator;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction attack1Action;
    private InputAction attack2Action;

    private Vector2 moveInput;
    private bool isDead;

    // Dash state
    private bool isDashing;
    private float dashTimer;
    private int facing = 1; // 1: phải, -1: trái

    // Double tap edge detect
    private bool prevLeftHeld, prevRightHeld;
    private float lastLeftTap = -999f;
    private float lastRightTap = -999f;

    // J+K detect
    private float lastJTime = -999f;
    private float lastKTime = -999f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (actions == null)
        {
            Debug.LogError("PlayerController: Chưa gán InputActionAsset vào script.");
            enabled = false;
            return;
        }

        var map = actions.FindActionMap(actionMapName, true);

        moveAction = map.FindAction(moveActionName, true);
        jumpAction = map.FindAction(jumpActionName, true);
        attack1Action = map.FindAction(attack1ActionName, true);
        attack2Action = map.FindAction(attack2ActionName, true);
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        attack1Action.Enable();
        attack2Action.Enable();

        jumpAction.performed += OnJump;
        attack1Action.performed += OnAttack1;
        attack2Action.performed += OnAttack2;
    }

    private void OnDisable()
    {
        if (jumpAction != null) jumpAction.performed -= OnJump;
        if (attack1Action != null) attack1Action.performed -= OnAttack1;
        if (attack2Action != null) attack2Action.performed -= OnAttack2;

        moveAction?.Disable();
        jumpAction?.Disable();
        attack1Action?.Disable();
        attack2Action?.Disable();
    }

    private void Update()
    {
        if (isDead) return;

        moveInput = moveAction.ReadValue<Vector2>();

        // Blend Idle/Run
        animator.SetFloat("Speed", Mathf.Abs(moveInput.x));

        // Dash timer (khóa movement)
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                isDashing = false;

                // trả scale về đúng hướng thật sau dash
                var s = transform.localScale;
                s.x = Mathf.Abs(s.x) * facing;
                transform.localScale = s;
            }
        }

        // Flip theo hướng chạy (chỉ khi không dash)
        if (!isDashing && Mathf.Abs(moveInput.x) > 0.01f)
        {
            facing = moveInput.x > 0 ? 1 : -1;
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * facing;
            transform.localScale = s;
        }

        // Double tap dash (A/D hoặc Arrow) — chỉ tính nhấn cạnh (không dash khi giữ)
        HandleDoubleTapDash(moveInput.x);
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        // Khi dash, khóa movement (không set velocity ngang)
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
            return;
        }

        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    // ===== Jump =====
    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isDead) return;
        if (!IsGrounded()) return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private bool IsGrounded()
    {
        if (groundCheck == null) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // ===== Dash double-tap =====
    private void HandleDoubleTapDash(float x)
    {
        if (isDead) return;
        if (isDashing) return;

        bool leftHeld = x < -0.6f;
        bool rightHeld = x > 0.6f;

        if (leftHeld && !prevLeftHeld)
        {
            float now = Time.time;
            if (now - lastLeftTap <= doubleTapTime) StartDash(-1);
            lastLeftTap = now;
        }

        if (rightHeld && !prevRightHeld)
        {
            float now = Time.time;
            if (now - lastRightTap <= doubleTapTime) StartDash(1);
            lastRightTap = now;
        }

        prevLeftHeld = leftHeld;
        prevRightHeld = rightHeld;
    }

    // Dash lướt 1 đoạn có chặn tường
    private void StartDash(int dir)
    {
        if (isDead) return;

        facing = dir;

        // ✅ fix ngược sprite dash (nếu dash sprite quay ngược so với idle/run)
        int visualDir = dashSpriteOpposite ? -facing : facing;
        var s = transform.localScale;
        s.x = Mathf.Abs(s.x) * visualDir;
        transform.localScale = s;

        Vector2 origin = rb.position;
        Vector2 dashDir = Vector2.right * facing;

        float dist = dashDistance;
        RaycastHit2D hit = Physics2D.Raycast(origin, dashDir, dist + dashSkin, dashBlockLayer);

        Vector2 targetPos = origin + dashDir * dist;
        if (hit.collider != null)
        {
            targetPos = hit.point - dashDir * dashSkin;
        }

        // ✅ dịch chuyển 1 phát (rõ ràng)
        rb.position = targetPos;

        // khóa movement một chút để tạo cảm giác dash
        isDashing = true;
        dashTimer = dashLockTime;

        animator.SetTrigger("Dash");
    }

    // ===== Attacks =====
    private void OnAttack1(InputAction.CallbackContext ctx)
    {
        if (isDead) return;

        lastJTime = Time.time;
        if (Time.time - lastKTime <= jkWindow)
        {
            animator.SetTrigger("DoubleAttack");
            return;
        }
        animator.SetTrigger("Attack1");
    }

    private void OnAttack2(InputAction.CallbackContext ctx)
    {
        if (isDead) return;

        lastKTime = Time.time;
        if (Time.time - lastJTime <= jkWindow)
        {
            animator.SetTrigger("DoubleAttack");
            return;
        }
        animator.SetTrigger("Attack2");
    }

    // ===== Death =====
    public void Die()
    {
        if (isDead) return;
        isDead = true;

        animator.SetBool("IsDead", true);
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
