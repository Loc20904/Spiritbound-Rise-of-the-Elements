using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Quản lý 4 skill slots (U, I, O, P) cho Active Skills
/// Người chơi có thể gán/bỏ gán skills từ inventory
/// </summary>
public class SkillSlotManager : MonoBehaviour
{
    [System.Serializable]
    public class SkillSlot
    {
        public KeyCode hotkey;
        public SkillSO assignedSkill;
        public float cooldownRemaining = 0f;

        public SkillSlot(KeyCode key)
        {
            hotkey = key;
            assignedSkill = null;
            cooldownRemaining = 0f;
        }

        public bool CanUse => assignedSkill != null && cooldownRemaining <= 0f;

        public void UseCooldown()
        {
            if (assignedSkill != null)
                cooldownRemaining = assignedSkill.cooldown;
        }

        public void UpdateCooldown(float deltaTime)
        {
            if (cooldownRemaining > 0f)
                cooldownRemaining -= deltaTime;
        }
    }

    [SerializeField] private InputActionAsset actions;
    private InputAction skillSlotUAction, skillSlotIAction, skillSlotOAction, skillSlotPAction;

    private SkillSlot[] skillSlots = new SkillSlot[4];
    private PlayerController playerController;
    private GameObject projectileSpawnPoint;

    // Armazenar callbacks para unsubscribe corretamente
    private System.Action<InputAction.CallbackContext> onSkillUCallback;
    private System.Action<InputAction.CallbackContext> onSkillICallback;
    private System.Action<InputAction.CallbackContext> onSkillOCallback;
    private System.Action<InputAction.CallbackContext> onSkillPCallback;
    public event Action<int> OnSkillUsed; // Event khi skill được sử dụng
    public event Action<int> OnSkillSlotChanged;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        InitializeSkillSlots();
    }

    private void InitializeSkillSlots()
    {
        skillSlots[0] = new SkillSlot(KeyCode.U);
        skillSlots[1] = new SkillSlot(KeyCode.I);
        skillSlots[2] = new SkillSlot(KeyCode.O);
        skillSlots[3] = new SkillSlot(KeyCode.P);
    }

    private void OnEnable()
    {
        if (actions == null)
        {
            Debug.LogError("SkillSlotManager: Chưa gán InputActionAsset!");
            return;
        }

        var playerActionMap = actions.FindActionMap("Player");
        if (playerActionMap == null)
        {
            Debug.LogError("SkillSlotManager: Không tìm thấy Action Map 'Player'");
            return;
        }

        // Bind các action U, I, O, P
        skillSlotUAction = playerActionMap.FindAction("Skill_U");
        skillSlotIAction = playerActionMap.FindAction("Skill_I");
        skillSlotOAction = playerActionMap.FindAction("Skill_O");
        skillSlotPAction = playerActionMap.FindAction("Skill_P");

        // Tạo và lưu callbacks
        onSkillUCallback = ctx => TryUseSkill(0);
        onSkillICallback = ctx => TryUseSkill(1);
        onSkillOCallback = ctx => TryUseSkill(2);
        onSkillPCallback = ctx => TryUseSkill(3);

        if (skillSlotUAction != null)
        {
            skillSlotUAction.Enable();
            skillSlotUAction.performed += onSkillUCallback;
            Debug.Log("[SkillSlotManager] Skill_U action habilitada");
        }
        else
            Debug.LogWarning("[SkillSlotManager] Skill_U action não encontrada!");

        if (skillSlotIAction != null)
        {
            skillSlotIAction.Enable();
            skillSlotIAction.performed += onSkillICallback;
            Debug.Log("[SkillSlotManager] Skill_I action habilitada");
        }
        else
            Debug.LogWarning("[SkillSlotManager] Skill_I action não encontrada!");

        if (skillSlotOAction != null)
        {
            skillSlotOAction.Enable();
            skillSlotOAction.performed += onSkillOCallback;
            Debug.Log("[SkillSlotManager] Skill_O action habilitada");
        }
        else
            Debug.LogWarning("[SkillSlotManager] Skill_O action não encontrada!");

        if (skillSlotPAction != null)
        {
            skillSlotPAction.Enable();
            skillSlotPAction.performed += onSkillPCallback;
            Debug.Log("[SkillSlotManager] Skill_P action habilitada");
        }
        else
            Debug.LogWarning("[SkillSlotManager] Skill_P action não encontrada!");
    }

    private void OnDisable()
    {
        if (skillSlotUAction != null && onSkillUCallback != null)
        {
            skillSlotUAction.performed -= onSkillUCallback;
            skillSlotUAction.Disable();
        }
        if (skillSlotIAction != null && onSkillICallback != null)
        {
            skillSlotIAction.performed -= onSkillICallback;
            skillSlotIAction.Disable();
        }
        if (skillSlotOAction != null && onSkillOCallback != null)
        {
            skillSlotOAction.performed -= onSkillOCallback;
            skillSlotOAction.Disable();
        }
        if (skillSlotPAction != null && onSkillPCallback != null)
        {
            skillSlotPAction.performed -= onSkillPCallback;
            skillSlotPAction.Disable();
        }
    }

    private void Update()
    {
        // Update cooldown cho mỗi slot
        for (int i = 0; i < skillSlots.Length; i++)
        {
            skillSlots[i].UpdateCooldown(Time.deltaTime);
        }
    }

    public void AssignSkillToSlot(int slotIndex, SkillSO skill)
    {
        if (slotIndex < 0 || slotIndex >= skillSlots.Length)
        {
            Debug.LogWarning($"AssignSkillToSlot: Slot {slotIndex} không hợp lệ!");
            return;
        }

        if (skill != null && skill.type != SkillType.Active)
        {
            Debug.LogWarning($"AssignSkillToSlot: {skill.skillName} không phải Active Skill!");
            return;
        }

        skillSlots[slotIndex].assignedSkill = skill;
        skillSlots[slotIndex].cooldownRemaining = 0f;
        OnSkillSlotChanged?.Invoke(slotIndex);
        Debug.Log($"Gán skill '{skill?.skillName ?? "None"}' vào slot {slotIndex} ({skillSlots[slotIndex].hotkey})");
    }

    public void RemoveSkillFromSlot(int slotIndex)
    {
        AssignSkillToSlot(slotIndex, null);
    }

    private void TryUseSkill(int slotIndex)
    {
        Debug.Log($"[SkillSlotManager] TryUseSkill called for slot {slotIndex}");

        if (playerController == null)
        {
            Debug.LogError("[SkillSlotManager] PlayerController é null!");
            return;
        }

        if (playerController.IsDead)
        {
            Debug.Log($"[SkillSlotManager] Player está morto!");
            return;
        }

        if (playerController.IsDashing)
        {
            Debug.Log($"[SkillSlotManager] Player está em dash!");
            return;
        }

        SkillSlot slot = skillSlots[slotIndex];

        if (!slot.CanUse)
        {
            Debug.Log($"[SkillSlotManager] Slot {slotIndex} em cooldown: {slot.cooldownRemaining:F2}s / {slot.assignedSkill?.cooldown ?? 0:F2}s");
            return;
        }

        if (slot.assignedSkill == null)
        {
            Debug.Log($"[SkillSlotManager] Slot {slotIndex} vazio - nenhum skill atribuído!");
            return;
        }

        // Activate skill
        Debug.Log($"[SkillSlotManager] ✓ Ativando skill '{slot.assignedSkill.skillName}' no slot {slotIndex}!");
        slot.assignedSkill.Activate(gameObject);
        slot.UseCooldown();
        OnSkillUsed?.Invoke(slotIndex);
        Debug.Log($"Sử dụng skill: {slot.assignedSkill.skillName}");
    }

    public SkillSlot GetSlot(int index) => index >= 0 && index < skillSlots.Length ? skillSlots[index] : null;
    public SkillSlot[] GetAllSlots() => skillSlots;

    // Getter for PlayerController state
    public bool IsSkillSlotReady(int slotIndex)
        => slotIndex >= 0 && slotIndex < skillSlots.Length && skillSlots[slotIndex].CanUse;
}
