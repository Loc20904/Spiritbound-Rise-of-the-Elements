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
    public MonoBehaviour playerMovement;
    public BossAttackBase bossAttack;
    public BossGroundMovement bossMove;
    public FinalBossMovement FinalBossMove;
    public BossHealth bossHealth;
    public Transform skillHolder;

    // ĐÃ XÓA HÀM START TỰ ĐỘNG CHẠY Ở ĐÂY

    // (MỚI) Biến hàm chiếu Intro thành Coroutine để hệ thống có thể chờ
    public IEnumerator PlayIntroCutsceneRoutine()
    {
        if (introTimeline != null)
        {
            PlayCutscene(introTimeline);

            // Bắt vòng lặp đợi cho đến khi Timeline chạy xong hoàn toàn
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
        if (playerMovement) playerMovement.enabled = false;
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

        director.Play();
        director.stopped += OnCutsceneFinished;
    }

    void OnCutsceneFinished(PlayableDirector director)
    {
        director.stopped -= OnCutsceneFinished;

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