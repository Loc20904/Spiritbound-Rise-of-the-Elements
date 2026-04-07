using UnityEngine;

namespace Assets.Scripts.Player.Skill
{
    [CreateAssetMenu(menuName = "Skill System/Conditions/Damge Condition")]
    public class DamageConditionSO : UnlockConditionSO
    {
        public int requiredDamage = 100;
        public override bool IsMet(PlayerStats stats)
        {
            if (stats.Damage > requiredDamage)
            {
                return true;
            }
            return false;
        }
    }
}
