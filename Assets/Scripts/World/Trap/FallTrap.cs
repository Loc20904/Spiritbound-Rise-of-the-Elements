using UnityEngine;

public class FallTrap : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isFalling = false;
    [SerializeReference] private Transform respawn;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !isFalling)
        {
            isFalling = true;
            rb.bodyType = RigidbodyType2D.Dynamic; // Make the trap fall
            Invoke("Respawn", 2f); // Schedule the trap to respawn after 2 seconds
        }
    }

    private void Respawn()
    {
        rb.bodyType = RigidbodyType2D.Kinematic; // Make the trap static again
        rb.linearVelocity = Vector2.zero;
        transform.position = respawn.position; // Move the trap back to the respawn point
        isFalling = false;
    }

}
