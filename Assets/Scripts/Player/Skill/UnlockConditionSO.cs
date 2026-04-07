using UnityEngine;

// Lớp cơ sở cho MỌI điều kiện mở khóa
public abstract class UnlockConditionSO : ScriptableObject
{
    // Cần truyền PlayerStats (hoặc class tương đương chứa dữ liệu người chơi) vào để kiểm tra
    public abstract bool IsMet(PlayerStats stats);
}