using UnityEngine;

public class EnemyHitBox : MonoBehaviour
{
    public int damage = 20;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("EnemyHitBox touched: " + other.name);

        // ✅ lấy PlayerStats từ cha
        PlayerStats stats = other.GetComponentInParent<PlayerStats>();

        if (stats != null)
        {
            Debug.Log("PlayerStats found → Deal Damage!");
            stats.TakeDamage(damage);
        }
    }
}
