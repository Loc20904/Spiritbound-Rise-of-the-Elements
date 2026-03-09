using UnityEngine;

public class PlayerAttackEvents : MonoBehaviour
{
    public PlayerAttackHitbox hitbox;

    public void HitboxOn() => hitbox.HitboxOn();
    public void HitboxOff() => hitbox.HitboxOff();
}
