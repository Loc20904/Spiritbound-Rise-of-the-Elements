using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    // Tham chiếu đến file chứa dữ liệu của người chơi (Level, Khoảng cách...)
    private PlayerStats playerStats;
    private SkillSlotManager skillSlotManager;

    [Header("All Available Skills")]
    // Kéo thả tất cả các cục SkillSO bạn tạo ra vào đây
    public List<SkillSO> allSkills;

    public List<SkillSO> activeSkills = new List<SkillSO>();
    public List<SkillSO> passiveSkills = new List<SkillSO>();

    public event Action<SkillSO> OnSkillUnlocked; // Event khi một skill được unlock

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        skillSlotManager = GetComponent<SkillSlotManager>();
    }

    private void OnEnable()
    {
        // Khi bật lên, dặn dò: "Ê PlayerStats, lúc nào OnStatsChanged reo, hãy gọi hàm CheckAllSkillsUnlock cho tôi"
        if (playerStats != null)
        {
            playerStats.OnStatsChanged += CheckAllSkillsUnlock;

        }
    }

    private void OnDisable()
    {
        // Nhớ gỡ ra khi tắt để tránh lỗi rò rỉ bộ nhớ (Memory Leak)
        if (playerStats != null)
        {
            playerStats.OnStatsChanged -= CheckAllSkillsUnlock;
        }
    }

    // Hàm này nên được gọi bằng Event khi người chơi Level Up hoặc di chuyển thay vì nhét vào Update()
    public void CheckAllSkillsUnlock()
    {
        foreach (var skill in allSkills)
        {
            if (!skill.isUnlocked)
            {
                // Nếu skill vừa được mở khóa thành công
                if (skill.EvaluateUnlock(playerStats))
                {
                    SortSkillIntoLists(skill);
                    OnSkillUnlocked?.Invoke(skill);
                    
                    // Auto-assign active skills vào các slot trống
                    if (skill.type == SkillType.Active && skillSlotManager != null)
                    {
                        AutoAssignSkillToEmptySlot(skill);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Tự động gán skill active vào slot trống đầu tiên
    /// </summary>
    private void AutoAssignSkillToEmptySlot(SkillSO skill)
    {
        for (int i = 0; i < 4; i++)
        {
            var slot = skillSlotManager.GetSlot(i);
            if (slot != null && slot.assignedSkill == null)
            {
                skillSlotManager.AssignSkillToSlot(i, skill);
                Debug.Log($"[SkillManager] Auto-assigned '{skill.skillName}' to slot {i}");
                return;
            }
        }
        Debug.Log($"[SkillManager] Tất cả các slot đã đầy. Skill '{skill.skillName}' không được gán tự động.");
    }

    /// <summary>
    /// Lấy danh sách tất cả active skills đã unlock để hiển thị trên UI
    /// </summary>
    public List<SkillSO> GetUnlockedActiveSkills()
    {
        var unlockedActive = new List<SkillSO>();
        foreach (var skill in activeSkills)
        {
            if (skill.isUnlocked)
                unlockedActive.Add(skill);
        }
        return unlockedActive;
    }


    private void SortSkillIntoLists(SkillSO skill)
    {
        if (skill.type == SkillType.Active && !activeSkills.Contains(skill))
            activeSkills.Add(skill);
        else if (skill.type == SkillType.Passive && !passiveSkills.Contains(skill))
            passiveSkills.Add(skill);
    }
}