using UnityEngine;
using UnityEngine.Playables;

public class IntroChangeScene : MonoBehaviour
{
    public PlayableDirector introCutScene;

    void Start()
    {
        if (introCutScene != null)
        {
            // Đăng ký: "Ê Timeline, khi nào mày chạy xong thì gọi hàm ChangeScene nhé!"
            introCutScene.stopped += ChangeScene;

            // Bắt đầu phát
            introCutScene.Play();
        }
    }

    // [MỚI] Thêm hàm Update để lắng nghe nút Space
    void Update()
    {
        // Kiểm tra xem Cutscene có đang chạy không và người chơi có bấm Space không
        if (introCutScene != null && introCutScene.state == PlayState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 1. Tua nhanh thời gian đến frame cuối cùng
                introCutScene.time = introCutScene.duration;

                // 2. Ép cập nhật trạng thái
                introCutScene.Evaluate();

                // 3. Dừng phim (lệnh này sẽ tự động kích hoạt cái event 'stopped' ở trên)
                introCutScene.Stop();
            }
        }
    }

    // Hàm này sẽ tự động chạy khi Timeline kết thúc (hoặc khi bị ép Stop ở hàm Update)
    private void ChangeScene(PlayableDirector director)
    {
        // [MỚI] Mẹo nhỏ: Hủy đăng ký sự kiện trước khi chuyển Scene để game không bị rò rỉ bộ nhớ
        introCutScene.stopped -= ChangeScene;

        SceneController.Instance.LoadScene("FortuneScene");
    }
}