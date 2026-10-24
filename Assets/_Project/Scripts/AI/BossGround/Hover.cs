using UnityEngine;

namespace Assets.Scripts.AI.BossGround
{
    public class Hover : MonoBehaviour
    {
        [Header("Hover Effect (Lơ lửng)")]
        public float hoverAmplitude = 0.5f; // Biên độ: Bay lên xuống bao nhiêu
        public float hoverFrequency = 2f;   // Tần số: Tốc độ nhấp nhô

        private float startY;       // Điểm tâm để dao động
        private float randomOffset; // Để tạo sự lệch pha ngẫu nhiên

        void Start()
        {
            // 1. Lưu vị trí Y ban đầu làm mốc (Dùng localPosition để an toàn khi parent di chuyển)
            startY = transform.localPosition.y;

            // 2. Tạo độ lệch ngẫu nhiên để các object không nhấp nhô đồng bộ 100%
            randomOffset = Random.Range(0f, 10f);
        }

        void Update()
        {
            // 3. Tính toán vị trí Y mới
            // Công thức: Y Gốc + Sin(Thời gian + Độ lệch) * Biên độ
            float newY = startY + Mathf.Sin((Time.time + randomOffset) * hoverFrequency) * hoverAmplitude;

            // 4. Cập nhật vị trí (Giữ nguyên X và Z hiện tại, chỉ đổi Y)
            transform.localPosition = new Vector3(transform.localPosition.x, newY, transform.localPosition.z);
        }
    }
}