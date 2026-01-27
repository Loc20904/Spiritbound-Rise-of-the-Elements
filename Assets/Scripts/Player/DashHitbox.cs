using UnityEngine;

public class DashHitbox : MonoBehaviour
{
    [SerializeField] private PlayerController player;
    [SerializeField] private string enemyTag = "Enemy";

    private void Reset()
    {
        player = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (player == null) return;

        // cách 1: theo tag
        if (other.CompareTag(enemyTag))
        {
            player.OnDashHit(other.gameObject);
            return;
        }

        // cách 2: ai có IDamageable thì cũng coi là enemy
        if (other.GetComponent<IDamageable>() != null)
        {
            player.OnDashHit(other.gameObject);
        }
    }
}
