using UnityEngine;

/// <summary>
/// DashStrike Skill - Lao tới và đánh một cú mạnh
/// </summary>
[CreateAssetMenu(menuName = "Skill System/Active Skills/Dash Strike")]
public class DashStrikeSkill : SkillSO
{
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float strikeRadius = 1f;
    [SerializeField] private float strikeDamage = 50f;

    private bool isDashing = false;
    private float dashTimer = 0f;

    public override void Activate(GameObject player)
    {
        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError("DashStrikeSkill: PlayerController not found!");
            return;
        }

        // Thực hiện dash strike
        Debug.Log($"Dash Strike activated!");
        // Bạn có thể gọi một coroutine hoặc phương thức trong PlayerController để xử lý dash strike
        // Hoặc có thể bỏ logic vào đây
    }
}
