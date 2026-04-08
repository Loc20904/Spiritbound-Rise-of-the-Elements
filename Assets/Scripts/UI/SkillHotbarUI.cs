using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI hiển thị 4 skill slots với icon, tên skill, cooldown indicator
/// </summary>
public class SkillHotbarUI : MonoBehaviour
{
    [System.Serializable]
    public class SlotUI
    {
        public Image skillIcon;
        //public TextMeshProUGUI skillNameText;
        public TextMeshProUGUI hotkeyText;
        public Image cooldownOverlay; // Tây overlay để hiện thị cooldown
        public TextMeshProUGUI cooldownText;
    }

    [SerializeField] private SlotUI[] slotUIs = new SlotUI[4];
    [SerializeField] private SkillSelectionPanel skillSelectionPanel; // Reference to panel
    public SkillSlotManager skillSlotManager;
    public SkillManager skillManager;

    private void Awake()
    {
        // Tìm SkillSlotManager từ player
        //skillSlotManager = FindObjectOfType<SkillSlotManager>();
        //skillManager = FindObjectOfType<SkillManager>();

        // Tìm SkillSelectionPanel nếu không assign
        if (skillSelectionPanel == null)
            skillSelectionPanel = FindObjectOfType<SkillSelectionPanel>();

        if (skillSlotManager == null)
        {
            Debug.LogError("[SkillHotbarUI] SkillSlotManager không tìm thấy!");
            return;
        }

        if (skillManager == null)
        {
            Debug.LogError("[SkillHotbarUI] SkillManager không tìm thấy!");
            return;
        }

        if (skillSelectionPanel == null)
        {
            Debug.LogWarning("[SkillHotbarUI] SkillSelectionPanel không tìm thấy - click slot sẽ rotaciona thay vì mở panel!");
        }
    }

    private void OnEnable()
    {
        if (skillSlotManager != null)
        {
            skillSlotManager.OnSkillSlotChanged += UpdateSlotDisplay;
            skillSlotManager.OnSkillUsed += OnSkillUsed;

            // Cập nhật tất cả các slot khi bật UI
            //for (int i = 0; i < 4; i++)
            //{
            //    UpdateSlotDisplay(i);

            //    // Setup button click để chuyển skill
            //    int slotIndex = i; // Capture for closure
            //    var slotTransform = slotUIs[i].skillIcon.transform.parent;
            //    Button slotButton = slotTransform.GetComponent<Button>();
            //    if (slotButton == null)
            //        slotButton = slotTransform.gameObject.AddComponent<Button>();

            //    slotButton.onClick.RemoveAllListeners();
            //    slotButton.onClick.AddListener(() => OnSlotClicked(slotIndex));
            //}

            if (skillManager != null && skillManager.activeSkills.Count > 0)
            {
                int temp = 0;
                foreach (var skill in skillManager.activeSkills)
                {
                    slotUIs[temp].skillIcon.sprite = skill.icon;
                    skillSlotManager.AssignSkillToSlot(temp, skill);
                    temp++;
                    if (temp > 4) break; // Chỉ gán tối đa 4 skills vào hotbar
                }
            }

            if (skillManager != null)
                skillManager.OnSkillUnlocked += OnSkillUnlocked;
        }
    }

    private void OnDisable()
    {
        if (skillSlotManager != null)
        {
            skillSlotManager.OnSkillSlotChanged -= UpdateSlotDisplay;
            skillSlotManager.OnSkillUsed -= OnSkillUsed;
        }

        if (skillManager != null)
            skillManager.OnSkillUnlocked -= OnSkillUnlocked;
    }

    private void Update()
    {
        // Cập nhật cooldown indicator mỗi frame
        for (int i = 0; i < 4; i++)
        {
            UpdateCooldownDisplay(i);
        }
    }

    private void UpdateSlotDisplay(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotUIs.Length)
            return;

        SlotUI slotUI = slotUIs[slotIndex];
        SkillSlotManager.SkillSlot slot = skillSlotManager.GetSlot(slotIndex);

