using UnityEngine;

public class TrapWallMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Transform endPoint;

    private bool isMoving = false;

    public void StartTrap()
    {
        isMoving = true;
    }

    private void Update()
    {
        if (!isMoving) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            endPoint.position,
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(transform.position, endPoint.position) < 0.05f)
        {
            isMoving = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player bị tường gai đâm!");

            // Nếu có hàm chết thì gọi:
            // collision.gameObject.GetComponent<PlayerLife>()?.Die();

            // test nhanh:
            collision.gameObject.SetActive(false);
        }
    }
}