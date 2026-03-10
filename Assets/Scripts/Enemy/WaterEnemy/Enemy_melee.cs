using UnityEngine;

public class EnemyMelee : EnemyBase
{
    [Header("Melee Damage")]
    public float damage = 5f;
    public Transform hitPoint;     // vị trí đánh
    public float hitRadius = 0.8f;
    public LayerMask playerLayer;

    protected override void Tick(float distToTarget)
    {
        if (distToTarget > attackRange)
        {
            MoveTowardTarget();
        }
        else
        {
            StopMove();
            if (CanAttack())
                TriggerAttack();
        }
    }

    // Gọi bằng Animation Event ở frame “trúng đòn”
    public void AnimEvent_DoMeleeHit()
    {
        if (IsDead()) return;
        Debug.Log("Melee hit event fired!");
        Vector2 center = hitPoint ? (Vector2)hitPoint.position : (Vector2)transform.position;
        Collider2D hit = Physics2D.OverlapCircle(center, hitRadius, playerLayer);
        if (hit != null)
        {
            // Player của bạn nên có hàm TakeDamage
            hit.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 center = hitPoint ? hitPoint.position : transform.position;
        Gizmos.DrawWireSphere(center, hitRadius);
    }
}