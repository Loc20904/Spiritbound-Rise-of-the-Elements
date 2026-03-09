using UnityEngine;

public class ArrowBullet : MonoBehaviour
{
    public float speed = 8f;
    public int damage = 2;

    [Header("Fire Damage")]
    public int damagePerTick = 2;
    public float burnDuration = 3f;
    public float tickRate = 0.5f;

    Vector2 dir;

    Collider2D ownerCol;
    float spawnIgnoreTime = 0.05f; // ⭐ chống tự chạm lúc spawn
    float timer;

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

        Destroy(gameObject, 3f);
    }

    // ================= UPDATE =================
    void Update()
    {
        timer += Time.deltaTime;

        transform.Translate(dir * speed * Time.deltaTime);
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

            PlayerFireDOT fire = other.GetComponent<PlayerFireDOT>();
            if (fire != null)
                fire.ApplyBurn(damagePerTick, burnDuration, tickRate);

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
