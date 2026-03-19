using UnityEngine;

public class SlashProjectile : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 1.2f;

    [Header("Damage")]
    [SerializeField] private int damage = 2;

    private int direction = 1;

    public void Init(int dir, int dmg)
    {
        direction = dir >= 0 ? 1 : -1;
        damage = dmg;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * -direction; // đảo lại
        transform.localScale = scale;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += Vector3.right * direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) return;

        EnemyBase enemy = other.GetComponentInParent<EnemyBase>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        other.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);

        if (((1 << other.gameObject.layer) & LayerMask.GetMask("Enemy", "Boss")) != 0)
        {
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}