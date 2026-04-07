using UnityEngine;

/// <summary>
/// SlashProjectile Skill (SkillK) - Phiên bản managed
/// Bắn một tia kiếm khí về phía trước
/// </summary>
[CreateAssetMenu(menuName = "Skill System/Active Skills/Slash Projectile")]
public class SlashProjectileSkill : SkillSO
{
    [SerializeField] private GameObject slashProjectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private int slashDamage = 2;

    public override void Activate(GameObject player)
    {
        if (slashProjectilePrefab == null || projectileSpawnPoint == null)
        {
            Debug.LogError("SlashProjectileSkill: Prefab hoặc spawn point chưa được gán!");
            return;
        }

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null)
        {
            Debug.LogError("SlashProjectileSkill: PlayerController not found!");
            return;
        }

        // Tạo kiếm khí projectile
        GameObject projectile = Instantiate(slashProjectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        // Lấy facing direction từ player
        float facingDirection = pc.GetFacingDirection();

        // Init projectile với damage và direction
        //Projectile proj = projectile.GetComponent<Projectile>();
        //if (proj != null)
        //{
        //    proj.Init(facingDirection, slashDamage);
        //}

        Debug.Log($"SlashProjectileSkill activated! Damage: {slashDamage}, Direction: {facingDirection}");
    }
}
