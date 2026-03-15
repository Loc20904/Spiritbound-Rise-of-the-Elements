using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    public float damageMultiplier = 1f;
    public LayerMask enemyLayer;

    private PlayerHealth ph;

    private void Awake()
    {
        ph = GetComponentInParent<PlayerHealth>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;
        if (ph == null) return;

        int dmg = Mathf.RoundToInt(ph.Damage * damageMultiplier);

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(dmg);
        }

        other.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);
    }
}