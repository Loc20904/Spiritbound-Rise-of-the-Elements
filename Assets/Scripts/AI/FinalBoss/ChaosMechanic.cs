using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChaosMechanic : MonoBehaviour
{
    [Header("Chaos Settings")]
    public float interval = 15f; // Định kỳ mỗi 15 giây
    public float reverseDuration = 5f; // Bị đảo ngược trong 5 giây

    [Header("Visual Effects")]
    //public GameObject imageObject;
    public Image chaosFlashImage; // UI Image chớp nháy luồng sáng tím
    public float flashDuration = 5f;

    private bool isChaosActive = true;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        // Bắt đầu vòng lặp Hư Không
        //StartCoroutine(ChaosLoopRoutine());
    }

    public IEnumerator ChaosLoopRoutine()
    {
        isChaosActive = true;
        while (isChaosActive)
        {
            // Đợi X giây (15s)
            yield return new WaitForSeconds(interval);

            // Bẻ cong thực tại! (Kích hoạt đảo ngược)
            yield return StartCoroutine(TriggerReverseControls());
        }
    }

    public void StopChaos()
    {
        isChaosActive = false;
    }

    private IEnumerator TriggerReverseControls()
    {
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null) yield break;

        //PlayerMovement playerMove = player.GetComponent<PlayerMovement>();
        //if (playerMove == null) yield break;

        // Chớp nháy màn hình sáng tím để cảnh báo
        if (chaosFlashImage != null)
        {
            StartCoroutine(FlashScreen(new Color(0.5f, 0f, 0.8f, 0.6f))); // Tím trong suốt
        }

        // Đảo ngược nút
        //playerMove.isReversed = true;
        Debug.Log("BOSS VOID: Đảo ngược điều khiển!");

        // Giữ trạng thái trong Y giây (5s)
        yield return new WaitForSeconds(reverseDuration);

        // Trả lại bình thường
        //playerMove.isReversed = false;
        Debug.Log("BOSS VOID: Điều khiển bình thường trở lại.");
    }

    private IEnumerator FlashScreen(Color flashColor)
    {
        //imageObject.SetActive(true);
        chaosFlashImage.color = flashColor;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            // Mờ dần về 0
            Color c = flashColor;
            c.a = Mathf.Lerp(flashColor.a, 0f, elapsed / flashDuration);
            chaosFlashImage.color = c;
            yield return null;
        }

        Color finalColor = flashColor;
        finalColor.a = 0f;
        chaosFlashImage.color = finalColor;
    }
}
