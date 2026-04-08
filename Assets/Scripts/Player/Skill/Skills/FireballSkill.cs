using System.Collections;
using UnityEngine;

/// <summary>
/// FireBall Skill - Bắn tia lửa về phía trước
/// </summary>
[CreateAssetMenu(menuName = "Skill System/Active Skills/Fireball")]
public class FireballSkill : SkillSO
{
    [Header("Dragon Settings")]
    [SerializeField] private GameObject dragonPrefab; // Khối chưởng rồng khổng lồ
    [SerializeField] private float dragonSpeed = 18f; // Rồng bay rất nhanh
    [SerializeField] private float castTime = 1f;   // Thời gian tụ lực trước khi bắn

    [Header("Effects")]
    [SerializeField] private GameObject VFXCharging;   // Hiệu ứng tụ năng lượng (cháy quanh người)
    [SerializeField] private AudioClip soundCharging;  // Tiếng tụ khí
    [SerializeField] private AudioClip soundDragonRoar; // Tiếng rồng gầm khi bắn

    public override IEnumerator Activate(GameObject player)
    {
        if (dragonPrefab == null)
        {
            Debug.LogError("FireDragonAttack: dragonPrefab chưa được gán!");
            yield break;
        }

        PlayerController controller = player.GetComponent<PlayerController>();

        // --- GIAI ĐOẠN 1: TỤ LỰC ---
        Debug.Log("[Fire Dragon] Đang tụ năng lượng...");

        if (soundCharging != null) SFXPool.Instance.Play(soundCharging, volume: 0.8f, pitch: 1f);

        GameObject chargingVFX = null;
        if (VFXCharging != null)
        {
            chargingVFX = Instantiate(VFXCharging, player.transform.position + new Vector3(-0.1f, 1f, 0), Quaternion.identity, player.transform);
        }

        // Có thể thêm code ép Player đứng im ở đây: controller.SetMovement(false);

        // Chờ tụ lực xong (Ví dụ: 1.5 giây)
        yield return new WaitForSeconds(castTime);


        // --- GIAI ĐOẠN 2: LONG GẦM (BẮN) ---
        Debug.Log("[Fire Dragon] BẮN!!");

        // Hủy hiệu ứng tụ lực
        if (chargingVFX != null) Destroy(chargingVFX);

        if (soundDragonRoar != null) SFXPool.Instance.Play(soundDragonRoar, volume: 0.5f, pitch: 1f);
        yield return new WaitForSeconds(0.2f);
        Transform spawnPoint = player.transform.Find("SpawnPoint") ?? player.transform;
        float direction = controller?.GetFacingDirection() ?? 1f;

        // Đẻ ra chưởng rồng
        GameObject dragon = Instantiate(dragonPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = dragon.GetComponent<Rigidbody2D>();

        // Xoay chiều con rồng
        if (direction < 0)
        {
            Vector3 scale = dragon.transform.localScale;
            scale.x *= -1;
            dragon.transform.localScale = scale;
        }

        // Tốc độ bay
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * dragonSpeed, 0f);
        }

        // Trả lại quyền di chuyển cho Player ở đây: controller.SetMovement(true);
    }
}