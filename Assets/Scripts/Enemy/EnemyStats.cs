using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public int maxHP = 50;
    public int currentHP;

    public int damage = 10;

    private void Awake()
    {
        currentHP = maxHP;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        Debug.Log(gameObject.name + " bị đánh: " + dmg + " | HP còn: " + currentHP);

        if (currentHP <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " chết!");
        Destroy(gameObject);
    }
}
