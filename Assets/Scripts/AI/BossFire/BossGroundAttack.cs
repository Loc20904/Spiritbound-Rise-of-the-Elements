using System.Collections;
using UnityEngine;

public class BossGroundAttack : BossAttackBase
{
    [Header("Dependencies")]
    public BossGroundMovement movement;
    public BossHealth health;

    [Header("Melee Settings")]
    public Transform attackPoint;
    public float meleeRange = 2f;
    public float meleeDamage = 10f;
    public LayerMask playerLayer;

    [Header("Skill 1: Shield (Phase 2)")]
    public GameObject shieldVFX;
    public float healPercent = 0.05f;

    [Header("Skill 2: Stun Spell (Phase 2)")]
    public GameObject stunProjectilePrefab;
    public Transform castPoint;

    [Header("Skill 3: Dash (Skill chủ động)")]
    public float dashDuration = 0.5f;

    // Biến kiểm soát trạng thái để không vừa chạy vừa đánh
    private bool isBusy = false;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<BossGroundMovement>();
        health = GetComponent<BossHealth>();
    }

    // --- LOGIC DI CHUYỂN (Đã sửa đổi) ---
    void Update()
    {
        // CHỈ DI CHUYỂN KHI KHÔNG BẬN (Không đang chém, không đang gồng skill)
        if (!isBusy && player != null)
        {
            float dist = Vector2.Distance(transform.position, player.position);

            // Logic tiếp cận: Chỉ chạy bộ
            if (dist > meleeRange)
            {
                movement.MoveToPlayer();
            }
            else
            {
                movement.Stop();
            }
        }
    }

    protected override IEnumerator PerformAttackRoutine()
    {
        // Nếu đang bận di chuyển hoặc làm gì đó thì bỏ qua nhịp này
        if (isBusy) yield break;

        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // Đánh dấu là đang bận tấn công -> Update sẽ dừng di chuyển
        isBusy = true;

        // ====================================================
        // QUYẾT ĐỊNH CHIÊU THỨC (AI LOGIC)
        // ====================================================

        bool actionDone = false; // Kiểm tra xem đã thực hiện hành động nào chưa

        // --- PHASE 2: Có full bộ kỹ năng ---
        if (isPhase2)
        {
            int rand = Random.Range(0, 100);

            if (rand < 20) // 20% Dựng khiên
            {
                yield return StartCoroutine(SkillShieldRegen());
                actionDone = true;
            }
            else if (rand < 40) // 20% Niệm phép Stun
            {
                yield return StartCoroutine(SkillSummonStun());
                actionDone = true;
            }
            else if (rand < 60) // 20% Dash (Skill lao vào)
            {
                yield return StartCoroutine(DashAttack());
                actionDone = true;
            }
            // 40% còn lại: Đánh thường (Rớt xuống logic bên dưới)
        }
        // --- PHASE 1: Chỉ có Đấm hoặc Dash ---
        else
        {
            // Ở Phase 1, tỷ lệ Dash thấp hơn (ví dụ 15%)
            if (Random.value < 0.15f)
            {
                yield return StartCoroutine(DashAttack());
                actionDone = true;
            }
        }

        // --- LOGIC ĐÁNH THƯỜNG (Nếu chưa dùng skill nào ở trên) ---
        if (!actionDone)
        {
            if (distToPlayer <= meleeRange)
            {
                // Nếu ở gần -> Đấm Combo
                yield return StartCoroutine(MeleeCombo());
            }
            else
            {
                // QUAN TRỌNG: Nếu ở xa mà không random trúng skill Dash
                // -> Thì KHÔNG LÀM GÌ CẢ. 
                // Kết thúc lệnh bận để Update() tiếp tục cho boss chạy bộ.
                isBusy = false;

                // Reset thời gian hồi chiêu về 0 để boss check hành động liên tục khi đang chạy
                // (Tránh trường hợp chạy đến nơi rồi đứng nhìn player 3s mới đánh)
                nextAttackTime = Time.time + 0.1f;
                yield break; // Thoát coroutine ngay
            }
        }

        // Sau khi đánh xong 1 chiêu, nghỉ một chút
        yield return new WaitForSeconds(0.5f);

        isBusy = false; // Hết bận, cho phép di chuyển lại
    }

    // --------------------------------------------------------
    // CÁC KỸ NĂNG (SKILLS)
    // --------------------------------------------------------

    IEnumerator MeleeCombo()
    {
        movement.Stop(); // Đảm bảo đứng yên khi chém

        // Hit 1
        anim.SetTrigger("attack1");
        CheckMeleeHit();
        yield return new WaitForSeconds(0.4f);

        // Hit 2
        anim.SetTrigger("attack2");
        CheckMeleeHit();
        yield return new WaitForSeconds(0.4f);

        // Hit 3
        anim.SetTrigger("attack3");
        CheckMeleeHit();
        yield return new WaitForSeconds(0.8f);
    }

    void CheckMeleeHit()
    {
        Collider2D hitPlayer = Physics2D.OverlapCircle(attackPoint.position, 1f, playerLayer);
        if (hitPlayer != null)
        {
            Debug.Log("Hit Player!");
            // Gọi hàm trừ máu player tại đây
            // var pHealth = hitPlayer.GetComponent<PlayerHealth>();
            // if(pHealth) pHealth.TakeDamage(meleeDamage);
        }
        base.PlaySound(shootSound);
    }

    IEnumerator SkillShieldRegen()
    {
        movement.Stop();
        anim.SetTrigger("skillShield");

        GameObject shield = null;
        if (shieldVFX) shield = Instantiate(shieldVFX, transform);

        float duration = 3f;
        health.isInvulnerable = true;

        for (int i = 0; i < 3; i++) // Lặp 3 lần (3 giây)
        {
            yield return new WaitForSeconds(1f);
            float healAmount = health.maxHP * healPercent;
            health.Heal(healAmount);
        }

        health.isInvulnerable = false;
        if (shield) Destroy(shield);
    }

    IEnumerator SkillSummonStun()
    {
        movement.Stop();
        anim.SetTrigger("skillCast");
        base.PlaySound(castSound);

        yield return new WaitForSeconds(2f); // Thời gian niệm

        for (int i = 0; i < 3; i++)
        {
            if (player == null) break;

            GameObject spell = Instantiate(stunProjectilePrefab, castPoint.position, Quaternion.identity);

            Vector2 dir = (player.position - castPoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            spell.transform.rotation = Quaternion.Euler(0, 0, angle);

            base.PlaySound(shootSound);
            yield return new WaitForSeconds(0.3f);
        }
    }

    IEnumerator DashAttack()
    {
        // Skill này cho phép boss tự lao đến kể cả khi đang ở xa
        anim.SetTrigger("dashPrepare");
        movement.Stop();
        yield return new WaitForSeconds(0.5f); // Gồng lâu hơn chút để player kịp né

        anim.SetTrigger("dashGo");
        movement.DashForward(dashDuration); // Script movement sẽ lo việc lao đi và gây choáng

        yield return new WaitForSeconds(dashDuration);
        movement.Stop();
    }
}