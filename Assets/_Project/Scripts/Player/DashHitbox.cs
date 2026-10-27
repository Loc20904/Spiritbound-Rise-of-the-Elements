using UnityEngine;

public class DashHitbox : MonoBehaviour
{
    [SerializeField] private PlayerController player;

    private void Awake()
    {
        if (player == null) player = GetComponentInParent<PlayerController>();
    }

    private void Reset()
    {
        player = GetComponentInParent<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (player == null) return;

        // ✅ ai có IDamageable thì coi là enemy
        if (other.GetComponentInParent<IDamageable>() != null)
        {
            player.OnDashHit(other.gameObject);
        }
    }
}
