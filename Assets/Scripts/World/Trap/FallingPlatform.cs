using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    [SerializeField] private float fallDelay = 1f; // Time before the platform starts falling
    //[SerializeField] private float destroyDelay = 2f; // Time before the platform is destroyed after falling
    [SerializeField] Transform pointRespawn;
    private bool falling = false;

    [SerializeField] private Rigidbody2D rb;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.tag == "Player" && !falling)
        {
            StartCoroutine(StartFall());
        }
    }
    private IEnumerator StartFall()
    {
        falling = true;

        yield return new WaitForSeconds(fallDelay);

        rb.bodyType = RigidbodyType2D.Dynamic; // Make the platform fall
        //Destroy(gameObject, destroyDelay); // Destroy the platform after a delay
        Invoke("Respawn", 2f);
    }

    private void Respawn()
    {
        rb.bodyType = RigidbodyType2D.Kinematic; // Make the platform static again
        rb.linearVelocity = Vector2.zero;
        rb.angularDamping = 0;
        transform.position = pointRespawn.position; // Move the platform back to the respawn point
        falling = false;
    }
}