        if (slot == null || slot.assignedSkill == null)
        {
            // Slot trống
            slotUI.skillIcon.sprite = null;
            //slotUI.skillNameText.text = "---";
            slotUI.cooldownOverlay.fillAmount = 0f;
            return;
        }

        // Hiển thị skill info
        slotUI.skillIcon.sprite = slot.assignedSkill.icon;
        //slotUI.skillNameText.text = slot.assignedSkill.skillName;
        slotUI.hotkeyText.text = GetHotkeyLabel(slotIndex);
    }

    private void UpdateCooldownDisplay(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slotUIs.Length)
            return;

        SlotUI slotUI = slotUIs[slotIndex];
        SkillSlotManager.SkillSlot slot = skillSlotManager.GetSlot(slotIndex);

        if (slot == null || slot.assignedSkill == null)
        {
            slotUI.cooldownOverlay.fillAmount = 0f;
            slotUI.cooldownText.text = "";
            return;
        }

        // Cập nhật cooldown overlay
        if (slot.cooldownRemaining > 0f)
        {
            float cooldownPercent = slot.cooldownRemaining / slot.assignedSkill.cooldown;
            slotUI.cooldownOverlay.fillAmount = cooldownPercent;
            slotUI.cooldownText.text = slot.cooldownRemaining.ToString("F1");
        }
        else
        {
            slotUI.cooldownOverlay.fillAmount = 0f;
            slotUI.cooldownText.text = "";
        }
    }

    private void OnSkillUsed(int slotIndex)
    {

    }


    private void OnSkillUnlocked(SkillSO skill)
    {
        Debug.Log($"[SkillHotbarUI] New skill unlocked: {skill.skillName}");
    }

    private string GetHotkeyLabel(int slotIndex)
    {
        return slotIndex switch
        {
            0 => "U",
            1 => "I",
            2 => "O",
            3 => "P",
            _ => ""
        };
    }

    public void OnSlotClicked(int slotIndex)
    {
        Debug.Log($"[SkillHotbarUI] Slot {slotIndex} clicked!");

        // Nếu có panel, mở nó
        if (skillSelectionPanel != null)
        {
            skillSelectionPanel.OpenPanelForSlot(slotIndex);
            Debug.Log($"[SkillHotbarUI] Mở selection panel cho slot {slotIndex}");
            return;
        }

        // Fallback: Rotaciona skills nếu panel không tồn tại
        Debug.Log("[SkillHotbarUI] Panel không tồn tại, rotaciona skills...");
        RotateSkillInSlot(slotIndex);
    }

    private void RotateSkillInSlot(int slotIndex)
    {
        if (skillManager == null)
        {
            Debug.LogWarning("[SkillHotbarUI] SkillManager null!");
            return;
        }

        var unlockedSkills = skillManager.GetUnlockedActiveSkills();

        if (unlockedSkills.Count == 0)
        {
            Debug.Log("[SkillHotbarUI] Nenhum skill ativo desbloqueado!");
            return;
        }

        var currentSlot = skillSlotManager.GetSlot(slotIndex);

        if (currentSlot != null && currentSlot.assignedSkill != null)
        {
            int currentIndex = unlockedSkills.IndexOf(currentSlot.assignedSkill);
            int nextIndex = (currentIndex + 1) % unlockedSkills.Count;
            skillSlotManager.AssignSkillToSlot(slotIndex, unlockedSkills[nextIndex]);
            Debug.Log($"[SkillHotbarUI] Slot {slotIndex}: {currentSlot.assignedSkill.skillName} → {unlockedSkills[nextIndex].skillName}");
        }
        else if (currentSlot != null)
        {
            skillSlotManager.AssignSkillToSlot(slotIndex, unlockedSkills[0]);
            Debug.Log($"[SkillHotbarUI] Slot {slotIndex} agora tem: {unlockedSkills[0].skillName}");
        }
    }
}
