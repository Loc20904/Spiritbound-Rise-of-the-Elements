using System; // Cần dùng thư viện này cho Action
using UnityEngine;

public class BossHealth : MonoBehaviour
{
    public float maxHP = 100f;
    public float currentHP;
    public bool isInvulnerable = false;

    // Sự kiện báo cho các script khác biết boss vừa mất bao nhiêu máu
    // float: lượng damage vừa nhận
    public event Action<float> OnDamageTaken;

    public delegate void PhaseChange(float hpPercent);
    public event PhaseChange OnPhaseChanged;

    public delegate void DeathEvent();
    public event DeathEvent OnDeath;

    private void Start()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(float dmg)
    {
        if (isInvulnerable || dmg <= 0f) return;
        currentHP -= dmg;

        // 1. BẮN SỰ KIỆN: "Tôi vừa mất 'dmg' máu!"
        // Dấu ?. để đảm bảo nếu không ai nghe thì không lỗi
        OnDamageTaken?.Invoke(dmg);

        // 2. Xử lý Phase và Chết (như cũ)
        float hpPercent = (currentHP / maxHP) * 100f;
        OnPhaseChanged?.Invoke(hpPercent);

        if (currentHP <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    // Thêm vào trong class BossHealth
    public void Heal(float amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;

        // Gọi event cập nhật UI (nếu có thanh máu)
        OnPhaseChanged?.Invoke((currentHP / maxHP) * 100f);
        Debug.Log($"Boss healed: {amount}. Current HP: {currentHP}");
    }

    [ContextMenu("TEST: Vào ngay Phase 2")]
    public void TestEnterPhase2()
    {
        // 1. Ép máu xuống còn 40%
        float damageToDeal = maxHP * 0.6f; // Trừ đi 60% máu

        // 2. Gọi hàm TakeDamage để kích hoạt toàn bộ sự kiện (OnPhaseChanged, Teleport...)
        TakeDamage(damageToDeal);

        Debug.Log("Đã kích hoạt Test Phase 2: Máu còn 40%");
    }
}