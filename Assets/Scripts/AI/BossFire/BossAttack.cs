using System.Collections;
using UnityEngine;

// Tự động thêm AudioSource vào GameObject nếu chưa có
[RequireComponent(typeof(AudioSource))]
public class BossAttack : MonoBehaviour
{
    [Header("Stats")]
    public float attackRatePhase1 = 3f;
    public float attackRatePhase2 = 2.5f;

    [Header("Spells")]
    public GameObject fireballPrefab;
    public GameObject specialPrefab;
    public Transform firePoint;

    [Header("Audio Settings")] // --- MỚI THÊM ---
    public AudioClip castSound;   // Tiếng niệm chú (gầm gừ hoặc tiếng phép thuật)
    public AudioClip shootSound;  // Tiếng đạn bay ra (vút, pew pew)
    [Range(0f, 1f)] public float soundVolume = 0.5f;

    private AudioSource audioSource; // --- MỚI THÊM ---

    private float nextAttackTime = 0f;
    private bool isPhase2 = false;

    // References
    private Animator anim;
    private Transform player;

    void Awake()
    {
        anim = GetComponent<Animator>();
        // Lấy AudioSource để phát nhạc
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
        if (this.enabled == false) return;
        if (player == null) return;
        if (Time.time < nextAttackTime) return;

        StartCoroutine(PerformAttackRoutine());

        float cooldown = isPhase2 ? attackRatePhase2 : attackRatePhase1;
        nextAttackTime = Time.time + cooldown;
    }

    // ---------------------------------------------------------
    // LOGIC TẤN CÔNG ĐA DẠNG
    // ---------------------------------------------------------
    IEnumerator PerformAttackRoutine()
    {
        // Xoay mặt về hướng player trước khi bắn
        Vector3 scale = transform.localScale;
        scale.x = (player.position.x > transform.position.x) ? -1 : 1;
        transform.localScale = scale;

        // --- PHASE 1 ---
        if (!isPhase2)
        {
            if (Random.value > 0.3f)
            {
                // Bắn chùm
                anim.SetTrigger("fastFireball");
                PlaySound(castSound); // Phát tiếng niệm chú
                yield return StartCoroutine(BurstFire(3, 0.2f, fireballPrefab));
            }
            else
            {
                // Bắn đạn to
                anim.SetTrigger("fastFireball");
                PlaySound(castSound); // Phát tiếng niệm chú
                yield return new WaitForSeconds(0.8f);
                SpawnSpell(specialPrefab ?? fireballPrefab, 0);
            }
        }
        // --- PHASE 2 ---
        else
        {
            int rand = Random.Range(0, 3);
            switch (rand)
            {
                case 0: // Shotgun
                    anim.SetTrigger("fastFireball");
                    PlaySound(castSound);
                    yield return new WaitForSeconds(0.3f);
                    // Bắn 5 viên cùng lúc -> Chỉ nên phát 1 tiếng bắn to hoặc phát riêng lẻ
                    SpawnSpell(fireballPrefab, 0);
                    SpawnSpell(fireballPrefab, 15);
                    SpawnSpell(fireballPrefab, -15);
                    SpawnSpell(fireballPrefab, 30);
                    SpawnSpell(fireballPrefab, -30);
                    break;

                case 1: // Machine Gun
                    anim.SetTrigger("fastFireball");
                    PlaySound(castSound);
                    yield return StartCoroutine(BurstFire(5, 0.15f, fireballPrefab));
                    break;

                case 2: // Mưa đạn (Fire Rain)
                    anim.SetTrigger("fireRain");
                    PlaySound(castSound); // Tiếng gọi mưa
                    yield return new WaitForSeconds(0.5f);

                    Vector2 screenTopLeft = Camera.main.ViewportToWorldPoint(new Vector3(0, 1, 0));
                    Vector2 screenTopRight = Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0));
                    float startY = screenTopLeft.y + 1f;
                    int meteorCount = 10;

                    for (int i = 0; i < meteorCount; i++)
                    {
                        if (player == null) break;

                        float randomX = Random.Range(screenTopLeft.x + 0.5f, screenTopRight.x - 0.5f);
                        Vector3 spawnPos = new Vector3(randomX, startY, 0);

                        GameObject rain = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
                        rain.transform.rotation = Quaternion.Euler(0, 0, -90);

                        // Phát tiếng bắn cho từng viên mưa rơi
                        PlaySound(shootSound);

                        yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
                    }
                    break;
            }
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator BurstFire(int count, float delay, GameObject ammo)
    {
        for (int i = 0; i < count; i++)
        {
            if (player == null) break;
            SpawnSpell(ammo, 0);
            yield return new WaitForSeconds(delay);
        }
    }

    void SpawnSpell(GameObject prefab, float angleOffset)
    {
        if (prefab == null || firePoint == null) return;
        if (player == null) return;

        GameObject spell = Instantiate(prefab, firePoint.position, Quaternion.identity);

        // Phát âm thanh bắn tại đây
        PlaySound(shootSound);

        Vector2 dir = (player.position - firePoint.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + angleOffset;
        spell.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // --- HÀM HỖ TRỢ PHÁT ÂM THANH ---
    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;

        SFXPool.Instance.Play(
            clip,
            soundVolume,
            Random.Range(0.9f, 1.1f)
        );
    }

}