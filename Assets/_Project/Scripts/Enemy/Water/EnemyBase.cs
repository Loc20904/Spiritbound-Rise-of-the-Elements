using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Stats")]
    public float maxHP = 20f;
    public float moveSpeed = 2.5f;

    [Header("Vision / Aggro")]
    public float visionRange = 6f;
    public float loseRange = 8f;
    public bool requireLineOfSight = false;
    public LayerMask obstacleMask;

    [Header("Attack")]
    public float attackRange = 1.2f;
    public float attackCooldown = 1.0f;

    [Header("Death Animation")]
    public string deadAnimName = "quainuocdanhgan_Dead"; // mặc định cho melee
    public float deathDelay = 2f;

    protected float hp;
    protected float lastAttackTime;
    protected Rigidbody2D rb;
    protected Animator anim;
    protected SpriteRenderer sr;
    protected EnemyDropTable dropTable;

    protected bool isAggro;

    protected virtual void Awake()
    {
        hp = maxHP;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        dropTable = GetComponent<EnemyDropTable>();
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
            if (dist >= loseRange)
            {
                isAggro = false;
                StopMove();
                return;
            }
        }

        Tick(dist);
    }

    bool HasSight()
    {
        if (!requireLineOfSight) return true;

        Vector2 origin = transform.position;
        Vector2 dir = (target.position - transform.position);
        float dist = dir.magnitude;

        RaycastHit2D hit = Physics2D.Raycast(origin, dir.normalized, dist, obstacleMask);
        return hit.collider == null;
    }

    protected abstract void Tick(float distToTarget);

    protected bool CanAttack() => Time.time >= lastAttackTime + attackCooldown;

    protected void TriggerAttack()
    {
        lastAttackTime = Time.time;
        if (anim) anim.SetTrigger("Attack");
    }

    protected void MoveTowardTarget()
    {
        Vector2 dir = (target.position - transform.position).normalized;
        rb.linearVelocity = dir * moveSpeed;

        if (dir.x != 0)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Abs(s.x) * (dir.x > 0 ? 1 : -1);
            transform.localScale = s;
        }

        SetSpeedAnim(rb.linearVelocity.magnitude);
    }

    protected void StopMove()
    {
        if (rb) rb.linearVelocity = Vector2.zero;
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
        else isAggro = true;
    }

    protected virtual void Die()
    {
        hp = 0;

        StopMove();

        if (anim && !string.IsNullOrEmpty(deadAnimName))
            anim.Play(deadAnimName, 0, 0f);

        if (rb)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.simulated = false;
        }

        gameObject.layer = LayerMask.NameToLayer("DeadEnemy");

        StartCoroutine(DieRoutine());
    }

    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(deathDelay);

        if (dropTable != null)
            dropTable.TryDrop(transform.position);

        Destroy(gameObject);
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