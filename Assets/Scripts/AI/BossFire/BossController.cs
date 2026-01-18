using UnityEngine;

public class BossController : MonoBehaviour
{
    BossHealth health;
    BossPhase phase;
    BossAttack attack;
    BossMovement movement;
    Animator anim;

    public BossCutsceneManager cutsceneManager;
    private bool hasPlayedPhase2Cutscene = false;

    void Awake()
    {
        health = GetComponent<BossHealth>();
        phase = GetComponent<BossPhase>();
        attack = GetComponent<BossAttack>();
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
        attack.Attack();
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
        anim.SetTrigger("die");
        Destroy(gameObject, 2f);
    }
}
