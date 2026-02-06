using System.Collections;
using UnityEngine;

namespace Assets.Scripts.AI.FinalBoss
{
    public class EnergyBoom : MonoBehaviour
    {
        [Header("Settings")]
        public float delayBeforeExplosion = 3f;
        public float explosionRadius = 3f;
        public LayerMask playerLayer;

        [Header("Effects")]
        public GameObject explosionVFX;
        public AudioClip energyIdleSound; // Âm thanh gồng năng lượng
        public AudioClip explosionSound;  // Âm thanh nổ

        private bool _isExploded = false;

        private void Start()
        {
            // Bắt đầu chuỗi kích nổ
            StartCoroutine(ExplosionRoutine());
        }

        private IEnumerator ExplosionRoutine()
        {
            // 1. Phát âm thanh năng lượng khi vừa xuất hiện
            PlaySound(energyIdleSound, 0.7f);

            // 2. Đợi trong tầm 3s (có thể thêm hiệu ứng nháy đỏ tại đây)
            yield return new WaitForSeconds(delayBeforeExplosion);

            // 3. Kích nổ
            StartCoroutine(Explode());
        }

        private IEnumerator Explode()
        {
            if (_isExploded) yield return null;
            _isExploded = true;
            // Phát hiệu ứng hình ảnh (VFX)

            GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            // Phát âm thanh nổ (SFX)
            PlaySound(explosionSound, 0.5f);
            // Gây sát thương trong phạm vi
            HandleDamage();
            // Xóa object sau khi nổ
            Destroy(vfx, 1f);
            Destroy(gameObject);
        }

        private void HandleDamage()
        {
            Collider2D hit = Physics2D.OverlapCircle(transform.position, explosionRadius, playerLayer);
            if (hit != null)
            {
                // Giả sử script Player có hàm TakeDamage
                // hit.GetComponent<IHealth>()?.TakeDamage(30);
            }
        }

        // Tận dụng hàm PlaySound bạn đã cung cấp
        protected void PlaySound(AudioClip clip, float volume)
        {
            if (clip != null && SFXPool.Instance != null)
            {
                // Random pitch nhẹ để âm thanh tự nhiên hơn như bạn đã viết
                SFXPool.Instance.Play(clip, volume, UnityEngine.Random.Range(0.9f, 1.1f));
            }
        }

        // Vẽ vòng tròn phạm vi trong Editor để dễ căn chỉnh
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}