using System.Collections;
using UnityEngine;

/// <summary>
/// Heal Skill - Phục hồi máu
/// </summary>
[CreateAssetMenu(menuName = "Skill System/Active Skills/Heal")]
public class HealSkill : SkillSO
{
    [SerializeField] private float healAmount = 30f;
    [SerializeField] private GameObject healEffectPrefab;

    public override IEnumerator Activate(GameObject player)
    {
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth == null)
        {
            Debug.LogError("HealSkill: PlayerHealth not found!");
            return null;
        }

        // Phục hồi máu
        playerHealth.Heal(healAmount);

        // Hiệu ứng heal
        if (healEffectPrefab != null)
        {
            Instantiate(healEffectPrefab, player.transform.position, Quaternion.identity);
        }

        Debug.Log($"Heal activated! Restored {healAmount} HP");
        return null;
    }
}
