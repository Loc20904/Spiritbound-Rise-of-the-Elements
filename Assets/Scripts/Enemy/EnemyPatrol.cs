using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;

    public float speed = 2f;
    public float arriveDistance = 0.05f;

    public bool flipByDirection = true;

    private Rigidbody2D rb;
    private Transform target;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (pointA == null || pointB == null)
        {
            Debug.LogError("EnemyPatrolRB2D: Chưa gán pointA/pointB");
            enabled = false;
            return;
        }

        target = pointB;

        // Gợi ý cho Dynamic để không bị kẹt/đứng im do ma sát
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Nếu bạn không muốn nó trượt/ma sát làm đứng:
        rb.sharedMaterial = null; // hoặc gán PhysicsMaterial2D ma sát thấp
    }

    private void FixedUpdate()
    {
        if (target == null) return;

        Vector2 pos = rb.position;
        Vector2 next = Vector2.MoveTowards(pos, target.position, speed * Time.fixedDeltaTime);
        rb.MovePosition(next);

        // tới điểm -> đổi hướng
        if (Vector2.Distance(next, target.position) <= arriveDistance)
            target = (target == pointA) ? pointB : pointA;

        // lật mặt
        if (flipByDirection)
        {
            float dir = target.position.x - transform.position.x;
            if (dir != 0)
            {
                Vector3 s = transform.localScale;
                s.x = Mathf.Abs(s.x) * (dir > 0 ? 1 : -1);
                transform.localScale = s;
            }
        }
    }
}
