using System.Collections;
using UnityEngine;

// Không gắn script này trực tiếp vào Boss, mà sẽ gắn các script con (Fire, Ice, Wind...)
public abstract class BossAttackBase : MonoBehaviour
{
    [Header("Base Stats")]
    public float attackRatePhase1 = 3f;
    public float attackRatePhase2 = 2.5f;

    [Header("Base Audio")]
    public AudioClip castSound;
    public AudioClip shootSound;
    [Range(0f, 1f)] public float soundVolume = 0.5f;

    protected float nextAttackTime = 0f;
    protected bool isPhase2 = false;

    // References
    protected Animator anim;
    protected Transform player;
    protected AudioSource audioSource; // Nếu cần dùng trực tiếp

    protected virtual void Awake() // Dùng virtual để con có thể override nếu cần
    {
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (GameObject.FindGameObjectWithTag("Player"))
            player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void SetPhase(bool phase2)
    {
        isPhase2 = phase2;
    }

    public void Attack()
    {
        if (this.enabled == false || player == null) return;
        if (Time.time < nextAttackTime) return;

        // Gọi Coroutine tấn công (Logic cụ thể sẽ nằm ở script con)
        StartCoroutine(PerformAttackRoutine());

        float cooldown = isPhase2 ? attackRatePhase2 : attackRatePhase1;
        nextAttackTime = Time.time + cooldown;
    }

    // --- HÀM TRỪU TƯỢNG: Bắt buộc script con phải tự viết nội dung ---
    protected abstract IEnumerator PerformAttackRoutine();

    // --- CÁC HÀM HỖ TRỢ DÙNG CHUNG (Helper) ---
    protected void PlaySound(AudioClip clip)
    {
        if (clip != null && SFXPool.Instance != null)
        {
            SFXPool.Instance.Play(clip, soundVolume, Random.Range(0.9f, 1.1f));
        }
    }

    // Hàm xoay mặt về player
    protected void FacePlayer()
    {
        if (player == null) return;
        Vector3 scale = transform.localScale;
        scale.x = (player.position.x > transform.position.x) ? -1 : 1;
        transform.localScale = scale;
    }
}