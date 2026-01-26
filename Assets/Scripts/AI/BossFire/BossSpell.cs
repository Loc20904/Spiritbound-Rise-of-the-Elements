using UnityEngine;

public class BossSpell : MonoBehaviour
{
    public float speed = 10f;
    public float lifeTime = 4f;
    public float damage = 10f;

    [Header("Knockback Effect")]
    public float knockbackForce = 10f;
    public GameObject hitVFX;

    [Header("Audio")] // --- THÊM MỚI ---
    public AudioClip explosionSound; // Kéo file âm thanh nổ vào đây
    [Range(0f, 1f)]
    public float soundVolume = 0.8f; // Chỉnh độ to nhỏ

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Trừ máu player ở đây...

            // Xử lý đẩy lùi
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                Vector2 direction = (other.transform.position - transform.position).normalized;
                direction.y = 0.2f;
                playerRb.linearVelocity = Vector2.zero;
                playerRb.AddForce(direction * knockbackForce, ForceMode2D.Impulse);
            }

            Hit(); // Gọi hàm va chạm
        }
        else if (other.CompareTag("Ground") || other.CompareTag("Wall"))
        {
            Hit(); // Gọi hàm va chạm
        }
    }

    void Hit()
    {
        // 1. Tạo hiệu ứng hình ảnh
        if (hitVFX) Instantiate(hitVFX, transform.position, Quaternion.identity);

        // 2. TẠO HIỆU ỨNG ÂM THANH (QUAN TRỌNG)
        if (explosionSound != null)
        {
            // Tạo một GameObject tạm thời tại vị trí nổ để phát âm thanh rồi tự hủy
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, soundVolume);
        }

        // 3. Xóa viên đạn
        Destroy(gameObject);
    }
}