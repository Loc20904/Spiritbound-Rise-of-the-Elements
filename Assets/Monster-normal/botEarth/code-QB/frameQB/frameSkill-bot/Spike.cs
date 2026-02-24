using UnityEngine;

public class Spike : MonoBehaviour
{
    public Sprite[] frames;
    public float frameRate = 0.07f;
    public int damage = 1;

    private SpriteRenderer sr;

    float timer;
    int index;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= frameRate)
        {
            timer = 0;

            if (index < frames.Length)
            {
                sr.sprite = frames[index];
                index++;
            }
            else
            {
                Destroy(gameObject); // xong animation thì tự hủy
            }
        }
    }

    // gây damage
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damage, transform.position);
            }
        }
    }
        
    
}
