using UnityEngine;

public class NPCInteract : MonoBehaviour
{
    [Header("Giao diện Hướng dẫn")]
    [Tooltip("Kéo thả GameObject chứa chữ 'Nhấn F để nói chuyện' vào đây")]
    public GameObject interactUI;

    private bool isPlayerInRange = false;

    private void Start()
    {
        // Đảm bảo lúc mới vào game thì nút F bị ẩn đi
        if (interactUI != null)
        {
            interactUI.SetActive(false);
        }
    }

    // Khi Player bước VÀO vùng của NPC
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            if (interactUI != null) interactUI.SetActive(true); // Hiện nút F
        }
    }

    // Khi Player bước RA KHỎI vùng của NPC
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            if (interactUI != null) interactUI.SetActive(false); // Ẩn nút F
        }
    }
}