using UnityEngine;

[CreateAssetMenu(menuName = "Skill System/Conditions/Level Condition")]
public class LevelConditionSO : UnlockConditionSO
{
    public int requiredLevel;

    public override bool IsMet(PlayerStats stats)
    {
        //return stats.currentLevel >= requiredLevel;
        return true;
    }
}