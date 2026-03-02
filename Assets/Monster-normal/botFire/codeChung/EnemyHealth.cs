    using UnityEngine;
    using System.Collections;

    public class EnemyHealth : MonoBehaviour
    {
        public int maxHP = 200;
        int currentHP;

        [Header("Death Animation")]
        public Sprite[] deathFrames;
        public float deathFrameRate = 0.08f;

        [Header("Hit Sound")]
        public AudioClip hitSound;   // kéo file sound vào đây trong Inspector
        AudioSource audioSource;

        bool isDead;

        SpriteRenderer sr;
        Rigidbody2D rb;
        Collider2D col;
        RangerBotFly ranger;
   
        QuaiAI ai;
        QuaiAI_QCC ai1;
        EnemyChaseAI chase;
        EnemyMeleeAttack melee;
        EnemyRangedAttack ranged;
        EnemyExploder exploder;
        BotFlyAI fly;

        void Awake()
        {
            currentHP = maxHP;
            fly = GetComponent<BotFlyAI>();
            sr = GetComponent<SpriteRenderer>();
            rb = GetComponent<Rigidbody2D>();
            col = GetComponent<Collider2D>();
            ranger = GetComponent<RangerBotFly>();
            ai1 = GetComponent<QuaiAI_QCC>();
            ai = GetComponent<QuaiAI>();
            chase = GetComponent<EnemyChaseAI>();
            melee = GetComponent<EnemyMeleeAttack>();
            ranged = GetComponent<EnemyRangedAttack>();
            exploder = GetComponent<EnemyExploder>();
            audioSource = GetComponent<AudioSource>();
        }

        public void TakeDamage(int dmg)
        {
            if (isDead) return;

            currentHP -= dmg;
            PlayHitSound();
            // ⭐ bị đánh → quay mặt player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player)
            {
                bool playerRight = player.transform.position.x > transform.position.x;

                if (ai != null)
                {
                    if (playerRight != ai.FacingRight)
                        ai.FlipPublic();
                }

                if (ai1 != null)
                {
                    if (playerRight != ai1.FacingRight)
                        ai1.FlipPublic();
                }
            }

            if (currentHP <= 0)
            {
                DieImmediate();
            }
        }

        // =====================================================
        // DIE
        // =====================================================
        void DieImmediate()
        {
            isDead = true;

            // ⭐ CẮT TOÀN BỘ ATTACK NGAY
            if (melee)
            {
                melee.StopAllCoroutines();
                melee.enabled = false;
            }

            if (ranged)
            {
                ranged.StopAllCoroutines();
                ranged.enabled = false;
            }

            // ⭐ CẮT AI + CHASE
            if (ai) ai.enabled = false;
            if (ai1) ai1.enabled = false;
            if (chase) chase.enabled = false;

            // ⭐ VẬT LÝ
            if (rb)
            {
                rb.linearVelocity = Vector2.zero;
                rb.simulated = false;
            }

            if (col) col.enabled = false;

            
            if (fly)
            {
                fly.StopAllCoroutines();
                fly.enabled = false;
            }

            if (ranger)
            {
                ranger.StopAllCoroutines();
                ranger.enabled = false;
            }

            if (exploder)
            {
                exploder.StopAllCoroutines();
                exploder.enabled = false;
            }
            // ⭐ ĐẢM BẢO KHÔNG CÒN SCRIPT NÀO SET SPRITE
            StopAllCoroutines();

            StartCoroutine(DeathAnimation());
        }
        public void PlayHitSound()
        {
            if (hitSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }

    IEnumerator DeathAnimation()
        {
            for (int i = 0; i < deathFrames.Length; i++)
            {
                sr.sprite = deathFrames[i];
                yield return new WaitForSeconds(deathFrameRate);
            }

            Destroy(gameObject); // tuỳ bạn
        }
    }
