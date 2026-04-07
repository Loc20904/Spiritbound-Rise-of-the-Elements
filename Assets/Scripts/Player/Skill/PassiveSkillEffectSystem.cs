using UnityEngine;

/// <summary>
/// Áp dụng các hiệu ứng (modifier) từ unlocked passive skills
/// Tích cộng STT các effect từ nhiều skills
/// </summary>
public class PassiveSkillEffectSystem : MonoBehaviour
{
    private SkillManager skillManager;
    private PlayerStats playerStats;
    private PlayerController playerMove;

    // Danh sách các modifier hiện tại được áp dụng
    private float speedMultiplier = 1f;
    private float damageMultiplier = 1f;
    private float healthMultiplier = 1f;

    private void Awake()
    {
        skillManager = GetComponent<SkillManager>();
        playerStats = GetComponent<PlayerStats>();
        playerMove = GetComponent<PlayerController>();
    }

    private void OnEnable()
    {
        if (skillManager != null)
        {
            skillManager.OnSkillUnlocked += ApplyPassiveSkillEffect;
        }
    }

    private void OnDisable()
    {
        if (skillManager != null)
        {
            skillManager.OnSkillUnlocked -= ApplyPassiveSkillEffect;
        }
    }

    private void ApplyPassiveSkillEffect(SkillSO skill)
    {
        if (skill.type != SkillType.Passive) return;

        Debug.Log($"[PassiveSkillEffectSystem] Áp dụng hiệu ứng passive: {skill.skillName}");

        // Parse description để tìm modifier
        // Format: "Speed +20% | Damage +10%" hoặc "Speed -10%" etc
        string[] effects = skill.description.Split('|');

        foreach (string effect in effects)
        {
            string trimmed = effect.Trim();
            if (trimmed.Contains("Speed"))
            {
                if (TryParse(trimmed, out float value))
                    speedMultiplier *= (1f + value / 100f);
            }
            else if (trimmed.Contains("Damage"))
            {
                if (TryParse(trimmed, out float value))
                    damageMultiplier *= (1f + value / 100f);
            }
            else if (trimmed.Contains("Health"))
            {
                if (TryParse(trimmed, out float value))
                    healthMultiplier *= (1f + value / 100f);
            }
        }

        // Áp dụng các modifier vào player
        ApplyModifiers();
    }

    private bool TryParse(string effect, out float value)
    {
        value = 0f;
        // Tìm số trong chuỗi (có thể là +10%, -5% etc)
        string[] parts = effect.Split('+', '-', '%');
        foreach (string part in parts)
        {
            if (float.TryParse(part.Trim(), out float result))
            {
                value = (effect.Contains("-") && !effect.StartsWith("-")) ? -result : result;
                return true;
            }
        }
        return false;
    }

    private void ApplyModifiers()
    {
        // Áp dụng speed multiplier
        if (playerMove != null)
        {
            playerMove.moveSpeed = speedMultiplier;
        }

        // Áp dụng damage multiplier
        if (playerStats != null)
        {
            // Sẽ cập nhật trong PlayerAttack hoặc PlayerStats
        }

        Debug.Log($"[PassiveSkillEffectSystem] Speed: x{speedMultiplier:F2}, Damage: x{damageMultiplier:F2}, Health: x{healthMultiplier:F2}");
    }

    public float GetSpeedMultiplier() => speedMultiplier;
    public float GetDamageMultiplier() => damageMultiplier;
    public float GetHealthMultiplier() => healthMultiplier;

    public void ResetModifiers()
    {
        speedMultiplier = 1f;
        damageMultiplier = 1f;
        healthMultiplier = 1f;
        ApplyModifiers();
    }
}
