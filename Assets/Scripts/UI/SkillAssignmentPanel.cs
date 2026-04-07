using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI Panel para gerenciar habilidades - permite arrastar/soltar ou clicar para atribuir skills aos slots
/// </summary>
public class SkillAssignmentPanel : MonoBehaviour
{
    [SerializeField] private GameObject skillButtonPrefab;
    [SerializeField] private Transform skillListContainer;
    [SerializeField] private TextMeshProUGUI instructionText;
    
    private SkillSlotManager skillSlotManager;
    private SkillManager skillManager;
    
    private int selectedSlotIndex = -1; // Slot que usuário está tentando preencher
    private List<Button> skillButtons = new List<Button>();

    private void Start()
    {
        skillSlotManager = FindObjectOfType<SkillSlotManager>();
        skillManager = FindObjectOfType<SkillManager>();
        
        if (skillSlotManager == null || skillManager == null)
        {
            Debug.LogError("SkillAssignmentPanel: Não encontrou SkillSlotManager ou SkillManager!");
            gameObject.SetActive(false);
            return;
        }

        // Se houver, bind com eventos
        skillSlotManager.OnSkillSlotChanged += RefreshSkillList;
        skillManager.OnSkillUnlocked += OnSkillUnlockedHandler;
        
        RefreshSkillList(-1);
    }

    private void OnDestroy()
    {
        if (skillSlotManager != null)
            skillSlotManager.OnSkillSlotChanged -= RefreshSkillList;
        if (skillManager != null)
            skillManager.OnSkillUnlocked -= OnSkillUnlockedHandler;
    }

    private void OnSkillUnlockedHandler(SkillSO skill)
    {
        if (skill.type == SkillType.Active)
        {
            RefreshSkillList(-1);
        }
    }

    /// <summary>
    /// Chamado pelo botão de slot (U/I/O/P) para iniciar seleção de skill
    /// </summary>
    public void SelectSlotForAssignment(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        instructionText.text = $"Escolha uma habilidade para o slot {slotIndex} ({new[] { "U", "I", "O", "P" }[slotIndex]})";
        RefreshSkillList(slotIndex);
    }

    /// <summary>
    /// Atualiza lista de skills disponíveis para seleção
    /// </summary>
    private void RefreshSkillList(int slotIndex)
    {
        // Limpar buttons antigos
        foreach (var btn in skillButtons)
        {
            Destroy(btn.gameObject);
        }
        skillButtons.Clear();

        // Se nenhum slot selecionado, mostrar todas as unlocked skills
        List<SkillSO> availableSkills = skillManager.GetUnlockedActiveSkills();

        if (availableSkills.Count == 0)
        {
            instructionText.text = "Nenhuma habilidade ativa desbloqueada ainda...";
            return;
        }

        foreach (var skill in availableSkills)
        {
            GameObject buttonObj = Instantiate(skillButtonPrefab, skillListContainer);
            Button btn = buttonObj.GetComponent<Button>();
            
            // Configurar visual do botão
            Image icon = buttonObj.GetComponentInChildren<Image>();
            if (icon != null && skill.icon != null)
                icon.sprite = skill.icon;

            TextMeshProUGUI text = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                text.text = skill.skillName;

            // Bind clique para atribuir skill ao slot
            btn.onClick.AddListener(() => OnSkillSelected(skill));
            
            skillButtons.Add(btn);
        }

        // Adicionar botão para remover skill (mostrar None)
        GameObject removeButtonObj = Instantiate(skillButtonPrefab, skillListContainer);
        Button removeBtn = removeButtonObj.GetComponent<Button>();
        TextMeshProUGUI removeText = removeButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (removeText != null)
            removeText.text = "[ Remover ]";
        removeBtn.onClick.AddListener(() => OnSkillSelected(null));
        skillButtons.Add(removeBtn);
    }

    /// <summary>
    /// Chamado quando usuário clica em uma skill para gená-la ao slot
    /// </summary>
    private void OnSkillSelected(SkillSO skill)
    {
        if (selectedSlotIndex < 0)
        {
            Debug.Log("[SkillAssignmentPanel] Nenhum slot selecionado!");
            return;
        }

        if (skill == null)
        {
            skillSlotManager.RemoveSkillFromSlot(selectedSlotIndex);
            instructionText.text = $"Removido skill do slot {selectedSlotIndex}";
        }
        else
        {
            skillSlotManager.AssignSkillToSlot(selectedSlotIndex, skill);
            instructionText.text = $"'{skill.skillName}' atribuído ao slot {selectedSlotIndex}";
        }

        selectedSlotIndex = -1;
        RefreshSkillList(-1);
    }

    /// <summary>
    /// Fecha painel de atribuição
    /// </summary>
    public void ClosePanel()
    {
        selectedSlotIndex = -1;
        gameObject.SetActive(false);
    }
}
