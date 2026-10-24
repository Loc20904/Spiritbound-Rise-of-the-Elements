using UnityEngine;

public class EnvironmentIce : MonoBehaviour
{
    private PlayerMovement playerMove;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerMove = player.GetComponent<PlayerMovement>();
            if (playerMove != null)
            {
                // Áp dụng trượt cho toàn map
                playerMove.isSlippery = true;
            }
        }
    }

    void OnDestroy()
    {
        // Trả lại bình thường
        if (playerMove != null)
        {
            playerMove.isSlippery = false;
        }
    }
}
