using UnityEngine;

public class TrapButton : MonoBehaviour
{
    [SerializeField] private TrapWallMove trapWall;
    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggered) return;

        if (collision.CompareTag("Player"))
        {
            triggered = true;
            Debug.Log("Player đã chạm button, tường gai bắt đầu chạy!");

            trapWall.StartTrap();
        }
    }
}