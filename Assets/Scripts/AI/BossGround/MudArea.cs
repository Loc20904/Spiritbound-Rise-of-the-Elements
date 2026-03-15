using UnityEngine;

public class EnvironmentMud : MonoBehaviour
{
    [Header("Global Mud Settings")]
    [Tooltip("Hệ số tốc độ cho toàn bộ map (0.5f = giảm 50% tốc độ)")]
    public float slowMultiplier = 0.5f;

    private PlayerMovement playerMove;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMove = player.GetComponent<PlayerMovement>();
            if (playerMove != null)
            {
                // Áp dụng chậm cho toàn scene
                playerMove.speedMultiplier = slowMultiplier;
            }
        }
    }

    void OnDestroy()
    {
        // Khi rời scene hoặc tắt script, phục hồi tốc độ
        if (playerMove != null)
        {
            playerMove.speedMultiplier = 1f;
        }
    }
}
