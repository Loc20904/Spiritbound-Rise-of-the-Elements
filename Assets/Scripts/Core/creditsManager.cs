using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditScroller : MonoBehaviour
{
    [Header("Scroll Settings")]
    public float normalSpeed = 100f;     // Tốc độ cuộn bình thường
    public float fastSpeed = 300f;       // Tốc độ cuộn khi tua nhanh (giữ Space)
    public float endYPosition = 2000f;   // Tọa độ Y mà tại đó Credits sẽ dừng lại

    [Header("Scene Transition")]
    public string mainMenuSceneName = "MainMenuScene"; // Tên Scene bạn muốn về sau khi chạy xong

    private RectTransform rectTransform;

    void Start()
    {
        // Lấy RectTransform của cái hộp chứa UI
        rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        // 1. Xử lý tốc độ (Tua nhanh nếu giữ phím Space hoặc chuột trái)
        float currentSpeed = normalSpeed;
        if (Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0))
        {
            currentSpeed = fastSpeed;
        }

        // 2. Di chuyển chữ đi lên
        rectTransform.anchoredPosition += Vector2.up * currentSpeed * Time.deltaTime;

        // 3. Kiểm tra xem chữ đã chạy hết chưa (Vượt qua mốc endYPosition)
        if (rectTransform.anchoredPosition.y >= endYPosition)
        {
            FinishCredits();
        }

        // 4. Cho phép bấm Esc để bỏ qua (Skip) ngay lập tức
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            FinishCredits();
        }
    }

    // Hàm gọi khi chạy xong hoặc bị Skip
    public void FinishCredits()
    {
        // Chuyển về màn hình chính
        SceneManager.LoadScene(mainMenuSceneName);

        // Hoặc nếu bạn muốn hiện chuột lại để bấm menu thì thêm:
        // Cursor.visible = true;
        // Cursor.lockState = CursorLockMode.None;
    }
}