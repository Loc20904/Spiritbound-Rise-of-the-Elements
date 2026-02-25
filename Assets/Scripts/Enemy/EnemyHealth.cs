using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] private int maxHp = 3;
    private int hp;

    private void Awake()
    {
        hp = maxHp;
    }

    public void TakeDamage(int dmg)
    {
        hp -= dmg;
        Debug.Log($"{name} took {dmg} damage. HP: {hp}");

        if (hp <= 0)
            Die();
    }

    private void Die()
    {
        // TODO: play animation / drop item / effect...
        Vector3 deadPos = transform.position;
        var drop = GetComponent<EnemyDropTable>();
        if (drop != null) drop.TryDrop(deadPos);
        Destroy(gameObject);
    }
}
