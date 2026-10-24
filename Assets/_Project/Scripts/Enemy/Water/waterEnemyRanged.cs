using UnityEngine;

public class waterEnemyRanged : EnemyBase
{
    [Header("Ranged Attack")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float retreatDistance = 2f;

    protected override void Tick(float distToTarget)
    {
        if (target == null) return;

        Vector2 dirToTarget = (target.position - transform.position).normalized;

        // flip
        if (dirToTarget.x != 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (dirToTarget.x > 0 ? 1 : -1);
            transform.localScale = s;
        }

        // ra quá loseRange thì bỏ luôn
        if (distToTarget > loseRange)
        {
            StopMove();
            return;
        }

        // quá gần thì lùi
        if (distToTarget < retreatDistance)
        {
            MoveAwayFromTarget();
            return;
        }

        // trong tầm bắn thì mới attack
        if (distToTarget <= attackRange)
        {
            StopMove();

            if (CanAttack())
            {
                TriggerAttack();
            }

            return;
        }

        // ngoài tầm bắn thì đuổi
        MoveTowardTarget();
    }

    private void MoveAwayFromTarget()
    {
        Vector2 dir = (transform.position - target.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        if (dir.x != 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (dir.x > 0 ? 1 : -1);
            transform.localScale = s;
        }

        SetSpeedAnim(rb.linearVelocity.magnitude);
    }

    public void AnimEvent_SpawnBullet()
    {
        if (IsDead()) return;
        if (bulletPrefab == null || firePoint == null || target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);

        // ra ngoài tầm thì không bắn
        if (dist > attackRange || dist > loseRange) return;

        Vector2 dir = (target.position - firePoint.position).normalized;

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        EnemyBullet bullet = bulletObj.GetComponent<EnemyBullet>();

        if (bullet != null)
        {
            bullet.Init(dir);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);

        if (firePoint != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(firePoint.position, 0.08f);
        }
    }
}