using UnityEngine;

namespace Assets.Scripts.Player.Skill
{
    [CreateAssetMenu(menuName = "Skill System/Conditions/Distance Condition")]
    public class DistanceConditionSO : UnlockConditionSO
    {

        public int distanceThreshold = 10;

        public override bool IsMet(PlayerStats stats)
        {
            if (stats.getSpeed() > distanceThreshold)
            {
                return true;
            }
            return false;
        }
    }
}
