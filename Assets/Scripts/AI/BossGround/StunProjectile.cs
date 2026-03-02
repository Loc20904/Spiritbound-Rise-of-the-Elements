using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class StunProjectile : MonoBehaviour
{
    [Header("Cài đặt bay (Kiếm Thần)")]
    public float startSpeed = 2f;      // Bay chậm lúc đầu
    public float acceleration = 40f;   // Tăng tốc cực nhanh (Kiếm bay vút đi)
    public float maxSpeed = 50f;       // Tốc độ tối đa

    [Header("Effect")]
    public float damage = 10f;
    public GameObject hitVFX; // Hiệu ứng nổ/gãy kiếm khi va chạm

    private bool isLaunched = false;
    private bool hasHit = false;
    private float currentSpeed;

    void Start()
    {
        currentSpeed = startSpeed;
        Destroy(gameObject, 6f); // Tự hủy nếu bay ra khỏi map
    }

    public void Launch(Transform target)
    {
        if (target == null) return;

        // 1. Chốt hướng ngắm lần cuối
        Vector2 direction = (target.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2. Tách khỏi Boss
        transform.SetParent(null);
        isLaunched = true;
    }

    void Update()
    {
        if (!isLaunched || hasHit) return;

        // Tăng tốc
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Min(currentSpeed, maxSpeed);

        // Bay thẳng theo hướng mũi kiếm (Vector3.right của object)
        transform.Translate(Vector3.right * currentSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isLaunched || hasHit) return;

        if (other.CompareTag("Player") || other.CompareTag("Ground"))
        {
            hasHit = true;
            currentSpeed = 0;

            // Ẩn hình ảnh kiếm
            var sprite = GetComponentInChildren<SpriteRenderer>(); // Dùng GetComponentInChildren cho an toàn
            if (sprite) sprite.enabled = false;

            var col = GetComponent<Collider2D>();
            if (col) col.enabled = false;

            if (other.CompareTag("Player"))
            {
                // Gây damage logic ở đây
                Debug.Log("Kiếm đâm trúng Player!");
            }

            if (hitVFX != null) Instantiate(hitVFX, transform.position, Quaternion.identity);

            Destroy(gameObject, 0.1f);
        }
    }
}