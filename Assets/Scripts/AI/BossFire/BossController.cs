using UnityEngine;

public class BossController : MonoBehaviour
{
    BossHealth health;
    BossPhase phase;
    BossAttackBase attack;
    BossMovement movement;
    Animator anim;
    public bool isFinalBoss = false;

    public BossCutsceneManager cutsceneManager;
    private bool hasPlayedPhase2Cutscene = false;

    void Awake()
    {
        health = GetComponent<BossHealth>();
        phase = GetComponent<BossPhase>();
        attack = GetComponent<BossAttackBase>();
        movement = GetComponent<BossMovement>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        health.OnPhaseChanged += OnBossPhaseCheck;
        health.OnDeath += OnBossDeath;
    }

    void Update()
    {
        if (!isFinalBoss)
            attack.Attack();
        else
        {
            attack.routineUlti();
        }
    }

    void OnBossPhaseCheck(float hpPercent)
    {
        phase.CheckPhase(hpPercent);
        attack.SetPhase(phase.isPhase2);
        //movement.SetPhase(phase.isPhase2);

        if (phase.isPhase2 && !hasPlayedPhase2Cutscene)
        {
            hasPlayedPhase2Cutscene = true;

            // Gọi Cutscene
            if (cutsceneManager != null)
            {
                cutsceneManager.PlayPhase2Cutscene();
            }
        }
    }


    void OnBossDeath()
    {
        // 1. Chạy phim cắt cảnh cái chết và đợi nó kết thúc hoàn toàn
        // Giả sử PlayDeathCutscene() trả về một IEnumerator xử lý Timeline
        cutsceneManager.PlayDeathCutscene();

        // 4. Cuối cùng mới xóa GameObject Boss
        //Destroy(gameObject);

        // 5. Chuyển Scene kết thúc (Ví dụ: SceneManager.LoadScene("EndGame"))
    }
}
