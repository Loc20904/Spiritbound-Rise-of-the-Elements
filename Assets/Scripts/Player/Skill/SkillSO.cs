using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType { Active, Passive }

[CreateAssetMenu(menuName = "Skill System/Skill")]
public class SkillSO : ScriptableObject
{
    [Header("Basic Info")]
    public string skillName;
    public SkillType type;
    public Sprite icon;
    public string description;

    [Header("Unlock Status")]
    public bool isUnlocked = false;

    [Header("Unlock Conditions")]
    // ĐÂY LÀ CHÌA KHÓA DYNAMIC: Bạn có thể kéo thả bao nhiêu điều kiện tùy thích vào List này từ Inspector
    public List<UnlockConditionSO> unlockConditions;

    [Header("Active Skill Settings")]
    [SerializeField] public float cooldown = 1f; // Cooldown cho active skill

    public virtual IEnumerator Activate(GameObject player)
    {
        Debug.Log($"Kỹ năng {name} đã được kích hoạt, nhưng chưa có logic cụ thể!");
        return null;
    }

    // Hàm kiểm tra xem kỹ năng đã đủ điều kiện mở chưa
    public bool EvaluateUnlock(PlayerStats stats)
    {
        if (isUnlocked) return true; // Đã mở rồi thì bỏ qua

        // Phải thỏa mãn TOÀN BỘ điều kiện trong danh sách mới được mở khóa
        foreach (var condition in unlockConditions)
        {
            if (!condition.IsMet(stats)) return false;
        }

        isUnlocked = true;
        Debug.Log($"Đã mở khóa kỹ năng: {skillName}!");
        return true;
    }
}