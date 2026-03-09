using UnityEngine;

public class NPCInteractable : MonoBehaviour
{
    [Header("Danh sách kịch bản (Ưu tiên từ trên xuống dưới)")]
    // Bạn kéo các file DialogueSequence vừa tạo vào đây. 
    // Những kịch bản có requiredFlag khó/đặc biệt nên để lên đầu (index 0).
    public DialogueSequence[] possibleDialogues;

    // Hàm này được gọi khi Player ấn nút tương tác (VD: phím F) cạnh NPC
    public void TriggerDialogue()
    {
        DialogueSequence dialogueToPlay = null;

        // Tìm kịch bản phù hợp nhất
        foreach (var dialogue in possibleDialogues)
        {
            // Nếu đoạn thoại này không yêu cầu cờ (mặc định), hoặc cờ yêu cầu đang là TRUE
            if (string.IsNullOrEmpty(dialogue.requiredFlag) ||
                GameProgressManager.Instance.GetFlag(dialogue.requiredFlag))
            {
                dialogueToPlay = dialogue;
                break; // Tìm thấy cái hợp lý nhất là dừng lại luôn
            }
        }

        if (dialogueToPlay != null)
        {
            // Đưa kịch bản này cho UI hiển thị lên màn hình
            DialogueUIManager.Instance.PlayDialogueRoutine(dialogueToPlay);
            Debug.Log($"Bắt đầu đọc kịch bản: {dialogueToPlay.name}");
        }
    }
}