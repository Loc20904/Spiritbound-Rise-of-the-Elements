using UnityEngine;

/// <summary>
/// IceSpike Skill - Triệu hồi những cây băng từ đất
/// </summary>
[CreateAssetMenu(menuName = "Skill System/Active Skills/Ice Spike")]
public class IceSpikeSkill : SkillSO
{
    [SerializeField] private GameObject iceSpikePrefab;
    [SerializeField] private int spikeCount = 3;
    [SerializeField] private float spawnRadius = 2f;

    public override void Activate(GameObject player)
    {
        if (iceSpikePrefab == null)
        {
            Debug.LogError("IceSpikeSkill: iceSpikePrefab chưa được gán!");
            return;
        }

        Vector3 playerPos = player.transform.position;

        // Triệu hồi nhiều ice spike xung quanh player
        for (int i = 0; i < spikeCount; i++)
        {
            float angle = (360f / spikeCount) * i;
            float x = playerPos.x + Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius;
            float y = playerPos.y + Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius;

            Instantiate(iceSpikePrefab, new Vector3(x, y, 0), Quaternion.identity);
        }

        Debug.Log($"Ice Spikes activated: {spikeCount} spikes spawned around {playerPos}");
    }
}
