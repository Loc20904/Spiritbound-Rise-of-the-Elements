using UnityEngine;

public class StunObject : MonoBehaviour
{
    public float stunDuration = 2f;
    public GameObject stunEffect;
    public AudioClip SFX;
    public int damege = 10;

    void OnTriggerEnter2D(Collider2D other)
    {
        if (SFX) PlaySound(SFX);
        if (other.CompareTag("Player"))
        {
            other.GetComponent<PlayerStats>().TakeDamage(10);
        }
    }
    protected void PlaySound(AudioClip clip)
    {
        if (clip != null && SFXPool.Instance != null)
        {
            SFXPool.Instance.Play(clip, 0.5f, Random.Range(0.9f, 1.1f));
        }
    }
}