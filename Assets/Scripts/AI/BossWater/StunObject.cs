using UnityEngine;

public class StunObject : MonoBehaviour
{
    public float stunDuration = 2f;
    public GameObject stunEffect;
    public AudioClip SFX;

    void OnTriggerEnter2D(Collider2D other)
    {
        PlaySound(SFX);
        if (other.CompareTag("Player"))
        {

            //PlayerController playerScript = other.GetComponent<PlayerController>();

            //    if (playerScript != null)
            //    {
            //        playerScript.ApplyStun(stunDuration, stunEffect);
            //    }

            //    Destroy(gameObject);
            //}
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