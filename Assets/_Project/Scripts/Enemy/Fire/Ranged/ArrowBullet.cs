using UnityEngine;

public class ArrowBullet : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 2;
    public float lifeTime = 5f;

    [Header("Fire Damage")]
    public int damagePerTick = 2;
    public float burnDuration = 3f;
    public float tickRate = 0.5f;

    [Header("Earth Effect")]
    public float earthKnockbackForce = 8f;
    public float earthKnockbackDuration = 0.2f;

    Vector2 dir;

    Collider2D ownerCol;
    float spawnIgnoreTime = 0.05f; // ⭐ chống tự chạm lúc spawn
    float timer;
    [Header("Element")]
    public ElementType elementType = ElementType.None;
    public enum ElementType
    {
        None,
        Fire,
        Earth
    }
    // ================= INIT =================
    public void Init(Vector2 direction, Collider2D owner)
    {
        dir = direction.normalized;
        ownerCol = owner;

        if (dir.x < 0)
        {
            Vector3 s = transform.localScale;
            s.x *= -1;
            transform.localScale = s;
        }
     
        Destroy(gameObject, lifeTime);
    }

    // ================= UPDATE =================
    void Update()
    {
        timer += Time.deltaTime;

        transform.Translate(dir * speed * Time.deltaTime, Space.World);
    }

    // ================= HIT =================
    void OnTriggerEnter2D(Collider2D other)
    {
        // ⭐ bỏ qua 0.05s đầu (fix spawn chết ngay)
        if (timer < spawnIgnoreTime)
            return;

        // ⭐ bỏ qua toàn bộ enemy bắn (FIX CHUẨN)
        if (ownerCol != null &&
            other.transform.root == ownerCol.transform.root)
            return;

        // ===== player trúng =====
        if (other.CompareTag("Player"))
        {
            // ⭐ Gửi damage với type Boss
            PlayerHealth health = other.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage, DamageType.Boss);
            }
            else
            {
                // Fallback nếu không có PlayerHealth
                other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }

            if (elementType == ElementType.Fire)
            {
                PlayerFireDOT fire = other.GetComponent<PlayerFireDOT>();
                if (fire != null)
                    fire.ApplyBurn(damagePerTick, burnDuration, tickRate);
            }

            if (elementType == ElementType.Earth)
            {
                if (health != null)
                {
                    Vector2 knockDir = (other.transform.position - transform.position).normalized;
                    Vector2 force = knockDir * earthKnockbackForce;

                    health.ApplyKnockback(force, earthKnockbackDuration);
                }
            }

            Destroy(gameObject);
            return;
        }

        // ⭐ xuyên qua Enemy (đồng minh)
        if (other.CompareTag("Enemy"))
            return;

        // ⭐ chạm các object khác (ground, wall, etc.) → destroy
        Destroy(gameObject);
    }
}
