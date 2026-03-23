using System.Collections; // Nhớ có thư viện này để dùng IEnumerator
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossController : MonoBehaviour
{
    BossHealth health;
    BossPhase phase;
    BossAttackBase attack;
    BossMovement movement;
    BossGroundMovement groundMovement;
    FinalBossMovement finalBossMovement;
    Animator anim;
    BossDialogue bossDialogue; // Script lo liệu phần thoại
    private ChaosMechanic chaosMechanic; // Script lo liệu phần Hư Không (Final Boss)
    private EnvironmentBurn environmentBurn; // Script lo liệu phần cháy môi trường (Final Boss)

    public bool isFinalBoss = false;

    public BossCutsceneManager cutsceneManager;
    private bool hasPlayedPhase2Cutscene = false;

    [Header("Scene Transition (Phòng khi không có BossDialogue)")]
    [Tooltip("Tên Scene tiếp theo, chỉ dùng khi không gắn BossDialogue")]
    public string nextSceneName;

    // (MỚI) Biến cờ khóa Boss lúc đang đọc thoại đầu game
    private bool isBattleStarted = false;

    void Awake()
    {
        health = GetComponent<BossHealth>();
        phase = GetComponent<BossPhase>();
        attack = GetComponent<BossAttackBase>();
        movement = GetComponent<BossMovement>();
        groundMovement = GetComponent<BossGroundMovement>();
        anim = GetComponent<Animator>();
        bossDialogue = GetComponent<BossDialogue>();
        if (isFinalBoss)
        {
            finalBossMovement = GetComponent<FinalBossMovement>();
            chaosMechanic = GetComponent<ChaosMechanic>();
        }
        environmentBurn = GetComponent<EnvironmentBurn>();

    }

    void Start()
    {
        health.OnPhaseChanged += OnBossPhaseCheck;
        health.OnDeath += OnBossDeath;

        // Bắt đầu chuỗi sự kiện khi vừa gặp Boss
        StartCoroutine(StartBattleRoutine());
    }

    // --- COROUTINE: KHI VỪA CHẠM MẶT BOSS ---
    IEnumerator StartBattleRoutine()
    {
        // 1. (MỚI) CHỜ CHIẾU PHIM INTRO TRƯỚC
        if (cutsceneManager != null)
        {
            // Code sẽ tạm dừng ở đây cho đến khi phim chiếu xong
            yield return StartCoroutine(cutsceneManager.PlayIntroCutsceneRoutine());
        }

        // Lưu ý: Sau khi phim kết thúc, BossCutsceneManager sẽ tự bật lại playerMovement và bossAttack.
        // Ta cần phải tạm khóa chúng lại một lần nữa để Boss không đánh lén lúc đang nói chuyện!
        if (attack) attack.enabled = false;
        if (movement) movement.enabled = false;
        if (groundMovement)
        {
            groundMovement.Stop();
            groundMovement.enabled = false;
        }
        if (finalBossMovement) finalBossMovement.stopMove();

        // 2. PHÁT THOẠI INTRO
        if (bossDialogue != null)
        {
            // Code lại tiếp tục dừng ở đây chờ người chơi bấm đọc hết chữ
            yield return StartCoroutine(bossDialogue.PlayIntroRoutine());
        }

        // 3. ĐỌC THOẠI XONG -> MỞ KHÓA CHO BOSS KHÔ MÁU
        if (attack) attack.enabled = true;
        if (movement) movement.enabled = true;
        if (groundMovement) groundMovement.enabled = true;
        if (finalBossMovement) finalBossMovement.startMove();

        isBattleStarted = true;

        //setup enviroment
        if (isFinalBoss && chaosMechanic != null)
            StartCoroutine(chaosMechanic.ChaosLoopRoutine());
        if (environmentBurn != null)
            environmentBurn.startBurn();
    }

    void Update()
    {
        if (!isBattleStarted) return; // Nếu đang nói chuyện thì không gọi hàm đánh

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

        if (phase.isPhase2 && !hasPlayedPhase2Cutscene)
        {
            hasPlayedPhase2Cutscene = true;
            if (cutsceneManager != null)
            {
                cutsceneManager.PlayPhase2Cutscene();
            }
        }
    }

    void OnBossDeath()
    {
        if (isFinalBoss && chaosMechanic != null)
            chaosMechanic.StopChaos();
        if (environmentBurn != null)
            environmentBurn.stopBurn();


        // Chuyển quyền xử lý cái chết cho BossDialogue
        if (bossDialogue != null)
        {
            // Khóa boss lại để tránh bị lỗi vừa chết vừa đánh
            if (attack) attack.enabled = false;
            if (movement) movement.enabled = false;
            if (groundMovement) groundMovement.enabled = false;
            if (finalBossMovement) finalBossMovement.stopMove();

            StartCoroutine(bossDialogue.DecisionRoutine());
        }
        else
        {
            // Code đề phòng nếu bạn quên gắn script BossDialogue
            StartCoroutine(DeathWithoutDialogueRoutine());
        }

        //cutsceneManager.PlayDeathCutscene();
    }

    // Coroutine xử lý chết khi không có BossDialogue
    IEnumerator DeathWithoutDialogueRoutine()
    {
        if (cutsceneManager != null)
            yield return StartCoroutine(cutsceneManager.PlayDeathCutscene());

        yield return new WaitForSeconds(1.5f);

        // Chuyển sang Scene tiếp theo
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            if (SceneController.Instance != null)
                SceneController.Instance.LoadScene(nextSceneName);
            else
                SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("BossController: nextSceneName chưa được gán!");
        }
    }
}