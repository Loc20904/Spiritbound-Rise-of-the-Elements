using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Health")]
    public int maxHP = 100;
    int currentHP;

    [Header("Debug")]
    public bool showDamageLog = true; // Hiển thị log damage

    void Start()
    {
        currentHP = maxHP;
    }

    // ================= TAKE DAMAGE =================
    public void TakeDamage(int damage)
    {
        // Mặc định là damage từ boss (melee/ranged attack)
        TakeDamage(damage, DamageType.Boss);
    }

    public void TakeDamage(int damage, DamageType damageType)
    {
        if (currentHP <= 0) return;

        currentHP -= damage;
        currentHP = Mathf.Max(0, currentHP);

        // ⭐ Hiển thị damage trong game (floating text hoặc UI)
        ShowDamageInGame(damage, damageType);

        // Hiển thị thông tin damage trong log
        if (showDamageLog)
        {
            string damageSource = damageType == DamageType.Boss ? "Boss" : "Fire DOT";
            string damageTypeName = damageType == DamageType.Boss ? "Damage cơ bản" : "Damage over time";
            
            Debug.Log($"[PlayerHealth] Nhận {damage} damage từ {damageSource} ({damageTypeName}) | HP: {currentHP}/{maxHP}");
        }

        // Kiểm tra chết
        if (currentHP <= 0)
        {
            Die();
        }
    }

    // ================= ON TAKE FIRE DOT DAMAGE =================
    // ⭐ Hàm này được gọi từ PlayerFireDOT để log damage type (không trừ damage 2 lần)
    void OnTakeFireDOTDamage(int damage)
    {
        // Chỉ log, không trừ damage vì đã trừ trong TakeDamage()
        if (showDamageLog)
        {
            Debug.Log($"[PlayerHealth] Fire DOT damage: {damage} | HP: {currentHP}/{maxHP}");
        }
    }

    // ================= SHOW DAMAGE IN GAME =================
    void ShowDamageInGame(int damage, DamageType damageType)
    {
        // ⭐ Gửi message để hệ thống hiển thị damage (tương thích với hệ thống cũ)
        // Có thể là floating text, UI, hoặc bất kỳ hệ thống nào đã có sẵn
        SendMessage(
            "OnTakeDamage",
            new DamageInfo { damage = damage, damageType = damageType },
            SendMessageOptions.DontRequireReceiver
        );

        // ⭐ Hoặc hiển thị trực tiếp nếu có component FloatingText hoặc DamageDisplay
        // Ví dụ: FloatingText.Show("-" + damage + "hp", transform.position, Color.red);
    }

    // ================= DIE =================
    void Die()
    {
        Debug.Log("[PlayerHealth] Player đã chết!");
        // Thêm logic chết ở đây (ví dụ: reload scene, show game over, etc.)
    }

    // ================= PUBLIC =================
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public float HealthPercent => maxHP > 0 ? (float)currentHP / maxHP : 0f;
}

// ================= DAMAGE TYPE =================
public enum DamageType
{
    Boss,      // Damage từ boss (melee/ranged attack)
    FireDOT    // Damage từ fire DOT
}

// ================= DAMAGE INFO =================
public struct DamageInfo
{
    public int damage;
    public DamageType damageType;
}
