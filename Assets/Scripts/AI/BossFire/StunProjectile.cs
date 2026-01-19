using UnityEngine;

public class StunProjectile : MonoBehaviour
{
    public float speed = 12f;
    public float stunDuration = 1.5f; // Thời gian choáng

    void Start()
    {
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Gây Damage
            // other.GetComponent<PlayerHealth>().TakeDamage(10);

            // 2. Gây Choáng (Giả sử Player có script PlayerMovement)
            /* var playerMove = other.GetComponent<PlayerMovement>();
            if (playerMove != null) {
                playerMove.Stun(stunDuration); // Bạn cần viết hàm Stun bên Player
            }
            */
            Debug.Log("Player bị choáng!");

            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }
}