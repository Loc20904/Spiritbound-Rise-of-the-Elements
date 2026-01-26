using UnityEngine;

public class EnemyAttackEvents : MonoBehaviour
{
    public EnemyAttackHitbox hitbox;

    public void HitboxOn() => hitbox.HitboxOn();
    public void HitboxOff() => hitbox.HitboxOff();
}
