using UnityEngine;

// 1. Cấu trúc của một câu nói đơn lẻ
[System.Serializable]
public class DialogueLine
{
    public string characterName; // Tên nhân vật
    public Sprite characterAvatar; // Hình đại diện (Avatar)
    [TextArea(3, 10)]
    public string sentence; // Nội dung câu nói (TextArea giúp khung gõ chữ to ra dễ nhìn)
}