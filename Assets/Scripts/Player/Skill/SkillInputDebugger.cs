using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Debug script para testar se InputActions estão funcionando
/// Adiciona ao Player para troubleshoot
/// </summary>
public class SkillInputDebugger : MonoBehaviour
{
    [SerializeField] private InputActionAsset actions;
    private InputAction skillUAction, skillIAction, skillOAction, skillPAction;
    private SkillSlotManager skillSlotManager;

    private void OnEnable()
    {
        if (actions == null)
        {
            Debug.LogError("[SkillInputDebugger] InputActionAsset não assignado!");
            return;
        }

        var playerActionMap = actions.FindActionMap("Player");
        if (playerActionMap == null)
        {
            Debug.LogError("[SkillInputDebugger] Action Map 'Player' não encontrado!");
            return;
        }

        skillUAction = playerActionMap.FindAction("Skill_U");
        skillIAction = playerActionMap.FindAction("Skill_I");
        skillOAction = playerActionMap.FindAction("Skill_O");
        skillPAction = playerActionMap.FindAction("Skill_P");

        if (skillUAction == null) Debug.LogError("[SkillInputDebugger] Skill_U action não encontrada!");
        if (skillIAction == null) Debug.LogError("[SkillInputDebugger] Skill_I action não encontrada!");
        if (skillOAction == null) Debug.LogError("[SkillInputDebugger] Skill_O action não encontrada!");
        if (skillPAction == null) Debug.LogError("[SkillInputDebugger] Skill_P action não encontrada!");

        skillSlotManager = GetComponent<SkillSlotManager>();

        // Habilitar e ligar listeners
        if (skillUAction != null)
        {
            skillUAction.Enable();
            skillUAction.performed += ctx => DebugSkillInput("U", 0);
        }
        if (skillIAction != null)
        {
            skillIAction.Enable();
            skillIAction.performed += ctx => DebugSkillInput("I", 1);
        }
        if (skillOAction != null)
        {
            skillOAction.Enable();
            skillOAction.performed += ctx => DebugSkillInput("O", 2);
        }
        if (skillPAction != null)
        {
            skillPAction.Enable();
            skillPAction.performed += ctx => DebugSkillInput("P", 3);
        }

        Debug.Log("[SkillInputDebugger] Skills input listeners attached - ready to test!");
    }

    private void DebugSkillInput(string key, int slotIndex)
    {
        Debug.Log($"[SkillInputDebugger] Key {key} pressed! (Slot {slotIndex})");
        
        if (skillSlotManager != null)
        {
            var slot = skillSlotManager.GetSlot(slotIndex);
            if (slot != null)
            {
                if (slot.assignedSkill != null)
                    Debug.Log($"  → Skill: {slot.assignedSkill.skillName}, Cooldown: {slot.cooldownRemaining:F2}s / {slot.assignedSkill.cooldown}s");
                else
                    Debug.Log($"  → Slot vazio!");
            }
        }
    }

    private void OnDisable()
    {
        if (skillUAction != null) skillUAction.Disable();
        if (skillIAction != null) skillIAction.Disable();
        if (skillOAction != null) skillOAction.Disable();
        if (skillPAction != null) skillPAction.Disable();
    }
}
