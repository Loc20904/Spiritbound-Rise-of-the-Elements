using UnityEngine;

public class QuaiAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float jumpForce = 8f;

    [Header("Check Points")]
    public Transform groundCheck;
    public Transform obstacleCheck;
    public Transform edgeGroundCheck;

    public float checkRadius = 0.2f;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.2f);
    public Vector2 groundAheadcheck = new Vector2(0.8f, 0.2f);
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    [Header("Patrol / Idle")]
    public float patrolTime = 3f;
    public float idleTime = 2f;

    [Header("Jump Control")]
    public float jumpCooldown = 0.5f;

    [Header("Animation")]
    public Sprite[] idleFrames;
    public Sprite[] runFrames;
    public Sprite jumpFrame;
    public float frameRate = 0.1f;

    EnemyChaseAI chaseAI;
    enum AIState { Patrol, Idle }
    AIState currentState = AIState.Patrol;

    Rigidbody2D rb;
    SpriteRenderer sr;
    EnemyRangedAttack ranged;

    float stateTimer;
    float jumpTimer;

    float animTimer;
    int frameIndex;
    bool facingRight = true;
    bool wasMoving;

    // ===== PUBLIC FOR OTHER SCRIPTS =====
    public bool FacingRight => facingRight;
    public void FlipPublic() => Flip();

    // ================= START =================
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        ranged = GetComponent<EnemyRangedAttack>();
        chaseAI = GetComponent<EnemyChaseAI>();
        stateTimer = patrolTime;
        ResetAnimation();
    }

    // ================= UPDATE =================
    void Update()
    {
        // ===== CHASE SYSTEM =====
        if (chaseAI != null)
        {
            chaseAI.Tick(); // ⭐ BẮT BUỘC PHẢI GỌI TRƯỚC

            if (chaseAI.IsChasing)
            {
                int dir = chaseAI.ChaseDirection;

                if (dir != 0)
                {
                    if ((dir > 0) != facingRight)
                        Flip();

                    float speed = moveSpeed * chaseAI.SpeedMultiplier;

                    rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);
                }

                CheckEnvironment();
                UpdateAnimation();
                return;
            }
        }
        jumpTimer -= Time.deltaTime;

        bool playerInRange = ranged != null && ranged.PlayerInRange;

        // ===== đang bắn =====
        if (ranged != null && ranged.IsAttacking)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // ===== player trước mặt =====
        if (playerInRange)
        {
            rb.linearVelocity = Vector2.zero;

            // quay mặt player
            Vector2 playerPos = ranged.PlayerPosition;
            bool playerRight = playerPos.x > transform.position.x;

            if (playerRight != facingRight)
                Flip();

            currentState = AIState.Idle;
            ResetAnimation();
            return;
        }

        HandleState();

        if (currentState == AIState.Patrol)
            CheckEnvironment();

        UpdateAnimation();
    }

    // ================= STATE =================
    void HandleState()
    {
        stateTimer -= Time.deltaTime;

        if (currentState == AIState.Patrol)
        {
            Move();

            if (stateTimer <= 0)
            {
                currentState = AIState.Idle;
                stateTimer = idleTime;
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                ResetAnimation();
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            if (stateTimer <= 0)
            {
                currentState = AIState.Patrol;
                stateTimer = patrolTime;
                ResetAnimation();
            }
        }
    }

    // ================= MOVE =================
    void Move()
    {
        float dir = facingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    // ================= ENV CHECK =================
    void CheckEnvironment()
    {
        LayerMask mask = groundLayer | obstacleLayer;

        bool isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, mask);
        bool wallAhead = Physics2D.OverlapCircle(obstacleCheck.position, checkRadius, mask);
        bool groundAhead = Physics2D.OverlapBox(edgeGroundCheck.position, groundAheadcheck, mask);

        if (wallAhead && isGrounded && groundAhead && jumpTimer <= 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpTimer = jumpCooldown;
        }

        if (!groundAhead && isGrounded)
        {
            if (chaseAI != null && chaseAI.IsChasing)
            {
                // ⭐ đang chase → hủy chase (boss không ngu nhảy vực)
                chaseAI.StopChase();
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else
            {
                // ⭐ patrol → quay đầu như cũ
                Flip();
            }
        }
    }

    // ================= FLIP =================
    public void Flip()
    {
        facingRight = !facingRight;

        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (facingRight ? 1 : -1);
        transform.localScale = s;
    }

    // ⭐ chạm player → quay đầu
    void OnCollisionEnter2D(Collision2D col)
    {
        if (!col.collider.CompareTag("Player"))
            return;

        bool playerRight = col.transform.position.x > transform.position.x;

        if (playerRight != facingRight)
            Flip();
    }

    // ================= ANIMATION =================
    void UpdateAnimation()
    {
        bool isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            checkRadius,
            groundLayer | obstacleLayer
        );

        if (!isGrounded)
        {
            if (jumpFrame != null)
                sr.sprite = jumpFrame;
            return;
        }

        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        if (isMoving != wasMoving)
        {
            ResetAnimation();
            wasMoving = isMoving;
        }

        if (isMoving)
            UpdateRunAnimation();
        else
            UpdateIdleAnimation();
    }

    void UpdateRunAnimation()
    {
        if (runFrames.Length == 0) return;

        animTimer += Time.deltaTime;
        if (animTimer >= frameRate)
        {
            animTimer = 0;
            frameIndex = (frameIndex + 1) % runFrames.Length;
        }

        sr.sprite = runFrames[frameIndex];
    }

    void UpdateIdleAnimation()
    {
        if (idleFrames.Length == 0) return;

        animTimer += Time.deltaTime;
        if (animTimer >= frameRate)
        {
            animTimer = 0;
            frameIndex = (frameIndex + 1) % idleFrames.Length;
        }

        sr.sprite = idleFrames[frameIndex];
    }

    void ResetAnimation()
    {
        animTimer = 0;
        frameIndex = 0;
        wasMoving = false;
    }
    // ================= GIZMOS =================
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (groundCheck)
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        Gizmos.color = Color.red;
        if (obstacleCheck)
            Gizmos.DrawWireSphere(obstacleCheck.position, checkRadius + 0.5f);

        Gizmos.color = Color.cyan;
        if (edgeGroundCheck)
            Gizmos.DrawWireCube(edgeGroundCheck.position, groundAheadcheck);
    }
}
