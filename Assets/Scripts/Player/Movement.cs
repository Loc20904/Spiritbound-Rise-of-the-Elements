using System.Collections;
using UnityEngine;

public class move : MonoBehaviour
{

    /// <summary>
    /// Movement parameters
    /// </summary>
    public float speed = 5f;
    public float jumpForce = 8f;
    float direction = 1f;
    Vector2 moveInput = Vector2.zero;

    /// <summary>
    /// Ground check parameters
    /// </summary>
    public Transform groundCheck;
    public float groundRadius = 0.2f;
    public LayerMask groundLayer;
    private bool grounded = true;

    /// <summary>
    /// Dash parameters
    /// </summary>
    public float dashSpeed = 10f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    bool isDashing = false;
    bool canDash = true;
    bool invincible = false;

    /// <summary>
    /// Double tap parameters
    /// </summary>
    public float doubleTapTime = 0.25f;
    float lastTapLeft = 0f;
    float lastTapRight = 0f;

    /// <summary>
    /// Input controls
    /// </summary>
    Player_Move controls;
    Rigidbody2D rb;
    Animator anim;

    /// <summary>
    /// Start 
    /// </summary>
    private void Awake()
    {
        controls = new Player_Move();

        controls.Player.Move.performed += ctx =>
        {
            Vector2 input = ctx.ReadValue<Vector2>();

            // --- DOUBLE TAP RIGHT ---
            if (input.x > 0.5f)
            {
                if (Time.time - lastTapRight < doubleTapTime)
                    Dash();   // Dash right

                lastTapRight = Time.time;
            }

            // --- DOUBLE TAP LEFT ---
            if (input.x < -0.5f)
            {
                if (Time.time - lastTapLeft < doubleTapTime)
                    Dash();   // Dash left

                lastTapLeft = Time.time;
            }

            moveInput = input;
        };

        controls.Player.Move.canceled += ctx =>
        {
            moveInput = Vector2.zero;
        };

        //Press Space to Jump
        controls.Player.Jump.performed += ctx => Jump();

        //Press Left Shift to Dash
        controls.Player.Sprint.performed += ctx => Dash();
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        Move();
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
        anim.SetBool("isGrounded", grounded && Mathf.Abs(rb.linearVelocityY) < 0.2f);
    }

    void Move()
    {
        if (isDashing)
        {
            anim.SetFloat("Speed", 0);
            return;
        }
        float x = moveInput.x;

        if (x > 0) transform.localScale = new Vector3(1, 1, 1);
        else if (x < 0) transform.localScale = new Vector3(-1, 1, 1);

        anim.SetFloat("Speed", Mathf.Abs(x));

        transform.position += new Vector3(x * speed * Time.deltaTime, 0, 0);

        direction = Mathf.Sign(transform.localScale.x);
    }

    void Jump()
    {
        if (!grounded) return;

        //if player is dashing, do not change horizontal velocity
        float vx = isDashing ? 0 : rb.linearVelocityX;

        anim.SetTrigger("jump");
        rb.linearVelocity = new Vector2(vx, jumpForce);
    }

    void Dash()
    {
        if (!canDash || isDashing) return;

        StartCoroutine(DoDash());
    }

    IEnumerator DoDash()
    {
        isDashing = true;
        canDash = false;

        // Bật chế độ né chiêu (i-frame)
        invincible = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;

        rb.linearVelocity = new Vector2(direction * dashSpeed, 0);

        anim.SetTrigger("dash");

        yield return new WaitForSeconds(dashDuration);

        // Hết Dash → trở lại trạng thái bình thường
        invincible = false;
        rb.gravityScale = originalGravity;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

}
