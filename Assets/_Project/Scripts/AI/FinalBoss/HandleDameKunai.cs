using UnityEngine;

public class HandleDameKunai : MonoBehaviour
{
    public float damage = 10f;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Gây sát thương cho Player
            PlayerStats.Resolve(other)?.TakeDamage((int)damage);
        }
    }
}
