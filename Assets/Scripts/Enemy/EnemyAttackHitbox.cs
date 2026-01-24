using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyAttackHitbox : MonoBehaviour
{
    public LayerMask playerLayer; // set Player layer
    private bool alreadyHit;

    private Collider2D col;
    private EnemyStats stats;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        stats = GetComponentInParent<EnemyStats>();
    }

    // Animation Event
    public void HitboxOn()
    {
        alreadyHit = false;
        DoHitCheck();
    }

    // Animation Event
    public void HitboxOff() { }

    void DoHitCheck()
    {
        if (alreadyHit) return;
        if (col == null) return;
        if (playerLayer.value == 0) return;

        Bounds b = col.bounds;
        Collider2D hit = Physics2D.OverlapBox(b.center, b.size, 0f, playerLayer);
        if (hit == null) return;

        var player = hit.GetComponentInParent<PlayerStats>();
        if (player != null)
        {
            int dmg = (stats != null) ? stats.damage : 1;
            player.TakeDamage(dmg);
            alreadyHit = true;
        }
    }
}
