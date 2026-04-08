using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Panel UI để chọn skill cho một slot
/// Mở khi click vào slot, người dùng chọn skill từ danh sách
/// </summary>
public class SkillSelectionPanel : MonoBehaviour
{
    [SerializeField] private GameObject skillButtonPrefab;
    [SerializeField] private Transform skillListContainer;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI instructionText;
    [SerializeField] private Button closeButton;

    public SkillSlotManager skillSlotManager;
    public SkillManager skillManager;

    private int selectedSlotIndex = -1;
    private List<Button> skillButtons = new List<Button>();

    private void Start()
    {
        //skillSlotManager = FindObjectOfType<SkillSlotManager>();
        //skillManager = FindObjectOfType<SkillManager>();

        if (skillSlotManager == null || skillManager == null)
        {
            Debug.LogError("[SkillSelectionPanel] SkillSlotManager hoặc SkillManager không tìm thấy!");
            gameObject.SetActive(false);
            return;
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);

        // Bắt đầu với panel đóng
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Gọi từ SkillHotbarUI khi click vào slot
    /// </summary>
    public void OpenPanelForSlot(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        gameObject.SetActive(true);

        // Cập nhật title
        string hotkeyLabel = slotIndex switch
        {
            0 => "U",
            1 => "I",
            2 => "O",
            3 => "P",
            _ => ""
        };

        if (titleText != null)
            titleText.text = $"Chọn Kỹ Năng cho Slot {hotkeyLabel}";

        RefreshSkillList();
        Debug.Log($"[SkillSelectionPanel] Mở panel cho slot {slotIndex} ({hotkeyLabel})");
    }

    private void RefreshSkillList()
    {
        // Xóa buttons cũ
        foreach (var btn in skillButtons)
        {
            Destroy(btn.gameObject);
        }
        skillButtons.Clear();

        // Lấy danh sách skills đã unlock
        var unlockedSkills = skillManager.GetUnlockedActiveSkills();

        if (unlockedSkills.Count == 0)
        {
            if (instructionText != null)
                instructionText.text = "Chưa có kỹ năng ativa nào được mở khóa!";
            return;
        }

        //if (instructionText != null)
        //    instructionText.text = $"Có {unlockedSkills.Count} kỹ năng có sẵn.";

        // Tạo button cho mỗi skill
        foreach (var skill in unlockedSkills)
        {
            GameObject buttonObj = Instantiate(skillButtonPrefab, skillListContainer);
            Button btn = buttonObj.GetComponent<Button>();

            // Setup visual
            Image icon = buttonObj.GetComponentInChildren<Image>();
            if (icon != null && skill.icon != null)
                icon.sprite = skill.icon;

            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = skill.skillName;

            // Setup click
            SkillSO skillRef = skill; // Capture for closure
            btn.onClick.AddListener(() => OnSkillSelected(skillRef));

            skillButtons.Add(btn);
        }

        // Button "Không có (Remove)"
        //GameObject noneButtonObj = Instantiate(skillButtonPrefab, skillListContainer);
        //Button noneBtn = noneButtonObj.GetComponent<Button>();
        //TextMeshProUGUI noneText = noneButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        //if (noneText != null)
        //    noneText.text = "[ Không có ]";

        //noneBtn.onClick.AddListener(() => OnSkillSelected(null));
        //skillButtons.Add(noneBtn);
    }

    private void OnSkillSelected(SkillSO skill)
    {
        if (selectedSlotIndex < 0)
        {
            Debug.LogWarning("[SkillSelectionPanel] Không có slot được chọn!");
            return;
        }

        if (skill == null)
        {
            skillSlotManager.RemoveSkillFromSlot(selectedSlotIndex);
            Debug.Log($"[SkillSelectionPanel] Đã xóa skill từ slot {selectedSlotIndex}");
        }
        else
        {
            skillSlotManager.AssignSkillToSlot(selectedSlotIndex, skill);
            Debug.Log($"[SkillSelectionPanel] Gán '{skill.skillName}' vào slot {selectedSlotIndex}");
        }

        ClosePanel();
    }

    public void ClosePanel()
    {
        selectedSlotIndex = -1;
        gameObject.SetActive(false);
        Debug.Log("[SkillSelectionPanel] Panel đóng");
    }

    private void OnDestroy()
    {
        if (closeButton != null)
            closeButton.onClick.RemoveListener(ClosePanel);
    }
}
