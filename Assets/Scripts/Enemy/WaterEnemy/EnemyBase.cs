using UnityEngine;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Stats")]
    public float maxHP = 20f;
    public float moveSpeed = 2.5f;

    [Header("Vision / Aggro")]
    public float visionRange = 6f;     // vào tầm này mới bắt đầu đuổi
    public float loseRange = 8f;       // ra quá tầm này thì bỏ đuổi (nên > visionRange)
    public bool requireLineOfSight = false;
    public LayerMask obstacleMask;     // layer tường/ground để raycast (nếu dùng LOS)

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.0f;

    protected float hp;
    protected float lastAttackTime;
    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;

    protected bool isAggro;

    protected virtual void Awake()
    {
        hp = maxHP;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    protected virtual void Start()
    {
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
        }
    }

    protected virtual void Update()
    {
        if (IsDead() || target == null) return;

        float dist = Vector2.Distance(transform.position, target.position);

        // 1) Cập nhật aggro theo tầm nhìn
        if (!isAggro)
        {
            if (dist <= visionRange && HasSight())
                isAggro = true;
            else
            {
                StopMove();
                return;
            }
        }
        else
        {
            if (dist >= loseRange) // mất mục tiêu
            {
                isAggro = false;
                StopMove();
                return;
            }
        }

        // 2) Đang aggro thì chạy AI thường
        Tick(dist);
    }

    bool HasSight()
    {
        if (!requireLineOfSight) return true;

        Vector2 origin = transform.position;
        Vector2 dir = (target.position - transform.position);
        float dist = dir.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, dist, obstacleMask);
        return hit.collider == null; // không bị tường che
    }

    protected abstract void Tick(float distToTarget);

    protected bool CanAttack() => Time.time >= lastAttackTime + attackCooldown;

    protected void TriggerAttack()
    {
        lastAttackTime = Time.time;
        anim.SetTrigger("Attack");
    }

    protected void MoveTowardTarget()
    {
        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
        if (dir.x != 0) sr.flipX = dir.x < 0;
        SetSpeedAnim(rb.linearVelocity.magnitude);
    }

    protected void StopMove()
    {
        rb.linearVelocity = Vector2.zero;
        SetSpeedAnim(0f);
    }

    protected void SetSpeedAnim(float speed)
    {
        if (anim) anim.SetFloat("Speed", speed);
    }

    public virtual void TakeDamage(float dmg)
    {
        if (IsDead()) return;

        hp -= dmg;
        if (hp <= 0) Die();
        else isAggro = true; // bị đánh thì aggro luôn (tuỳ bạn)
    }

    protected virtual void Die()
    {
        hp = 0;

        // dừng AI + animation
        StopMove();
        if (anim) anim.SetBool("Dead", true);

        // khóa physics để không bị rơi / bay
        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Static;   // hoặc Kinematic
        }

        // KHÔNG tắt collider chính (để còn đứng trên ground)
        // nhưng có thể đổi layer để không chặn player nữa
        gameObject.layer = LayerMask.NameToLayer("DeadEnemy"); // tạo layer này và disable collision với Player

        Destroy(gameObject, 2f);
    }

    protected bool IsDead() => hp <= 0;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, visionRange);
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(transform.position, loseRange);
    }
}