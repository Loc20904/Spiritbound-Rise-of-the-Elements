using UnityEngine;
using System.Collections;

public class PlayerSimpleTest : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 6f;

    [Header("Attack")]
    public float attackRange = 1f;
    public Vector2 attackBoxSize = new Vector2(1f, 1f);
    public int damage = 10;
    public float attackDuration = 0.15f;
    public LayerMask enemyLayer;

    Rigidbody2D rb;
    PlayerHealth playerHealth;

    Vector2 moveInput;
    bool facingRight = true;
    bool isAttacking;

    // ================= START =================
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        playerHealth = GetComponent<PlayerHealth>();
    }

    // ================= UPDATE =================
    void Update()
    {
        MoveInput();

        if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
        {
            StartCoroutine(Attack());
        }
    }

    
    

    // ================= MOVE =================
    void MoveInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(h, v).normalized;

        // flip player
        if (h > 0 && !facingRight)
            Flip();
        else if (h < 0 && facingRight)
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;

        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    // ================= ATTACK =================
    IEnumerator Attack()
    {
        isAttacking = true;

        Vector2 center = (Vector2)transform.position +
                         new Vector2(facingRight ? attackRange : -attackRange, 0);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, attackBoxSize, 0, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            hit.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }
    void FixedUpdate()
    {
        if (playerHealth != null && playerHealth.isKnockback)
            return;

        rb.linearVelocity = moveInput * moveSpeed;
    }
    // ================= GIZMOS =================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector2 center = (Vector2)transform.position +
                         new Vector2(facingRight ? attackRange : -attackRange, 0);

        Gizmos.DrawWireCube(center, attackBoxSize);
    }
}
