using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BossDialogue : MonoBehaviour
{
    [Header("Boss Settings")]
    [Tooltip("Tick vào đây NẾU ĐÂY LÀ BOSS ĐẦU TIÊN (Boss quyết định Route của game)")]
    public bool isFirstBoss = false;

    [Header("Scene Transition")]
    [Tooltip("Tên Scene sẽ chuyển tới sau khi đánh bại Boss")]
    public string nextSceneName;

    [Header("Boss Dialogues")]
    public DialogueSequence introDialogue;     // (MỚI) Thoại khi vừa chạm mặt Boss
    public DialogueSequence preChoiceDialogue; // Thoại hấp hối
    public DialogueSequence killDialogue;      // Thoại nguyền rủa khi bị giết (Route Kill)
    public DialogueSequence mercyDialogue;     // Thoại cảm kích khi được tha (Route Mercy)

    [Header("Choice System (Chỉ Boss Đầu mới dùng)")]
    public GameObject choiceUIPanel;
    private int choiceResult = -1;

    Animator anim;
    public BossCutsceneManager cutsceneManager;

    public void Awake()
    {
        if (choiceUIPanel != null)
            choiceUIPanel.SetActive(false);
        anim = GetComponent<Animator>();
    }

    // --- (MỚI) PHÁT THOẠI INTRO KHI VỪA VÀO SCENE ---
    public IEnumerator PlayIntroRoutine()
    {
        if (introDialogue != null && DialogueUIManager.Instance != null)
        {
            yield return StartCoroutine(DialogueUIManager.Instance.PlayDialogueRoutine(introDialogue));
        }
    }

    // --- COROUTINE XỬ LÝ LỰA CHỌN HOẶC TỰ ĐỘNG ---
    public IEnumerator DecisionRoutine()
    {
        if (anim) anim.SetTrigger("Kneel");

        yield return new WaitForSeconds(1f);

        // Luôn phát thoại hấp hối
        yield return StartCoroutine(PlayPreChoiceDialogue());

        // KIỂM TRA XEM LÀ BOSS NÀO ĐỂ XỬ LÝ
        if (isFirstBoss)
        {
            // LÀ BOSS ĐẦU TIÊN -> HIỆN UI LỰA CHỌN
            if (choiceUIPanel) choiceUIPanel.SetActive(true);
            choiceResult = -1;

            while (choiceResult == -1)
            {
                yield return null;
            }

            if (choiceUIPanel) choiceUIPanel.SetActive(false);

            if (choiceResult == 0)
            {
                yield return StartCoroutine(ExecuteKillPath(true)); // true = Lưu cờ vào GameProgress
            }
            else if (choiceResult == 1)
            {
                yield return StartCoroutine(ExecuteAllyPath(true));
            }
        }
        else
        {
            // LÀ BOSS SAU -> TỰ ĐỘNG ĐỌC FLAG VÀ RẼ NHÁNH KHÔNG CẦN HỎI
            bool isKillPath = false;
            if (GameProgressManager.Instance != null)
            {
                // Kiểm tra xem Boss đầu tiên người chơi đã chọn đường nào
                isKillPath = GameProgressManager.Instance.GetFlag("GlobalPath_Kill");
            }

            if (isKillPath)
            {
                yield return StartCoroutine(ExecuteKillPath(false)); // false = Không cần lưu cờ nữa
            }
            else
            {
                yield return StartCoroutine(ExecuteAllyPath(false));
            }
        }
    }

    public void SetChoice(int result)
    {
        choiceResult = result;
    }

    // --- CÁC NHÁNH KẾT THÚC ---
    IEnumerator ExecuteKillPath(bool isFirstBoss)
    {
        // Nếu là Boss đầu tiên chọn Kill -> Mở khóa tuyến truyện Genocide (Giết chóc)
        if (isFirstBoss && GameProgressManager.Instance != null)
            GameProgressManager.Instance.SetFlag("GlobalPath_Kill", true);

        yield return StartCoroutine(PlayKillDialogue());

        if (cutsceneManager != null) yield return StartCoroutine(cutsceneManager.PlayDeathCutscene());

        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);

        // Chuyển sang Scene tiếp theo sau khi đánh bại Boss
        LoadNextScene();
    }

    IEnumerator ExecuteAllyPath(bool isFirstBoss)
    {
        // Nếu là Boss đầu tiên chọn Tha -> Mở khóa tuyến truyện Pacifist (Hòa bình)
        if (isFirstBoss && GameProgressManager.Instance != null)
            GameProgressManager.Instance.SetFlag("GlobalPath_Mercy", true);

        yield return StartCoroutine(PlayMercyDialogue());

        if (anim) anim.SetTrigger("Recover");

        yield return new WaitForSeconds(3f);

        // Chuyển sang Scene tiếp theo sau khi đánh bại Boss
        LoadNextScene();
    }

    // Các hàm phát thoại phụ trợ (Giữ nguyên)
    public IEnumerator PlayPreChoiceDialogue()
    {
        if (preChoiceDialogue != null && DialogueUIManager.Instance != null)
            yield return StartCoroutine(DialogueUIManager.Instance.PlayDialogueRoutine(preChoiceDialogue));
    }

    public IEnumerator PlayKillDialogue()
    {
        if (killDialogue != null && DialogueUIManager.Instance != null)
            yield return StartCoroutine(DialogueUIManager.Instance.PlayDialogueRoutine(killDialogue));
    }

    public IEnumerator PlayMercyDialogue()
    {
        if (mercyDialogue != null && DialogueUIManager.Instance != null)
            yield return StartCoroutine(DialogueUIManager.Instance.PlayDialogueRoutine(mercyDialogue));
    }

    // --- CHUYỂN SCENE SAU KHI ĐÁNH BẠI BOSS ---
    private void LoadNextScene()
    {
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("BossDialogue: nextSceneName chưa được gán! Không thể chuyển Scene.");
            return;
        }

        Debug.Log($"Chuyển sang Scene: {nextSceneName}");

        if (SceneController.Instance != null)
        {
            SceneController.Instance.LoadScene(nextSceneName);
        }
        else
        {
            // Phòng trường hợp SceneController chưa tồn tại
            SceneManager.LoadScene(nextSceneName);
        }
    }
}