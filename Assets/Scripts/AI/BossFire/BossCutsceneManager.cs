using UnityEngine;
using UnityEngine.Playables; // Thư viện cần thiết cho Timeline

public class BossCutsceneManager : MonoBehaviour
{
    [Header("Timelines")]
    public PlayableDirector introTimeline;
    public PlayableDirector phase2Timeline;

    [Header("UI")]
    public GameObject gameUI; // Thanh máu, nút bấm... (Tắt đi cho đẹp khi chiếu phim)

    [Header("Controllers")]
    public MonoBehaviour playerMovement; // Script di chuyển của Player
    public BossAttackBase bossAttack;     // Script tấn công của Boss
    public BossHealth bossHealth;        // Để bật bất tử khi chuyển phase

    private void Start()
    {
        // Tự động chạy Intro khi vào game
        if (introTimeline != null)
        {
            PlayCutscene(introTimeline);
        }
    }

    public void PlayPhase2Cutscene()
    {
        if (phase2Timeline != null)
        {
            PlayCutscene(phase2Timeline);
        }
    }

    void PlayCutscene(PlayableDirector director)
    {
        // 1. Dừng điều khiển của người chơi và Boss
        if (playerMovement) playerMovement.enabled = false;
        if (bossAttack) bossAttack.enabled = false;

        // 2. QUAN TRỌNG: Dừng ngay mọi luồng bắn đạn đang chờ (Coroutine)
        bossAttack.StopAllCoroutines();

        // 3. Nếu Boss dùng Invoke (hẹn giờ), hủy luôn
        bossAttack.CancelInvoke();

        // 2. Tắt UI game
        if (gameUI) gameUI.SetActive(false);

        // 3. Bật bất tử cho Boss (tránh việc Player đánh lén khi Boss đang gào thét)
        if (bossHealth) bossHealth.isInvulnerable = true;

        // 4. Chạy Timeline
        director.Play();

        // 5. Đăng ký sự kiện: Khi chạy xong thì làm gì?
        director.stopped += OnCutsceneFinished;
    }

    void OnCutsceneFinished(PlayableDirector director)
    {
        // Hủy đăng ký sự kiện để tránh lỗi
        director.stopped -= OnCutsceneFinished;

        // 1. Trả lại điều khiển
        if (playerMovement) playerMovement.enabled = true;
        if (bossAttack) bossAttack.enabled = true;

        // 2. Bật lại UI
        if (gameUI) gameUI.SetActive(true);

        // 3. Tắt bất tử
        if (bossHealth) bossHealth.isInvulnerable = false;
    }
}