using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    [Header("Bullet Stats")]
    public float speed = 8f;
    public float lifeTime = 3f;
    public float damage = 5f;

    [Header("Hit Settings")]
    public LayerMask obstacleLayer;   // layer tường / ground

    private Vector2 moveDir;

    public void Init(Vector2 dir)
    {
        moveDir = dir.normalized;

        float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;

        // thử 0, 90, -90, 180
        transform.rotation = Quaternion.Euler(0f, 0f, angle + 180f);

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Trúng player
        if (other.CompareTag("Player"))
        {
            other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            Destroy(gameObject);
            return;
        }

        // Trúng vật cản / mặt đất / tường
        if (((1 << other.gameObject.layer) & obstacleLayer) != 0)
        {
            Destroy(gameObject);
        }
    }
}