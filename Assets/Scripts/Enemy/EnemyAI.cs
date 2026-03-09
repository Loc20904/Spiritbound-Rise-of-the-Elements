using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public float attackRange = 1.2f;
    public float attackCooldown = 1.0f;
    public LayerMask playerLayer;

    private float nextAttackTime;
    private Animator anim;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (Time.time < nextAttackTime) return;

        // check player có ở gần không
        Collider2D player = Physics2D.OverlapCircle(transform.position, attackRange, playerLayer);
        if (player != null)
        {
            // trigger animation attack (bạn tạo Trigger "Attack" trong Animator)
            anim.SetTrigger("Attack");
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
