using System.Collections;
using UnityEngine;

/// <summary>
/// FireBall Skill - Bắn tia lửa về phía trước
/// </summary>
[CreateAssetMenu(menuName = "Skill System/Active Skills/Fireball")]
public class FireballSkill : SkillSO
{
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float fireballSpeed = 15f;
    [SerializeField] private AudioClip soundEffect;
    [SerializeField] private AudioClip soundFireEffect;

    // ❌ Xóa dòng này đi, không lưu Transform trong SO nữa
    // [SerializeField] private Transform spawnPoint; 

    public override IEnumerator Activate(GameObject player)
    {
        if (fireballPrefab == null)
        {
            Debug.LogError("FireballSkill: fireballPrefab chưa được gán!");
            return null;
        }

        // ✅ TÌM SPAWN POINT TỪ PLAYER
        // Tìm một vật thể con tên là "SpawnPoint" nằm trong Player
        Transform spawnPoint = player.transform.Find("SpawnPoint");

        // Nếu quên chưa tạo SpawnPoint thì lấy luôn vị trí của Player làm gốc
        if (spawnPoint == null)
        {
            Debug.LogWarning("Không tìm thấy vật thể con tên 'SpawnPoint' trong Player, bắn từ tâm Player!");
            spawnPoint = player.transform;
        }

        // Tạo fireball tại vị trí vừa tìm được
        GameObject fireball = Instantiate(fireballPrefab, spawnPoint.position, Quaternion.identity);
        Rigidbody2D rb = fireball.GetComponent<Rigidbody2D>();

        // Xác định hướng bắn dựa vào direction của player
        float direction = player.GetComponent<PlayerController>()?.GetFacingDirection() ?? 1f;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(direction * fireballSpeed, 0f);
        }

        Debug.Log($"Fireball activated at {spawnPoint.position}, direction: {direction}");
        return null;
    }
}