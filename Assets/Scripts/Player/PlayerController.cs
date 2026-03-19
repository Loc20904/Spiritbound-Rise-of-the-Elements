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
    [SerializeField] private string dashActionName = "Dash";
    [SerializeField] private string attackActionName = "Attack1"; // CHỈ 1 phím J
    [SerializeField] private string skillKActionName = "Attack2"; // K

    // InputActions
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction attackAction;
    private InputAction skillKAction;

    [Header("Combo (1 phím J)")]
    [SerializeField] private float comboWindow = 0.35f; // thời gian cho phép bấm lần 2
    private int comboStep = 0;        // 0: chưa đánh, 1: đã đánh hit 1
    private float comboTimer = 0f;    // đếm ngược cửa sổ combo
    private bool attackLocked = false; // khóa spam (mở bằng Animation Event)


    [Header("Skill K - Slash Projectile")]
    [SerializeField] private GameObject slashProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private int slashDamage = 2;
    [SerializeField] private float skillKCooldown = 0.5f;

    private bool canUseSkillK = true;
    private bool pendingSlashShot = false; // dùng cho animation event


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
    [SerializeField] private float coyoteTime = 0.15f;
    [SerializeField] private float jumpBufferTime = 0.15f;
    [SerializeField] private float doubleJumpForce = 13f;

    private float coyoteTimer;
    private float jumpBufferTimer;

    [Header("Dash (lụt - không teleport)")]
    [SerializeField] private float dashSpeed = 16f;
    [SerializeField] private float dashDuration = 0.14f;
    [SerializeField] private float dashCooldown = 0.25f;
    [SerializeField] private bool stopXAfterDash = true;

    [Header("Climb / Wall")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private float wallCheckRadius = 0.15f;

    [SerializeField] private LayerMask climbLayer;   // ladder/climbzone
    [SerializeField] private Transform climbCheck;
    [SerializeField] private float climbCheckRadius = 0.15f;
    [SerializeField] private float climbSpeed = 4f;

    [Header("Combat")]
    [SerializeField] private int dashDamage = 1;

    private Rigidbody2D rb;
    private Animator animator;

    [Header("Chaos / Debuff")]
    private bool isReversed = false; // Cờ theo dõi trạng thái đảo ngược

    private Vector2 moveInput;
    private int facing = 1;

    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;

    private bool isClimbing;
    private float defaultGravity;

    private bool isDead;
    private bool dashHasHit;

    // Animator hashes
    private static readonly int A_IsGround = Animator.StringToHash("IsGrounded");
    private static readonly int A_IsRunning = Animator.StringToHash("IsRunning");
    private static readonly int A_IsDashing = Animator.StringToHash("IsDashing");
    private static readonly int A_Yvel = Animator.StringToHash("Yvel");
    private static readonly int A_Isdead = Animator.StringToHash("Isdead");
    private static readonly int A_IsClimbing = Animator.StringToHash("IsClimbing");
    private static readonly int A_isLooking = Animator.StringToHash("isLookingUp");

    private static readonly int T_Attack1 = Animator.StringToHash("tAttack1"); // J cũ
    private static readonly int T_Attack2 = Animator.StringToHash("tAttack2"); // K cũ
    private static readonly int T_Hit = Animator.StringToHash("tHit");
    private static readonly int T_Dash = Animator.StringToHash("tDash");
    private static readonly int T_DashAtta = Animator.StringToHash("tDashAttack");

    //dùng để knockback hoặc các hiệu ứng khác cần tương tác với PlayerHealth 
    PlayerHealth playerHealth;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>(); //gọi PlayerHealth để sau này có thể tương tác (vd: knockback khi hit)
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
        dashAction = map.FindAction(dashActionName, true);
        attackAction = map.FindAction(attackActionName, true);
        skillKAction = map.FindAction(skillKActionName, true);   // K
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        dashAction.Enable();
        attackAction.Enable();
        skillKAction.Enable();

        jumpAction.performed += OnJump;
        dashAction.performed += OnDash;
        attackAction.performed += OnAttack; // chỉ 1 phím
        skillKAction.performed += OnSkillK;   // K bắn kiếm khí
    }

    private void OnDisable()
    {
        jumpAction.performed -= OnJump;
        dashAction.performed -= OnDash;
        attackAction.performed -= OnAttack;
        skillKAction.performed -= OnSkillK;

        moveAction.Disable();
        jumpAction.Disable();
        dashAction.Disable();
        attackAction.Disable();
        skillKAction.Disable();
    }

    private void Update()
    {
        if (isDead) return;

        moveInput = moveAction.ReadValue<Vector2>();

        // NGAY TẠI ĐÂY: Nếu dính debuff Chaos, bẻ ngược mũi tên (nhân với -1)
        if (isReversed)
        {
            moveInput = new Vector2(-moveInput.x, moveInput.y);
        }

        // ===== Combo timer =====
        if (comboTimer > 0f)
        {
            comboTimer -= Time.deltaTime;
            if (comboTimer <= 0f) comboStep = 0;
        }

        // Facing
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
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        if (isGrounded)
        {
            coyoteTimer = coyoteTime;
            jumpCount = 0;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        TryConsumeJumpBuffer();

        animator.SetFloat(A_Yvel, rb.linearVelocity.y);

        bool isRunning = isGrounded && Mathf.Abs(moveInput.x) > 0.1f && !isDashing && !isClimbing;
        animator.SetBool(A_IsRunning, isRunning);

        bool looking = moveInput.y > 0.6f;
        animator.SetBool(A_isLooking, looking);

        HandleClimb();
    }

    private void FixedUpdate()
    {
        if (isDead) return;
        if (playerHealth != null && playerHealth.isKnockback)
            return;   // CHẶN ghi đè velocity khi đang knockback
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

    // ===== Jump =====
    private void OnJump(InputAction.CallbackContext ctx)
    {
        if (isDead) return;
        jumpBufferTimer = jumpBufferTime;
        TryConsumeJumpBuffer();
    }

    private void TryConsumeJumpBuffer()
    {
        if (jumpBufferTimer <= 0f) return;
        if (isDead || isDashing) return;

        // Nhảy thường khi đang đứng đất hoặc còn coyote time
        if (isGrounded || coyoteTimer > 0f)
        {
            DoJump(isFirstJump: true);
            jumpBufferTimer = 0f;
            return;
        }

        // Đang trên không thì cho nhảy tiếp nếu còn lượt
        if (jumpCount < maxJumpCount)
        {
            DoJump(isFirstJump: false);
            jumpBufferTimer = 0f;
        }
    }

    private void DoJump(bool isFirstJump)
    {
        if (isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = defaultGravity;
            animator.SetBool(A_IsClimbing, false);
        }

        float force = isFirstJump ? jumpForce : doubleJumpForce;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);

        jumpCount++;
        coyoteTimer = 0f;
        //Debug.Log("asds" + jumpCount);
    }

    // ===== Dash =====
    private void OnDash(InputAction.CallbackContext ctx)
    {
        if (isDead) return;
        if (!canDash || isDashing) return;
        StartCoroutine(CoDash());
    }

    private IEnumerator CoDash()
    {
        canDash = false;
        isDashing = true;
        dashHasHit = false;

        if (isClimbing)
        {
            isClimbing = false;
            rb.gravityScale = defaultGravity;
            animator.SetBool(A_IsClimbing, false);
        }

        animator.SetBool(A_IsDashing, true);
        animator.SetTrigger(T_Dash);

        float t = 0f;
        float keepY = rb.linearVelocity.y;
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

    // ===== Climb =====
    private bool CanClimbHere()
    {
        if (!climbCheck) return false;
        return Physics2D.OverlapCircle(climbCheck.position, climbCheckRadius, climbLayer);
    }

    private bool TouchWall()
    {
        Transform check = (facing == 1) ? wallCheckRight : wallCheckLeft;
        if (!check) return false;
        return Physics2D.OverlapCircle(check.position, wallCheckRadius, wallLayer);
    }

    private void HandleClimb()
    {
        bool touchClimb = CanClimbHere();
        bool touchWall = TouchWall();

        bool wantClimb = (touchClimb || touchWall)
                         && Mathf.Abs(moveInput.y) > 0.1f
                         && !isDashing;

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

    // ===== Attack (1 phím J: J -> K) =====
    private bool queueSecondHit = false; // ghi nhớ nhấn J lần 2
    private void OnAttack(InputAction.CallbackContext ctx)
    {
        if (isDead || isDashing || isClimbing) return;

        if (attackLocked)
        {
            if (comboStep == 1 && comboTimer > 0f)
                queueSecondHit = true;
            return;
        }

        // Hit 1 (J)
        if (comboStep == 0)
        {
            comboStep = 1;
            comboTimer = comboWindow;

            attackLocked = true;
            queueSecondHit = false;
            pendingSlashShot = false;

            animator.SetTrigger(T_Attack1);
            GetComponent<PlayerAttack>()?.PlayAttack(AttackType.J);
            return;
        }

        // Hit 2 (J lần 2 -> K cận chiến)
        if (comboStep == 1 && comboTimer > 0f)
        {
            comboStep = 0;
            comboTimer = 0f;

            attackLocked = true;
            queueSecondHit = false;
            pendingSlashShot = false;

            animator.SetTrigger(T_Attack2);
            GetComponent<PlayerAttack>()?.PlayAttack(AttackType.K);
        }
    }
    // Animation Event: gọi ở frame cuối Attack1/Attack2 để mở khóa đánh tiếp
    public void Anim_AttackUnlock()
    {
        attackLocked = false;

        if (queueSecondHit && comboStep == 1 && comboTimer > 0f)
        {
            queueSecondHit = false;
            comboStep = 0;
            comboTimer = 0f;

            attackLocked = true;
            pendingSlashShot = false;

            animator.SetTrigger(T_Attack2);
            GetComponent<PlayerAttack>()?.PlayAttack(AttackType.K);
        }
    }
    private void OnSkillK(InputAction.CallbackContext ctx)
    {
        if (isDead || isDashing || isClimbing) return;
        //if (attackLocked) return;
        if (!canUseSkillK) return;

        comboStep = 0;
        comboTimer = 0f;
        queueSecondHit = false;

        attackLocked = true;
        pendingSlashShot = true;

        animator.SetTrigger(T_Attack2); // dùng anim chém K
        StartCoroutine(SkillKCooldownRoutine());
    }

    private IEnumerator SkillKCooldownRoutine()
    {
        canUseSkillK = false;
        yield return new WaitForSeconds(skillKCooldown);
        canUseSkillK = true;
    }

    // Animation Event: đặt đúng frame chém ra kiếm khí
    public void Anim_FireSlashProjectile()
    {
        if (!pendingSlashShot) return;

        pendingSlashShot = false;

        if (slashProjectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogWarning("Thiếu slashProjectilePrefab hoặc projectileSpawnPoint");
            return;
        }

        GameObject go = Instantiate(
            slashProjectilePrefab,
            projectileSpawnPoint.position,
            Quaternion.identity
        );

        SlashProjectile projectile = go.GetComponent<SlashProjectile>();
        if (projectile != null)
        {
            projectile.Init(facing, slashDamage);
        }
    }


    // Dash hit enemy -> DashAttack
    public void OnDashHit(GameObject enemy)
    {
        if (!isDashing) return;
        if (dashHasHit) return;

        dashHasHit = true;
        animator.SetTrigger(T_DashAtta);

        var dmg = enemy.GetComponent<IDamageable>();
        if (dmg != null) dmg.TakeDamage(dashDamage);
    }

    public void TakeHit()
    {
        if (isDead) return;
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

    public void StopMove()
    {
        // 1. Dừng ngay lập tức các Coroutine đang chạy (đặc biệt là CoDash)
        StopAllCoroutines();

        // 2. Reset vận tốc và input
        moveInput = Vector2.zero;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = defaultGravity; // Trả lại trọng lực lỡ đang leo tường/thang
        }

        // 3. Reset các biến trạng thái di chuyển
        isDashing = false;
        isClimbing = false;
        canDash = true;
        jumpBufferTimer = 0f;
        coyoteTimer = 0f;

        // 4. Reset trạng thái tấn công (tránh kẹt combo)
        comboStep = 0;
        comboTimer = 0f;
        attackLocked = false;
        queueSecondHit = false;

        // 5. Ép Animator cập nhật về trạng thái Idle đứng im
        if (animator != null)
        {
            animator.SetBool(A_IsRunning, false);
            animator.SetBool(A_IsDashing, false);
            animator.SetBool(A_IsClimbing, false);
            animator.SetFloat(A_Yvel, 0f);
        }
    }

    // Cấp quyền cho ChaosMechanic bật/tắt trạng thái đảo ngược
    public void SetReverseControl(bool state)
    {
        isReversed = state;
    }
}


public interface IDamageable
{
    void TakeDamage(int amount);
}