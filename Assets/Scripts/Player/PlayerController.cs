using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class PlayerController : MonoBehaviour
{
    [Header("Input (InputActionAsset)")]
    [SerializeField] private InputActionAsset actions;
    [SerializeField] private string actionMapName = "Player";
    [SerializeField] private string moveActionName = "Move";
    [SerializeField] private string jumpActionName = "Jump";
    [SerializeField] private string attack1ActionName = "Attack1";         // J
    [SerializeField] private string attack2ActionName = "Attack2";         // K
    [SerializeField] private string dashActionName = "Dash";               // Shift
    [SerializeField] private string upIdleAttackActionName = "Up_idle_Attack"; // L (theo ảnh)

    [Header("Double Jump")]
    [SerializeField] private int maxJumpCount = 2;

    private int jumpCount;


    [Header("Move")]
    [SerializeField] private float moveSpeed = 6f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.18f;
    [SerializeField] private float doubleJumpForce = 14f;   // ✅ jump 2 cao hơn
    [SerializeField] private float coyoteTime = 0.12f;       // ✅ rời đất vẫn nhảy được
    [SerializeField] private float jumpBufferTime = 0.12f;   // ✅ bấm sớm vẫn nhảy

    private float coyoteTimer;
    private float jumpBufferTimer;


    [Header("Dash (lụt - không teleport)")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.14f;
    [SerializeField] private float dashCooldown = 0.25f;
    [SerializeField] private bool stopXAfterDash = true;

    [Header("Climb (ladder/vùng climb)")]
    [SerializeField] private LayerMask climbLayer;   // layer Ladder/ClimbZone
    [SerializeField] private Transform climbCheck;   // điểm check chạm ladder
    [SerializeField] private float climbCheckRadius = 0.15f;
    [SerializeField] private float climbSpeed = 4f;

    [Header("Combat")]
    [SerializeField] private int dashDamage = 1;

    private Rigidbody2D rb;
    private Animator animator;

    private InputAction moveAction, jumpAction, attack1Action, attack2Action, dashAction, upIdleAttackAction;

    private Vector2 moveInput;
    private int facing = 1;

    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;

    private bool isClimbing;
    private float defaultGravity;

    private bool isDead;

    // DashAttack: tránh hit nhiều lần trong 1 dash
    private bool dashHasHit;

    private static readonly int A_IsGround = Animator.StringToHash("IsGrounded");
    private static readonly int A_IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int A_IsDashing = Animator.StringToHash("IsDashing");
    private static readonly int A_Yvel = Animator.StringToHash("Yvel");
    private static readonly int A_Isdead = Animator.StringToHash("Isdead");
    private static readonly int A_IsClimbing = Animator.StringToHash("IsClimbing");
    private static readonly int A_isLooking = Animator.StringToHash("isLookingUp");

    private static readonly int T_Attack1 = Animator.StringToHash("tAttack1");
    private static readonly int T_Attack2 = Animator.StringToHash("tAttack2");
    private static readonly int T_Hit = Animator.StringToHash("tHit");
    private static readonly int T_Dash = Animator.StringToHash("tDash");
    private static readonly int T_DashAtta = Animator.StringToHash("tDashAttack");
    private static readonly int T_UpIdleAtt = Animator.StringToHash("T_Idle_Att");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        defaultGravity = rb.gravityScale;

        if (actions == null)
        {
            Debug.LogError("PlayerController: Chưa gán InputActionAsset.");
            enabled = false;
            return;
        }

        var map = actions.FindActionMap(actionMapName, true);

        moveAction = map.FindAction(moveActionName, true);
        jumpAction = map.FindAction(jumpActionName, true);
        attack1Action = map.FindAction(attack1ActionName, true);
        attack2Action = map.FindAction(attack2ActionName, true);
        dashAction = map.FindAction(dashActionName, true);
        upIdleAttackAction = map.FindAction(upIdleAttackActionName, true);
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        attack1Action.Enable();
        attack2Action.Enable();
        dashAction.Enable();
        upIdleAttackAction.Enable();

        jumpAction.performed += OnJump;
        attack1Action.performed += OnAttack1;
        attack2Action.performed += OnAttack2;
        dashAction.performed += OnDash;
        upIdleAttackAction.performed += OnUpIdleAttack;
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJump;
        attack1Action.performed -= OnAttack1;
        attack2Action.performed -= OnAttack2;
        dashAction.performed -= OnDash;
        upIdleAttackAction.performed -= OnUpIdleAttack;

        moveAction.Disable();
        jumpAction.Disable();
        attack1Action.Disable();
        attack2Action.Disable();
        dashAction.Disable();
        upIdleAttackAction.Disable();
    }

    private void Update()
    {
        if (isDead) return;

        moveInput = moveAction.ReadValue<Vector2>();

        // Facing (không flip khi đang dash để dash ổn định)
        if (!isDashing && Mathf.Abs(moveInput.x) > 0.05f)
        {
            facing = moveInput.x > 0 ? 1 : -1;
            var s = transform.localScale;
            s.x = Mathf.Abs(s.x) * facing;
            transform.localScale = s;
        }

        // grounded
        isGrounded = IsGrounded();
        animator.SetBool(A_IsGround, isGrounded);

        // coyote time
        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            jumpCount = 0;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        // jump buffer countdown
        jumpBufferTimer -= Time.deltaTime;

        // nếu có buffer thì thử nhảy ngay khi đủ điều kiện
        TryConsumeJumpBuffer();


        animator.SetFloat(A_Yvel, rb.linearVelocity.y);

        // Running (chỉ khi grounded và có input X)
        bool isRunning = isGrounded && Mathf.Abs(moveInput.x) > 0.1f && !isDashing && !isClimbing;
        animator.SetBool(A_IsRunning, isRunning);

        // Looking up (tuỳ bạn: dùng input Y > 0.6 coi là nhìn lên)
        bool looking = moveInput.y > 0.6f;
        animator.SetBool(A_isLooking, looking);

        // Climb state
        HandleClimb();
    }
    private void TryConsumeJumpBuffer()
    {
        if (jumpBufferTimer <= 0f) return;
        if (isDead || isDashing) return;

        // lần 1: dùng grounded hoặc coyote
        if (jumpCount == 0)
        {
            if (isGrounded || coyoteTimer > 0f)
            {
                DoJump(isFirstJump: true);
                jumpBufferTimer = 0f;
            }
            return;
        }

        // lần 2: luôn cho phép khi đang ở trên không
        if (jumpCount < maxJumpCount)
        {
            DoJump(isFirstJump: false);
            jumpBufferTimer = 0f;
        }
    }
    private void DoJump(bool isFirstJump)
    {
        // nếu đang climb thì thoát climb
        if (isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = defaultGravity;
            animator.SetBool(A_IsClimbing, false);
        }

        float force = isFirstJump ? jumpForce : doubleJumpForce;

        // reset Y trước khi bật nhảy để không bị "ì" khi đang rơi nhanh
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        jumpCount++;
        coyoteTimer = 0f; // đã nhảy thì hết coyote
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (isDashing) return; // dash sẽ set velocity riêng trong coroutine

        if (isClimbing)
        {
            rb.linearVelocity = new Vector2(0f, moveInput.y * climbSpeed);
            return;
        }

        rb.linearVelocity = new Vector2(moveInput.x * moveSpeed, rb.linearVelocity.y);
    }

    // ===== Ground =====
    private bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    // ===== Climb =====
    private bool CanClimbHere()
    {
        if (!climbCheck) return false;
        return Physics2D.OverlapCircle(climbCheck.position, climbCheckRadius, climbLayer);
    }

    private void HandleClimb()
    {
        // chỉ leo khi đang chạm ladder/climbzone và có input Y
        bool touchClimb = CanClimbHere();
        bool wantClimb = touchClimb && Mathf.Abs(moveInput.y) > 0.1f && !isDashing;

        if (wantClimb && !isClimbing)
        {
            isClimbing = true;
            rb.gravityScale = 0f;
            rb.linearVelocity = Vector2.zero;
        }
        else if (!wantClimb && isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = defaultGravity;
        }

        animator.SetBool(A_IsClimbing, isClimbing);
    }

    // ===== Jump =====
    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isDead) return;

        // lưu buffer để Update xử lý (đỡ delay và mượt hơn)
        jumpBufferTimer = jumpBufferTime;

        // thử ăn ngay (trường hợp đang đủ điều kiện)
        TryConsumeJumpBuffer();
    }



    // ===== Dash (Shift) =====
    private void OnDash(InputAction.CallbackContext ctx)
    {
        if (isDead) return;
        if (!canDash) return;
        if (isDashing) return;

        StartCoroutine(CoDash());
    }

    private IEnumerator CoDash()
    {
        canDash = false;
        isDashing = true;
        dashHasHit = false;

        // Nếu đang climb thì thoát climb
        if (isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = defaultGravity;
            animator.SetBool(A_IsClimbing, false);
        }

        animator.SetBool(A_IsDashing, true);
        animator.SetTrigger(T_Dash);

        // Dash kiểu lướt: set velocity trong 1 khoảng thời gian ngắn
        float t = 0f;
        float keepY = rb.linearVelocity.y; // giữ Y để không reminding
        while (t < dashDuration)
        {
            rb.linearVelocity = new Vector2(facing * dashSpeed, keepY);
            t += Time.deltaTime;
            yield return null;
        }

        if (stopXAfterDash)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

        animator.SetBool(A_IsDashing, false);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    // ===== Attacks =====
    private void OnAttack1(InputAction.CallbackContext ctx)
    {
        if (isDead) return;
        if (isDashing) return;      // đang dash thì không cho đánh thường
        if (isClimbing) return;

        //animator.Play("Player_Attack_1", 0, 0f);

        animator.SetTrigger(T_Attack1);

    }

    private void OnAttack2(InputAction.CallbackContext ctx)
    {
        if (isDead) return;
        if (isDashing) return;
        if (isClimbing) return;

        animator.SetTrigger(T_Attack2);
        

    }

    private void OnUpIdleAttack(InputAction.CallbackContext ctx)
    {
        if (isDead) return;
        if (isDashing) return;
        if (isClimbing) return;

        // bạn muốn “đứng yên đánh lên” => nếu đang chạy thì có thể chặn
        if (!isGrounded) return;

        animator.SetTrigger(T_UpIdleAtt);
    }

    // ===== Hit / Dead =====
    public void TakeHit()
    {
      
        if (isDead) return;
        Debug.Log("TakeHit() called, animator = " + (animator ? animator.name : "NULL"));
        animator.SetTrigger(T_Hit);
    }

    public void Die()
    {
        if (isDead) return;
        isDead = true;
        animator.SetBool(A_Isdead, true);
        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
    }

    // ===== Dash hit enemy -> DashAttack =====
    // Gọi hàm này từ hitbox trigger (script bên dưới)
    public void OnDashHit(GameObject enemy)
    {
        if (!isDashing) return;
        if (dashHasHit) return;

        dashHasHit = true;

        // Trigger DashAttack anim
        animator.SetTrigger(T_DashAtta);

        // Gây damage nếu enemy có IDamageable
        var dmg = enemy.GetComponent<IDamageable>();
        if (dmg != null) dmg.TakeDamage(dashDamage);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck)
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);

        if (climbCheck)
            Gizmos.DrawWireSphere(climbCheck.position, climbCheckRadius);
    }
    // Biến kiểm tra trạng thái để khóa di chuyển
    public bool isStunned = false;

    public void ApplyStun(float duration, GameObject stunVFXPrefab)
    {
        if (isStunned) return; // Nếu đang choáng thì không chồng hiệu ứng

        StartCoroutine(StunRoutine(duration, stunVFXPrefab));
    }

    private IEnumerator StunRoutine(float duration, GameObject vfxPrefab)
    {
        isStunned = true;
        Debug.Log("Player bị choáng!");

        // 1. Tạo hiệu ứng trên đầu
        GameObject currentVFX = null;
        if (vfxPrefab != null)
        {
            Vector3 headPos = transform.position + new Vector3(0, 1f, 0);
            currentVFX = Instantiate(vfxPrefab, headPos, Quaternion.identity);
            currentVFX.transform.SetParent(this.transform); // Gán làm con để di chuyển theo
        }

        // 2. Tại đây bạn nên KHÓA DI CHUYỂN (disable movement script hoặc set velocity = 0)
        // Ví dụ: GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(duration);

        // 3. Hết thời gian choáng
        isStunned = false;
        Debug.Log("Hết choáng!");

        // Xóa hiệu ứng
        if (currentVFX != null) Destroy(currentVFX);
    }
}

// interface đơn giản để enemy nhận damage
public interface IDamageable
{
    void TakeDamage(int amount);
}
