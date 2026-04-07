using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    public float damageMultiplier = 1f;
    public LayerMask enemyLayer;

    //private PlayerHealth ph;
    private PlayerStats stats;

    private void Awake()
    {
        stats = GetComponentInParent<PlayerStats>();
        //ph = GetComponentInParent<PlayerHealth>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log(other.tag+" "+other.layer);
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;
        if (stats == null) return;

        int dmg = Mathf.RoundToInt(stats.Damage * damageMultiplier);

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(dmg);
        }

        other.SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);
    }
}