using UnityEngine;

public class jumpPad : MonoBehaviour
{
    [SerializeField] private float bounceForce = 10f; // Force applied to the player when they jump on the pad

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            collision.gameObject.GetComponent<Rigidbody2D>().AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
        }
    }
}
