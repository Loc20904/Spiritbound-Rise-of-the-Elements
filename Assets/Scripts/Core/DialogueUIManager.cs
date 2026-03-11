using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUIManager : MonoBehaviour
{
    public static DialogueUIManager Instance;

    [Header("UI References")]
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Image characterAvatar;
    public Sprite characterAvatarDefault;
    public TextMeshProUGUI characterAvatarText;

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.04f;   // Tốc độ chạy chữ (càng nhỏ càng nhanh)
    public AudioClip typingSound;       // Âm thanh khi hiện 1 chữ cái
    [Range(0f, 1f)] public float soundVolume = 0.5f;

    private AudioSource audioSource;
    private bool isTyping = false;      // Đang chạy chữ hay không?

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // Tự động tìm hoặc thêm AudioSource để phát nhạc
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public IEnumerator PlayDialogueRoutine(DialogueSequence sequence)
    {
        dialoguePanel.SetActive(true);

        // Chạy từng câu thoại trong kịch bản
        foreach (DialogueLine line in sequence.lines)
        {
            // Cập nhật Avatar và Tên
            if (characterAvatar) characterAvatar.sprite = line.characterAvatar;
            else characterAvatar.sprite = characterAvatarDefault; // Nếu không có avatar riêng thì dùng mặc định
            if (characterAvatarText) characterAvatarText.text = line.characterName;

            // Xóa chữ cũ
            dialogueText.text = "";
            isTyping = true;

            // Bắt đầu Coroutine gõ chữ
            Coroutine typingCoroutine = StartCoroutine(TypeSentence(line.sentence));

            // VÒNG LẶP CHỜ: Đợi chữ gõ xong, HOẶC người chơi bấm skip
            while (isTyping)
            {
                // Nếu đang gõ mà người chơi bấm Space/Click -> Bỏ qua hiệu ứng gõ
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    StopCoroutine(typingCoroutine);   // Dừng việc gõ từng chữ
                    dialogueText.text = line.sentence; // In ra toàn bộ câu ngay lập tức
                    isTyping = false;                  // Đánh dấu là đã gõ xong
                }
                yield return null;
            }

            // Đợi 1 frame để phím bấm Skip ở trên không bị ăn lẹm xuống phím Next
            yield return null;

            // Đợi người chơi bấm phím Space/Chuột trái để qua câu TIẾP THEO
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));

            yield return null;
        }

        // Đọc xong hết mảng thoại -> Tắt UI
        dialoguePanel.SetActive(false);
    }

    // --- COROUTINE HIỆU ỨNG GÕ CHỮ ---
    private IEnumerator TypeSentence(string sentence)
    {
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter; // Thêm 1 ký tự vào màn hình

            // Phát âm thanh nếu có (chỉ phát khi ký tự không phải là khoảng trắng)
            if (typingSound != null && letter != ' ')
            {
                // Thay đổi pitch một chút xíu để tiếng thoại nghe tự nhiên, luyến láy hơn
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(typingSound, soundVolume);
            }

            // Đợi một khoảng thời gian trước khi gõ chữ tiếp theo
            yield return new WaitForSeconds(typingSpeed);
        }

        // Đã gõ xong toàn bộ
        isTyping = false;
    }
}