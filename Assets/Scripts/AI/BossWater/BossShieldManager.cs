using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossShieldManager : MonoBehaviour
{
    [Header("Dependencies")]
    public BossHealth bossHealth;       // Kéo Boss vào đây
    public List<BossPillar> pillars;    // Kéo 2 cái Trụ vào đây

    [Header("Settings")]
    public float respawnTime = 10f;     // Thời gian hồi trụ (10s)

    private bool isRespawning = false;

    void Start()
    {
        // Đăng ký sự kiện: Mỗi khi có trụ vỡ, gọi hàm CheckPillars
        foreach (var pillar in pillars)
        {
            pillar.OnPillarBroken += CheckAllPillars;
        }

        // Mới vào game bật khiên luôn
        ActivateShield();
    }

    void CheckAllPillars()
    {
        // Kiểm tra xem CÓ TRỤ NÀO CÒN SỐNG KHÔNG?
        bool anyPillarAlive = false;
        foreach (var pillar in pillars)
        {
            if (!pillar.IsBroken)
            {
                anyPillarAlive = true;
                break;
            }
        }

        if (!anyPillarAlive)
        {
            // Nếu KHÔNG còn trụ nào sống -> Boss mất khiên
            DeactivateShield();
        }
    }

    void ActivateShield()
    {
        if (bossHealth) bossHealth.SetInvulnerable(true);
        Debug.Log("SHIELD ON: Boss Bất Tử!");
    }

    void DeactivateShield()
    {
        if (bossHealth) bossHealth.SetInvulnerable(false);
        Debug.Log("SHIELD OFF: Boss Nhận Sát Thương!");

        // Bắt đầu đếm ngược hồi sinh
        if (!isRespawning)
        {
            StartCoroutine(RespawnRoutine());
        }
    }

    IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        // Chờ 10 giây (Trong thời gian này Player tranh thủ đánh Boss)
        yield return new WaitForSeconds(respawnTime);

        // Hồi sinh tất cả trụ
        foreach (var pillar in pillars)
        {
            pillar.Revive();
        }

        // Bật lại khiên cho Boss
        ActivateShield();

        isRespawning = false;
    }
}