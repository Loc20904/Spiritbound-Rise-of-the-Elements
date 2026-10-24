using UnityEngine;
using System;
using System.Collections;

public class EarthSummon : MonoBehaviour
{
    [Header("Prefabs (thay vì Sprite)")]
    public GameObject smallStonePrefab;   // frame 1 - đá nhỏ
    public GameObject bigStonePrefab;     // frame 2 - đá lớn
    public GameObject mergedStonePrefab;  // frame 3 - hợp nhất

    [Tooltip("Tốc độ bay lên (số càng lớn càng nhanh). ~12–20 cho nhanh.")]
    public float riseSpeed = 15f;
    [Tooltip("Chờ bao lâu rồi mới spawn đá lớn sau khi đá nhỏ tới.")]
    public float bigDelay = 0.2f;

    private Vector3 targetPoint;
    private Transform player;
    private Action onComplete;
    private RangerBotFly rangerBot;
    private bool keepMergedForFirePoint1; // true = firePoint1: giữ merged đến khi bắn

    private GameObject smallObj;

    /// <param name="keepMergedForSecondFirePoint">true cho firePoint1: merged giữ đến khi bắn (không biến mất sớm)</param>
    public void Init(Vector3 firePointPos, RangerBotFly bot, Transform target, Action onComplete = null, bool keepMergedForSecondFirePoint = false)
    {
        targetPoint = firePointPos;
        player = target;
        this.onComplete = onComplete;
        rangerBot = bot;
        keepMergedForFirePoint1 = keepMergedForSecondFirePoint;

        StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        if (smallStonePrefab == null) yield break;

        // 🔹 Spawn đá nhỏ (prefab)
        smallObj = Instantiate(smallStonePrefab, transform.position, Quaternion.identity);

        // Bay đá nhỏ lên trước
        yield return StartCoroutine(MoveToTarget(smallObj));

        // 🔹 Chờ 1 chút rồi spawn đá lớn
        yield return new WaitForSeconds(bigDelay);

        if (bigStonePrefab == null) { Destroy(smallObj); Destroy(gameObject); yield break; }

        GameObject bigObj = Instantiate(bigStonePrefab, transform.position, Quaternion.identity);

        // Bay đá lớn lên
        yield return StartCoroutine(MoveToTarget(bigObj));

        // 🔹 Khi big tới nơi → hợp nhất (frame 3): thay bằng prefab merged, không bay nữa
        Destroy(bigObj);
        Destroy(smallObj);

        if (mergedStonePrefab != null)
        {
            GameObject mergedObj = Instantiate(mergedStonePrefab, targetPoint, Quaternion.identity);
            yield return new WaitForSeconds(0.2f);

            if (keepMergedForFirePoint1 && rangerBot != null)
            {
                // firePoint1: giao merged cho RangerBotFly, giữ đến khi bắn mới hủy
                rangerBot.SetPendingMergedAtFirePoint1(mergedObj);
            }
            else
            {
                Destroy(mergedObj);
            }
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        // Báo CastSkill "xong" — CastSkill sẽ gọi bắn khi CẢ HAI EarthSummon xong
        onComplete?.Invoke();

        Destroy(gameObject);
    }

    IEnumerator MoveToTarget(GameObject obj)
    {
        float dist = Vector2.Distance(obj.transform.position, targetPoint);
        float speed = Mathf.Max(riseSpeed, dist / 0.5f); // tối thiểu đủ để ~0.5s bay xong
        while (Vector2.Distance(obj.transform.position, targetPoint) > 0.05f)
        {
            obj.transform.position = Vector2.MoveTowards(
                obj.transform.position,
                targetPoint,
                speed * Time.deltaTime
            );
            yield return null;
        }
    }
}