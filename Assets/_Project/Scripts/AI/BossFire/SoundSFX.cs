using System.Collections.Generic;
using UnityEngine;

public class SFXPool : MonoBehaviour
{
    public static SFXPool Instance;

    [Header("Pool Setting")]
    public int poolSize = 12;   // 12-20 là phù hợp cho Boss + Player
    private List<AudioSource> sources;

    void Awake()
    {
        // --- NGĂN TẠO 2 INSTANCE ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // --- GIỮ LẠI KHI ĐỔI SCENE (nếu muốn global) ---
        DontDestroyOnLoad(gameObject);

        // Khởi tạo audio pool
        sources = new List<AudioSource>();
        for (int i = 0; i < poolSize; i++)
        {
            AudioSource src = gameObject.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f;   // 2D sound
            sources.Add(src);
        }
    }

    // Hàm phát âm
    public void Play(AudioClip clip, float volume = 0.1f, float pitch = 1f)
    {
        if (clip == null) return;

        foreach (var src in sources)
        {
            if (!src.isPlaying)
            {
                src.pitch = pitch;
                src.volume = volume;
                src.clip = clip;
                src.Play();
                return;
            }
        }

        // fallback
        var first = sources[0];
        first.pitch = pitch;
        first.volume = volume;
        first.clip = clip;
        first.Play();
    }

}
