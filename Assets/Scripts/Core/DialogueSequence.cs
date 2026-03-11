using UnityEngine;


// 2. Cấu trúc của một đoạn hội thoại (Gồm nhiều câu)
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Story/Dialogue Sequence")]
public class DialogueSequence : ScriptableObject
{
    [Header("Điều kiện để đoạn thoại này xuất hiện")]
    [Tooltip("Để trống nếu đây là thoại mặc định. Nhập tên cờ (VD: Boss1_Spared) nếu cần điều kiện.")]
    public string requiredFlag;

    [Header("Nội dung thoại")]
    public DialogueLine[] lines;
}

