using UnityEngine;
using System.Collections;

public class EnemyExploder : MonoBehaviour
{
    [Header("Detect")]
    public float detectRange = 5f;
    public float explodeRange = 1.2f;
    public LayerMask playerLayer;

    [Header("Move")]
    public float moveSpeed = 3f;

    [Header("Explosion")]
    public int damage = 25;
    public float explosionRadius = 2f;
    public float knockbackForce = 8f;

    [Header("Animation Frames")]
    public Sprite[] idleFrames;
    public Sprite[] chaseFrames;
    public Sprite[] deathFrames;
    public float frameRate = 0.08f;

    Rigidbody2D rb;
    SpriteRenderer sr;

    Transform player;

    bool isExploding = false;
    bool isChasing = false;

    int frameIndex = 0;
    float frameTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isExploding) return;

        DetectPlayer();
        HandleAnimation();
    }

    void DetectPlayer()
    {
        Collider2D hit = Physics2D.OverlapCircle(transform.position, detectRange, playerLayer);

        if (hit != null)
        {
            player = hit.transform;

            float distance = Vector2.Distance(transform.position, player.position);

            if (distance > explodeRange)
            {
                isChasing = true;
                MoveToPlayer();
            }
            else
            {
                StartCoroutine(Explode());
            }
        }
        else
        {
            isChasing = false;
            rb.linearVelocity = Vector2.zero;
        }
    }

    void MoveToPlayer()
    {
        Vector2 dir = (player.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        // Lật sprite
        if (dir.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = dir.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void HandleAnimation()
    {
        Sprite[] currentFrames = idleFrames;

        if (isChasing)
            currentFrames = chaseFrames;

        if (currentFrames == null || currentFrames.Length == 0)
            return;

        frameTimer += Time.deltaTime;

        if (frameTimer >= frameRate)
        {
            frameTimer = 0f;
            frameIndex++;

            if (frameIndex >= currentFrames.Length)
                frameIndex = 0;

            sr.sprite = currentFrames[frameIndex];
        }
    }

    IEnumerator Explode()
    {
        isExploding = true;
        isChasing = false;
        rb.linearVelocity = Vector2.zero;

        // Chạy animation nổ
        for (int i = 0; i < deathFrames.Length; i++)
        {
            sr.sprite = deathFrames[i];
            yield return new WaitForSeconds(frameRate);
        }

        // Gây damage + knockback
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, playerLayer);

        foreach (Collider2D hit in hits)
        {
            PlayerHealth health = hit.GetComponent<PlayerHealth>();
            if (health != null)
                health.TakeDamage(damage);
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                Vector2 force = dir * knockbackForce;
                health.ApplyKnockback(force, 0.2f);
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}