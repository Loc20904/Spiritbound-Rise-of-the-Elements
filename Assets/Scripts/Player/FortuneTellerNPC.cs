using System.Collections;
using UnityEngine;

public class FortuneTellerNPC : MonoBehaviour
{
    [Header("Dialogue Sequences")]
    [Tooltip("Dialogue played BEFORE revealing the stat (intro speech)")]
    public DialogueSequence introDialogue;

    [Tooltip("Dialogue played AFTER stat is assigned (will have dynamic text injected)")]
    public DialogueSequence revealDialogueTemplate;

    [Tooltip("Dialogue played when player already has a stat (reminder)")]
    public DialogueSequence reminderDialogueTemplate;

    [Header("UI Reference")]
    [Tooltip("Optional: PlayerStatUI to show the stat panel after reveal")]
    public PlayerStatUI statUI;

    private bool isInteracting = false;

#if UNITY_EDITOR
    private void Awake()
    {
        PlayerPrefs.DeleteKey("PlayerStatIndex");
    }
#endif

    /// <summary>
    /// Called when the player presses the interact key near this NPC.
    /// </summary>
    public void TriggerInteraction()
    {
        if (isInteracting) return;
        StartCoroutine(InteractionRoutine());
    }

    private IEnumerator InteractionRoutine()
    {
        isInteracting = true;

        if (PlayerStatManager.Instance == null)
        {
            Debug.LogError("[FortuneTellerNPC] PlayerStatManager.Instance is null!");
            isInteracting = false;
            yield break;
        }

        if (!PlayerStatManager.Instance.HasReceivedStat)
        {
            // === FIRST TIME: Reveal new stat ===
            yield return StartCoroutine(FirstTimeRevealRoutine());
        }
        else
        {
            // === ALREADY HAS STAT: Remind player ===
            yield return StartCoroutine(ReminderRoutine());
        }

        isInteracting = false;
    }

    /// <summary>
    /// First-time interaction: intro dialogue → random stat → reveal dialogue → UI panel.
    /// </summary>
    private IEnumerator FirstTimeRevealRoutine()
    {
        // 1. Play intro dialogue
        if (introDialogue != null && DialogueUIManager.Instance != null)
        {
            yield return StartCoroutine(
                DialogueUIManager.Instance.PlayDialogueRoutine(introDialogue)
            );
        }

        // 2. Assign random stat
        PlayerStatProfile assignedStat = PlayerStatManager.Instance.AssignRandomStat();

        if (assignedStat == null)
        {
            Debug.LogError("[FortuneTellerNPC] Failed to assign stat!");
            yield break;
        }

        //// 3. Play reveal dialogue with dynamic stat info
        //if (revealDialogueTemplate != null && DialogueUIManager.Instance != null)
        //{
        //    // Create a temporary dialogue sequence with stat info injected
        //    DialogueSequence dynamicReveal = CreateDynamicDialogue(revealDialogueTemplate, assignedStat);
        //    yield return StartCoroutine(
        //        DialogueUIManager.Instance.PlayDialogueRoutine(dynamicReveal)
        //    );
        //}

        // 4. Show UI panel
        if (statUI != null)
        {
            statUI.ShowStat(assignedStat);
        }

        Debug.Log($"[FortuneTellerNPC] Revealed: {assignedStat.titleEmoji} {assignedStat.cardName}");
    }

    /// <summary>
    /// Reminder interaction: just show the existing stat info.
    /// </summary>
    private IEnumerator ReminderRoutine()
    {
        PlayerStatProfile currentStat = PlayerStatManager.Instance.ActiveStat;

        if (reminderDialogueTemplate != null && DialogueUIManager.Instance != null)
        {
            DialogueSequence dynamicReminder = CreateDynamicDialogue(reminderDialogueTemplate, currentStat);
            yield return StartCoroutine(
                DialogueUIManager.Instance.PlayDialogueRoutine(dynamicReminder)
            );
        }

        // Show UI panel again
        if (statUI != null)
        {
            statUI.ShowStat(currentStat);
        }

        Debug.Log($"[FortuneTellerNPC] Reminded: {currentStat.titleEmoji} {currentStat.cardName}");
    }

    /// <summary>
    /// Creates a copy of a DialogueSequence with placeholder text replaced by stat info.
    /// Placeholders: {TITLE}, {EMOJI}, {STAT_NAME}, {EFFECT}, {TYPE}
    /// </summary>
    private DialogueSequence CreateDynamicDialogue(DialogueSequence template, PlayerStatProfile stat)
    {
        // Create a runtime copy so we don't modify the original ScriptableObject
        DialogueSequence copy = ScriptableObject.CreateInstance<DialogueSequence>();
        copy.requiredFlag = template.requiredFlag;
        copy.lines = new DialogueLine[template.lines.Length];

        for (int i = 0; i < template.lines.Length; i++)
        {
            copy.lines[i] = new DialogueLine
            {
                characterName = template.lines[i].characterName,
                characterAvatar = template.lines[i].characterAvatar,
                sentence = ReplacePlaceholders(template.lines[i].sentence, stat)
            };
        }

        return copy;
    }

    private string ReplacePlaceholders(string text, PlayerStatProfile stat)
    {
        return text
            .Replace("{TITLE}", stat.cardName)       // Fallback for old templates
            .Replace("{STAT_NAME}", stat.cardName)   // Fallback for old templates
            .Replace("{NUMERAL}", stat.cardNumeral)
            .Replace("{CARD_NAME}", stat.cardName)
            .Replace("{EMOJI}", stat.titleEmoji)
            .Replace("{EFFECT}", stat.GetEffectSummary())
            .Replace("{TYPE}", stat.statType.ToString());
    }

    // Thêm vào cuối class FortuneTellerNPC, trước dấu } cuối cùng:

    private bool playerInRange = false;

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            TriggerInteraction();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }

}
