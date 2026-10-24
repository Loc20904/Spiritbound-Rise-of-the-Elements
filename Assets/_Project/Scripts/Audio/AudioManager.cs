using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] public AudioSource musicAudioSource;

    [SerializeField] public AudioClip musicClips;

    void Start()
    {
        musicAudioSource.clip = musicClips;
        musicAudioSource.Play();
    }
}
