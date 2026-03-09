using UnityEngine;

public class WaterBeamDamage : MonoBehaviour
{
    [Header("Beam Stats")]
    public float damagePerSecond = 20f; // Sát thương mỗi giây
    public float tickRate = 0.2f;       // Tần suất gây damage (0.2s một lần)
    public float knockbackForce = 5f;   // Lực đẩy lùi

    private float nextDamageTime = 0f;

    // Dùng OnTriggerStay để gây damage liên tục khi tia nước đang quét qua
    void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && Time.time >= nextDamageTime)
        {
            // 1. Gây Damage
            // Lưu ý: Thay "TakeDamage" bằng tên hàm trong script máu của Player bạn
            collision.SendMessage("TakeDamage", damagePerSecond * tickRate, SendMessageOptions.DontRequireReceiver);

            // 2. Đẩy lùi Player (Knockback) - Tạo cảm giác tia nước mạnh
            Rigidbody2D pRb = collision.GetComponent<Rigidbody2D>();
            if (pRb != null)
            {
                // Hướng đẩy: Từ tâm tia nước đẩy ra
                Vector2 knockDir = (collision.transform.position - transform.position).normalized;
                pRb.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            }

            // Reset timer
            nextDamageTime = Time.time + tickRate;
        }
    }

}