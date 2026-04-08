using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Player.Skill.Skills
{
    [CreateAssetMenu(menuName = "Skill System/Active Skills/Kaioken")]
    public class KaiokenSkill : SkillSO
    {
        [Header("Kaioken Effects")]
        [SerializeField] private AudioClip SFXKaioken;
        [SerializeField] private GameObject VFXKaiokenPrefab; // Chứa Prefab hiệu ứng lửa đỏ đỏ

        [Tooltip("Phần trăm máu tối đa sẽ bị trừ (0.25 = 25%)")]
        [SerializeField] private float hpCostPercent = 0.25f;

        [Tooltip("Lượng sát thương được cộng thêm trong thời gian Kaioken")]
        [SerializeField] private int bonusDamage = 50;

        [Tooltip("Thời gian duy trì trạng thái Kaioken")]
        [SerializeField] private float duration = 7f;

        // Lưu ý: Vì Activate giờ trả về IEnumerator, bạn phải dùng StartCoroutine() ở class gọi nó
        public override IEnumerator Activate(GameObject player)
        {
            // 1. Lấy thông tin máu và chỉ số của Player
            PlayerHealth health = player.GetComponent<PlayerHealth>();
            PlayerStats stats = player.GetComponent<PlayerStats>();

            if (health == null || stats == null)
            {
                Debug.LogError("Không tìm thấy PlayerHealth hoặc PlayerStats!");
                yield break; // Ngừng skill nếu không tìm thấy
            }

            // 2. Vận công (Chạy âm thanh và chờ 2 giây)
            if (SFXKaioken != null)
            {
                // Giả sử bạn có class SFXPool thật
                SFXPool.Instance.Play(SFXKaioken, volume: 0.5f, pitch: 1);
            }
            yield return new WaitForSeconds(2.5f);

            // 3. TRỪ MÁU TỨC THÌ LÀM GIÁ PHẢI TRẢ (% Max HP)
            int hpCost = Mathf.RoundToInt(health.maxHP * hpCostPercent);
            // Có thể dùng DamageType.FireDOT để không kích hoạt animation bị văng đi (knockback)
            health.TakeDamage(hpCost, DamageType.FireDOT);

            // Nếu dùng Kaioken mà chết luôn thì ngừng skill
            if (health.currentHP <= 0) yield break;

            // 4. BẬT HIỆU ỨNG VFX
            GameObject currentVFX = null;
            if (VFXKaiokenPrefab != null)
            {
                // Tạo VFX và cho nó làm "con" của Player để nó di chuyển theo Player
                currentVFX = Instantiate(VFXKaiokenPrefab, player.transform.position + new Vector3(0, 1f, 0), Quaternion.identity, player.transform);
            }

            // 5. TĂNG DAMAGE (BUFF)
            stats.Damage += bonusDamage;
            Debug.Log($"[Kaioken] Đã kích hoạt! Mất {hpCost} HP. Damage tăng thêm {bonusDamage}!");

            // 6. CHỜ CHO ĐẾN HẾT THỜI GIAN
            yield return new WaitForSeconds(duration);

            // 7. TRẢ LẠI TRẠNG THÁI CŨ
            stats.Damage -= bonusDamage;

            if (currentVFX != null)
            {
                Destroy(currentVFX); // Xóa hiệu ứng đi
            }

            Debug.Log("[Kaioken] Đã hết thời gian! Sát thương trở về bình thường.");
        }
    }
}