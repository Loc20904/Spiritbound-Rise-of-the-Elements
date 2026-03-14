using UnityEngine;

public class IceZone : MonoBehaviour
{
    [SerializeField] private float slideSpeed = 8f;
    [SerializeField] private Vector2 slideDirection = Vector2.right;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody2D rb = other.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        rb.linearVelocity = new Vector2(slideDirection.x * slideSpeed, rb.linearVelocity.y);
    }
}