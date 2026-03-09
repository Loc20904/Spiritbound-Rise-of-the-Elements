using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Damage Source")]
    public bool usePlayerStatsDamage = true;   // bật: lấy damage từ PlayerStats
    public int fallbackDamage = 50;            // dùng khi không tìm thấy PlayerStats

    [Header("Hit Settings")]
    public LayerMask enemyLayer;               // nhớ set Enemy layer ở Inspector

    private bool alreadyHit;
    private Collider2D col;
    private PlayerStats playerStats;

    private void Awake()
    {
        col = GetComponent<Collider2D>();

        // Lấy PlayerStats từ object cha (Player root)
        playerStats = GetComponentInParent<PlayerStats>();

        // Gợi ý: hitbox thường nên là trigger để không bị physics đẩy
        // (không bắt buộc, nhưng khuyên)
        // col.isTrigger = true; // nếu bạn muốn auto bật thì mở dòng này
    }

    // Gọi bằng Animation Event
    public void HitboxOn()
    {
        alreadyHit = false;

        if (col == null)
        {
            Debug.LogWarning("[Hitbox] Missing Collider2D on hitbox object.");
            return;
        }

        // Nếu bạn quên set enemyLayer thì nó sẽ = 0 (Nothing) => không bao giờ hit
        if (enemyLayer.value == 0)
        {
            Debug.LogWarning("[Hitbox] enemyLayer is NOT set! Please assign Enemy layer in Inspector.");
            return;
        }

        DoHitCheck();
    }

    // Gọi bằng Animation Event
    public void HitboxOff()
    {
        // Hiện tại bạn chỉ đánh 1 lần lúc On, nên Off để trống cũng được
        // Debug.Log("[Hitbox] OFF");
    }

    private int GetDamage()
    {
        if (usePlayerStatsDamage && playerStats != null)
            return playerStats.damage; // lấy damage từ PlayerStats

        return fallbackDamage;
    }

    void DoHitCheck()
    {
        if (alreadyHit) return;

        Bounds b = col.bounds;

        // OverlapBox 1 mục tiêu (giữ đúng kiểu bạn đang làm)
        Collider2D hit = Physics2D.OverlapBox(b.center, b.size, 0f, enemyLayer);

        if (hit == null)
            return;

        // Tìm EnemyStats: ưu tiên GetComponent, nếu không có thì tìm parent
        EnemyStats enemy = hit.GetComponent<EnemyStats>();
        if (enemy == null) enemy = hit.GetComponentInParent<EnemyStats>();

        if (enemy != null)
        {
            int dmg = GetDamage();
            enemy.TakeDamage(dmg);
            alreadyHit = true;
        }
        else
        {
            Debug.LogWarning("[Hitbox] Hit collider but EnemyStats not found on it or its parents: " + hit.name);
        }
    }

    // nhìn vùng overlap trong Scene
    private void OnDrawGizmosSelected()
    {
        var c = GetComponent<Collider2D>();
        if (c == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(c.bounds.center, c.bounds.size);
    }
}
