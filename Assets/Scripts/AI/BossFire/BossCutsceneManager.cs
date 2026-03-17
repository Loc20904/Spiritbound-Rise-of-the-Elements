using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class BossCutsceneManager : MonoBehaviour
{
    [Header("Timelines")]
    public PlayableDirector introTimeline;
    public PlayableDirector phase2Timeline;
    public PlayableDirector DeathTimeline;

    [Header("UI")]
    public GameObject gameUI;

    [Header("Controllers")]
    public PlayerController playerMovement;
    public BossAttackBase bossAttack;
    public BossGroundMovement bossMove;
    public FinalBossMovement FinalBossMove;
    public BossHealth bossHealth;
    public Transform skillHolder;

    // [MỚI] Biến để theo dõi xem Cutscene nào đang được chiếu
    private PlayableDirector currentPlayingDirector;

    // [MỚI] Lắng nghe nút Space để Skip
    void Update()
    {
        if (currentPlayingDirector != null && currentPlayingDirector.state == PlayState.Playing)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                // 1. Tua nhanh thời gian của Timeline đến đúng giây cuối cùng
                currentPlayingDirector.time = currentPlayingDirector.duration;

                // 2. Ép Unity tính toán và áp dụng ngay lập tức trạng thái ở frame cuối này
                currentPlayingDirector.Evaluate();

                // 3. Bây giờ mới chính thức dừng phim (sẽ tự động gọi OnCutsceneFinished)
                currentPlayingDirector.Stop();
            }
        }
    }

    public IEnumerator PlayIntroCutsceneRoutine()
    {
        if (introTimeline != null)
        {
            PlayCutscene(introTimeline);

            // Bắt vòng lặp đợi cho đến khi Timeline chạy xong hoàn toàn (hoặc bị ép Stop)
            while (introTimeline.state == PlayState.Playing)
            {
                yield return null;
            }
        }
    }

    public void PlayPhase2Cutscene()
    {
        if (phase2Timeline != null) PlayCutscene(phase2Timeline);
    }

    public IEnumerator PlayDeathCutscene()
    {
        if (DeathTimeline != null)
        {
            PlayCutscene(DeathTimeline);
            while (DeathTimeline.state == PlayState.Playing) yield return null;
        }
    }

    void PlayCutscene(PlayableDirector director)
    {
        if (skillHolder != null)
        {
            foreach (Transform child in skillHolder) Destroy(child.gameObject);
        }

        // Khóa điều khiển

        if (playerMovement)
        {
            //playerMovement.StopMove();
            playerMovement.enabled = false;
        }
        if (bossAttack) bossAttack.enabled = false;
        if (bossMove)
        {
            bossMove.Stop();
            bossMove.enabled = false;
        }
        if (FinalBossMove)
        {
            FinalBossMove.stopMove();
            //FinalBossMove.enabled = false;
        }

        bossAttack.ultiReady = 0f;
        bossAttack.StopAllCoroutines();
        bossAttack.CancelInvoke();

        if (gameUI) gameUI.SetActive(false);
        if (bossHealth) bossHealth.isInvulnerable = true;

        // [MỚI] Gán cutscene này làm cutscene hiện tại đang chạy
        currentPlayingDirector = director;

        director.Play();
        director.stopped += OnCutsceneFinished;
    }

    void OnCutsceneFinished(PlayableDirector director)
    {
        director.stopped -= OnCutsceneFinished;

        // [MỚI] Reset lại biến báo hiệu không có phim nào đang chiếu
        currentPlayingDirector = null;

        // Trả lại điều khiển sau khi hết phim
        if (playerMovement) playerMovement.enabled = true;
        if (bossAttack) bossAttack.enabled = true;
        if (bossMove) bossMove.enabled = true;
        //if (FinalBossMove) FinalBossMove.enabled = true;
        if (FinalBossMove) FinalBossMove.startMove();

        if (gameUI) gameUI.SetActive(true);
        if (bossHealth) bossHealth.isInvulnerable = false;
    }
}