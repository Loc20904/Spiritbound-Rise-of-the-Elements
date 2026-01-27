using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float damageMultiplier = 1f; // J=1, K=1.2, L=1.6...

    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponentInParent<PlayerStats>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;

        var hp = other.GetComponent<EnemyHealth>();
        if (hp == null) return;

        int dmg = Mathf.RoundToInt(stats.damage * damageMultiplier);
        hp.TakeDamage(dmg);
    }
}
