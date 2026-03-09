using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class PlayerStatUI : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject statPanel;
    public TextMeshProUGUI titleText;       // Shows emoji + title (e.g. "🐉 Heir of the Dragon")
    public TextMeshProUGUI statNameText;    // Shows stat name (e.g. "Dragon Blood")
    public TextMeshProUGUI effectText;      // Shows effect summary (e.g. "+25% Max HP")
    public TextMeshProUGUI typeText;        // Shows stat type (e.g. "Buff")
    public SpriteRenderer FrameCard;              // Color bar that changes based on stat type
    public SpriteRenderer Card;
    public GameObject cardFade;

    public Sprite buffIcon;   // Optional: assign icons for each type
    public Sprite debuff;   // Optional: assign icons for each type
    public Sprite mixed;   // Optional: assign icons for each type
    public Sprite neutral;   // Optional: assign icons for each type

    [Header("Animation")]
    public float fadeInDuration = 1f;
    public float displayDuration = 5f;  // How long to show before auto-hiding (0 = manual close only)

    private CanvasGroup canvasGroup;
    [SerializeField] PlayableDirector Introtimeline;

    private void Awake()
    {
        // Get or add CanvasGroup for fade animation
        canvasGroup = statPanel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = statPanel.AddComponent<CanvasGroup>();

    }

    private void Start()
    {
        // Listen for stat assignment events
        if (PlayerStatManager.Instance != null)
        {
            PlayerStatManager.Instance.OnStatAssigned += OnStatAssigned;
        }
    }

    private void OnDestroy()
    {
        if (PlayerStatManager.Instance != null)
        {
            PlayerStatManager.Instance.OnStatAssigned -= OnStatAssigned;
        }
    }

    private void OnStatAssigned(PlayerStatProfile stat)
    {
        ShowStat(stat);
    }

    /// <summary>
    /// Displays the stat info on the UI panel with a fade-in animation.
    /// </summary>
    public void ShowStat(PlayerStatProfile stat)
    {
        if (stat == null) return;

        // Bật gameObject chứa script này lên trước để có thể chạy Coroutine
        gameObject.SetActive(true);

        // Populate UI
        if (titleText) titleText.text = stat.cardNumeral;
        if (statNameText) statNameText.text = $"{stat.titleEmoji} {stat.cardName}";
        if (effectText) effectText.text = stat.GetEffectSummary();
        if (typeText) typeText.text = stat.statType.ToString().ToUpper();
        if (Card) Card.sprite = stat.cardImage;

        // Set color based on type
        Sprite typeColor = GetColorForType(stat.statType);
        if (FrameCard) FrameCard.sprite = typeColor;
        ////if (typeText) typeText.color = typeColor;

        // Show panel with animation
        StopAllCoroutines();
        StartCoroutine(ShowPanelRoutine());
    }

    /// <summary>
    /// Hides the stat panel immediately.
    /// </summary>
    public void HidePanel()
    {
        StopAllCoroutines();
        statPanel.SetActive(false);
    }

    private IEnumerator ShowPanelRoutine()
    {
        FrameCard.gameObject.SetActive(false);
        canvasGroup.alpha = 0f;

        yield return StartCoroutine(playIntro()); // Wait for intro cutscene to finish before showing stat

        cardFade.SetActive(true);
        Card.gameObject.SetActive(true);

        // LẤY COMPONENT IMAGE ĐỂ LÀM HIỆU ỨNG TỪ TRÊN XUỐNG DƯỚI
        Image fadeImage = cardFade.GetComponent<Image>();
        if (fadeImage != null)
        {
            fadeImage.type = Image.Type.Filled;
            fadeImage.fillMethod = Image.FillMethod.Vertical;
            fadeImage.fillOrigin = (int)Image.OriginVertical.Bottom;
            fadeImage.fillAmount = 1f; // Đảm bảo lúc bắt đầu ảnh hiển thị 100%
        }

        // Đợi người chơi bấm phím lần 1
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));

        // --- HIỆU ỨNG CROSSFADE ---
        //statPanel.SetActive(true);

        float elapsed = 0f;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration; // Tiến trình từ 0 -> 1

            // 2. HIỆU ỨNG QUÉT TỪ TRÊN XUỐNG DƯỚI (fillAmount giảm từ 1 -> 0)
            if (fadeImage != null)
            {
                fadeImage.fillAmount = Mathf.Lerp(1f, 0f, progress);
            }

            yield return null;
        }
        yield return new WaitForSeconds(1f); // Nhỏ delay để đảm bảo FrameCard đã bật trước khi bắt đầu hiện dần
        FrameCard.gameObject.SetActive(true);

        elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / fadeInDuration; // Tiến trình từ 0 -> 1

            // 1. Hiện dần statPanel lên (alpha tăng từ 0 -> 1)
            canvasGroup.alpha = Mathf.Clamp01(progress);

            yield return null;
        }

        // Đảm bảo các giá trị đạt chuẩn ở frame cuối
        canvasGroup.alpha = 1f;
        if (fadeImage != null) fadeImage.fillAmount = 0f;

        cardFade.SetActive(false); // Tắt hẳn cardFade đi cho nhẹ máy

        // Đợi người chơi bấm phím lần 2 để tắt bảng chỉ số
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));

        // Fade out statPanel
        elapsed = 0f;
        // 1. Lấy màu hiện tại của Card ra một biến tạm
        Color cardColor = Card.color;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = 1f - Mathf.Clamp01(elapsed / fadeInDuration);

            // 2. Chỉnh sửa kênh Alpha (a) của biến tạm đó
            cardColor.a = 1f - Mathf.Clamp01(elapsed / fadeInDuration);

            // 3. Gắn biến màu đã chỉnh sửa ngược trở lại cho Card
            Card.color = cardColor;
            yield return null;
        }

        statPanel.SetActive(false);
    }

    IEnumerator playIntro()
    {
        if (Introtimeline != null)
        {
            Introtimeline.Play();
            while (Introtimeline.state == PlayState.Playing)
            {
                yield return null;
            }
        }
    }
    private Sprite GetColorForType(StatType type)
    {
        switch (type)
        {
            case StatType.Buff: return buffIcon;
            case StatType.Debuff: return debuff;
            case StatType.Mixed: return mixed;
            case StatType.Neutral: return neutral;
            default: return neutral;
        }
    }
}
