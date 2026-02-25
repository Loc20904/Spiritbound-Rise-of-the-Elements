using System.Collections;
using UnityEngine;

public class UltiChainController : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 50;

    private Animator anim;

    // Yêu cầu Prefab xích phải có một Collider2D (VD: BoxCollider2D) và đã tick vào ô "Is Trigger"
    private Collider2D chainCollider;

    void Awake()
    {
        chainCollider = GetComponent<Collider2D>();
        // Phase 1: Vừa sinh ra, xích chưa đâm tới -> Tắt va chạm (Hitbox OFF)
        if (chainCollider != null)
        {
            chainCollider.enabled = false;
        }
        anim = GetComponent<Animator>();
    }

    // --- CÁC HÀM NÀY SẼ ĐƯỢC GỌI BỞI ANIMATION EVENT ---

    // Gọi ở Frame bắt đầu Phase 2 (Xích lao ra)
    public void EnableHitbox()
    {
        if (chainCollider != null) chainCollider.enabled = true;
    }

    // Gọi ở Frame bắt đầu Phase 3 (Xích thu lại hoặc dừng lại)
    public void DisableHitbox()
    {
        if (chainCollider != null) chainCollider.enabled = false;
    }

    // Xử lý gây sát thương khi chạm Player (trong lúc Hitbox đang bật ở Phase 2)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem có đúng layer của Player không
        if (collision.CompareTag("Player"))
        {
            Debug.Log("<color=red>Player dính Xích!</color>");

            // Lấy script máu của player và trừ máu
            // collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            // Nếu có hiệu ứng trói/stun, gọi ở đây
            // collision.GetComponent<PlayerMovement>()?.Stun(1.5f);

            // Tùy chọn: Tắt hitbox luôn sau khi trúng đích để không gây sát thương nhiều lần (multi-hit)
            DisableHitbox();
        }
    }

    // Gọi ở Frame cuối cùng của Phase 3 để tự hủy xích
    public void DestroyChain()
    {
        Destroy(gameObject);
    }

    public void delay()
    {
        StartCoroutine(delayCoroutine());
    }

    public IEnumerator delayCoroutine()
    {
        // 1. Chỉnh tốc độ Animator về 0 -> Animation lập tức đóng băng tại frame hiện tại
        anim.speed = 0f;

        // 2. Đứng chờ đúng 1 giây
        yield return new WaitForSeconds(1f);

        // 3. Chỉnh tốc độ Animator về lại 1 -> Animation tiếp tục chạy bình thường
        anim.speed = 1f;
    }
}