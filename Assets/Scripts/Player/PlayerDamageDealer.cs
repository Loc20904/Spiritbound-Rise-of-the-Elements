using UnityEngine;

public class PlayerDamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 20;

    [Tooltip("Chọn layer của quái vật (vd: 'Enemy') hoặc Tag 'Enemy'")]
    public LayerMask enemyLayer;

    [Tooltip("Bật: Chỉ tính sát thương 1 lần mỗi lần quẹt trúng (như chém kiếm).\nTắt: Trừ máu liên tục khi quấn vào (như bãi lửa cháy lâu).")]
    public bool isSingleHit = true;
    
    // Lưu trạng thái để tránh 1 chiêu chém trúng 10 lần
    private bool hasDealtDamage = false;

    private void OnEnable()
    {
        // Mỗi lần bật Hitbox lên (vd: AttackController tự bật) thì reset lại cờ cho phép gây sát thương
        hasDealtDamage = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu đã chém trúng rồi và đang cài đặt 1 Hit -> Không ăn thêm dmg nữa
        if (isSingleHit && hasDealtDamage) return;

        // Kiểm tra Layer có trùng với enemyLayer không, HOẶC Tag có phải là "Enemy" không
        if (((1 << collision.gameObject.layer) & enemyLayer) != 0 || collision.CompareTag("Enemy"))
        {
            // Tìm code EnemyHealth trên quái vật
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            
            // Nếu có thì gọi hàm TakeDamage
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damageAmount);
                
                // Cắm cờ đã gây sát thương xong, chờ tắt hitbox đi bật lại mới đánh tiếp được
                if (isSingleHit)
                {
                    hasDealtDamage = true;
                }
            }
        }
    }
}
