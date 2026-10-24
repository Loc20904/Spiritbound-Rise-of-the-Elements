using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChaosMechanic : MonoBehaviour
{
    [Header("Chaos Settings")]
    public float interval = 15f;
    public float reverseDuration = 5f;

    [Header("Visual Effects")]
    public Image chaosFlashImage;
    public float flashDuration = 5f;

    private bool isChaosActive = true;
    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Mở comment để bắt đầu vòng lặp Hư Không
        StartCoroutine(ChaosLoopRoutine());
    }

    public IEnumerator ChaosLoopRoutine()
    {
        isChaosActive = true;
        while (isChaosActive)
        {
            yield return new WaitForSeconds(interval);
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

        // Lấy component PlayerController
        PlayerController playerCtrl = player.GetComponent<PlayerController>();
        if (playerCtrl == null) yield break;

        // Chớp nháy màn hình
        if (chaosFlashImage != null)
        {
            StartCoroutine(FlashScreen(new Color(0.5f, 0f, 0.8f, 0.6f)));
        }

        // Kích hoạt đảo ngược
        playerCtrl.SetReverseControl(true);
        Debug.Log("BOSS VOID: Đảo ngược điều khiển!");

        // Giữ trạng thái trong 5s
        yield return new WaitForSeconds(reverseDuration);

        // Trả lại bình thường
        playerCtrl.SetReverseControl(false);
        Debug.Log("BOSS VOID: Điều khiển bình thường trở lại.");
    }

    private IEnumerator FlashScreen(Color flashColor)
    {
        chaosFlashImage.color = flashColor;

        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
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