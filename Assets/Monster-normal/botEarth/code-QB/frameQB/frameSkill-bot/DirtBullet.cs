using UnityEngine;

public class DirtBullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float speed = 8f;
    public int damage = 10;
    public float lifeTime = 5f;
    private Collider2D ownerCol;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    [Header("Knockback")]
    public float knockbackForce = 10f;
    public float knockbackDuration = 0.2f;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Init(Vector2 direction, Collider2D owner)
    {
        moveDirection = direction.normalized;
        rb.linearVelocity = moveDirection * speed;

        ownerCol = owner;

        // Ignore collision với bot
        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), ownerCol);

        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu trúng Player
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeDamage(damage);

                //  Knockback
                Vector2 dir = (collision.transform.position - transform.position).normalized;
                Vector2 force = dir * knockbackForce;

                player.ApplyKnockback(force, knockbackDuration);
            }

            Destroy(gameObject);
            return;
        }

        // Xuyên qua đồng minh (Enemy) → không hủy đạn
        if (collision.CompareTag("Enemy"))
            return;
        if (collision.CompareTag("Bullet"))
            return;

        // Chạm thứ khác (tường, nền, v.v.) → hủy đạn
        Destroy(gameObject);
    }
}