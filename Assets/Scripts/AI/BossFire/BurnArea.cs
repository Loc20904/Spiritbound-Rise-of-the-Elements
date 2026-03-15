using System.Collections;
using UnityEngine;

public class EnvironmentBurn : MonoBehaviour
{
    [Header("Ambient Burn Settings")]
    [Tooltip("Lượng sát thương mỗi lần kích hoạt")]
    public float damageTick = 5f;
    [Tooltip("Khoảng thời gian (giây) giữa các lần trừ máu (để không bị ức chế)")]
    public float tickInterval = 3f;
    private bool isBurning = false;

    private Transform player;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        StartCoroutine(BurnRoutine());
    }

    public void startBurn()
    {
        isBurning = true;
        StartCoroutine(BurnRoutine());
    }

    public void stopBurn()
    {
        isBurning = false;
    }

    private IEnumerator BurnRoutine()
    {
        // Khởi đầu cho người chơi vài giây chuẩn bị trước khi đốt
        yield return new WaitForSeconds(tickInterval);

        while (isBurning)
        {
            if (player != null)
            {
                // Gây sát thương
                //player.SendMessage("TakeDamage", damageTick, SendMessageOptions.DontRequireReceiver);
                Debug.Log("Player burn");
            }
            yield return new WaitForSeconds(tickInterval);
        }
    }
}
