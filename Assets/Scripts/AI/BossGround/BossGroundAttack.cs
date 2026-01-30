using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGroundAttack : BossAttackBase
{
    [Header("Dependencies")]
    public BossGroundMovement movement;
    public BossHealth health;
    // Lưu ý: Biến 'anim' đã có trong BossAttackBase, không khai báo lại ở đây.

    [Header("Prefabs sword")]
    public List<GameObject> SwordPrefab;

    [Header("Melee Settings")]
    public Transform attackPoint;
    public float meleeRange = 2f;
    public float meleeDamage = 10f;
    public LayerMask playerLayer;

    [Header("Animation Timings (Chỉnh giây tại đây)")]
    [Tooltip("Thời gian animation Attack 1 (Lấy Time gốc / Speed)")]
    public float attack1Duration = 0.8f;

    [Tooltip("Thời gian animation Attack 2")]
    public float attack2Duration = 0.8f;

    [Tooltip("Thời gian animation Attack 3")]
    public float attack3Duration = 1.2f;

    [Tooltip("Thời gian gồng trước khi Dash")]
    public float dashPrepareTime = 1.0f;

    [Header("Skill 1: Shield (Phase 2)")]
    public GameObject shieldVFX;
    public float healPercent = 5f;

    [Header("Skill 2: Stun Spell (Phase 2)")]
    public GameObject stunProjectilePrefab;
    public Transform castPoint;

    [Header("Skill 3: Dash (Skill chủ động)")]
    public float dashDuration = 0.5f;

    private bool isBusy = false;

    [Header("Skill Cooldowns & AI Settings")]
    public float shieldCooldown = 10f; // Hồi chiêu khiên (10s)
    public float stunCooldown = 3f;    // Hồi chiêu kiếm (3s)

    // Biến nội bộ để đếm giờ
    private float nextShieldTime = 0f;
    private float nextStunTime = 0f;

    // ---------------------------------------------------------
    // INIT & UPDATE
    // ---------------------------------------------------------

    protected override void Awake()
    {
        base.Awake(); // Gọi hàm cha

        movement = GetComponent<BossGroundMovement>();
        health = GetComponent<BossHealth>();

        // Đảm bảo biến anim của cha không bị null
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Nếu đang đánh (isBusy) hoặc Player chết -> Không di chuyển theo AI
        if (isBusy || player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // Logic di chuyển cơ bản
        if (dist > meleeRange)
        {
            movement.MoveToPlayer();
        }
        else
        {
            movement.Stop();
        }
    }

    // ---------------------------------------------------------
    // ATTACK LOGIC
    // ---------------------------------------------------------

    protected override IEnumerator PerformAttackRoutine()
    {
        if (isBusy) yield break;
        isBusy = true;

        bool actionDone = false;
        float distToPlayer = Vector2.Distance(transform.position, player.position);

        // --- GIAI ĐOẠN 2 (PHASE 2 - SMART AI) ---
        if (isPhase2)
        {
            // 1. ƯU TIÊN SỐ 1: SHIELD (Hồi máu/Giáp) - CD 10s
            // Logic: Nếu đã hồi chiêu -> 70% tỉ lệ sẽ dùng ngay (để không quá spam nếu vừa hết CD)
            if (Time.time >= nextShieldTime && Random.value < 0.7f)
            {
                yield return StartCoroutine(SkillShieldRegen());

                // Đặt thời gian hồi chiêu cho lần tới
                nextShieldTime = Time.time + shieldCooldown;
                actionDone = true;
            }

            // 2. ƯU TIÊN SỐ 2: STUN SWORD (Kiếm bay) - CD 3s
            // Logic: Nếu Shield chưa dùng được, kiểm tra Kiếm.
            // Nếu Player ở xa (> 3m) thì tỉ lệ dùng cao hơn (80%), ở gần thì ít dùng hơn (40%)
            else if (Time.time >= nextStunTime)
            {
                bool shouldCast = (distToPlayer > 3f) ? (Random.value < 0.8f) : (Random.value < 0.4f);

                if (shouldCast)
                {
                    yield return StartCoroutine(SkillSummonStun());

                    // Đặt thời gian hồi chiêu
                    nextStunTime = Time.time + stunCooldown;
                    actionDone = true;
                }
            }

            // 3. ƯU TIÊN SỐ 3: DASH (Húc)
            // Logic: Nếu không dùng skill nào ở trên, tỉ lệ nhỏ (20%) sẽ Húc
            if (!actionDone && Random.value < 0.2f)
            {
                yield return StartCoroutine(DashAttack());
                actionDone = true;
            }
        }
        // --- GIAI ĐOẠN 1 (PHASE 1 - CƠ BẢN) ---
        else
        {
            // Phase 1 chỉ thỉnh thoảng húc (15%)
            if (Random.value < 0.15f)
            {
                yield return StartCoroutine(DashAttack());
                actionDone = true;
            }
        }

        // --- HÀNH ĐỘNG MẶC ĐỊNH: ĐÁNH THƯỜNG (MELEE) ---
        // Nếu AI quyết định không dùng Skill nào (hoặc đang CoolDown hết)
        if (!actionDone)
        {
            if (distToPlayer <= meleeRange)
            {
                yield return StartCoroutine(MeleeCombo());
            }
            else
            {
                // Nếu ở xa mà hết skill -> Boss sẽ nghỉ một chút để script Movement (Update) chạy lại gần Player
                isBusy = false;
                nextAttackTime = Time.time + 0.1f;
                yield break;
            }
        }

        // Nghỉ mệt sau khi thực hiện hành động
        yield return new WaitForSeconds(0.5f);
        isBusy = false;
    }

    // ---------------------------------------------------------
    // SKILLS IMPLEMENTATION
    // ---------------------------------------------------------

    IEnumerator MeleeCombo()
    {
        movement.SetMove(false); // Khóa di chuyển

        // --- HIT 1 ---
        movement.FacePlayer();
        anim.SetTrigger("attack1");
        yield return new WaitForSeconds(attack1Duration); // Chờ tay
        anim.ResetTrigger("attack1");

        // --- HIT 2 ---
        movement.FacePlayer(); // Quay mặt lại cho chuẩn hướng
        anim.SetTrigger("attack2");
        yield return new WaitForSeconds(attack2Duration); // Chờ tay
        anim.ResetTrigger("attack2");

        // --- HIT 3 ---
        movement.FacePlayer();
        anim.SetTrigger("attack3");
        yield return new WaitForSeconds(attack3Duration); // Chờ tay
        anim.ResetTrigger("attack3");

        movement.SetMove(true); // Mở lại di chuyển
    }

    IEnumerator DashAttack()
    {
        movement.Stop();
        movement.SetMove(false);

        // 1. Chuẩn bị (Gồng)
        movement.FacePlayer();
        anim.SetTrigger("dashPrepare");
        yield return new WaitForSeconds(dashPrepareTime);

        // 2. Lao đi
        movement.FacePlayer(); // Cập nhật hướng lần cuối trước khi lao
        anim.SetTrigger("dashGo");

        movement.DashForward(dashDuration);
        yield return new WaitForSeconds(dashDuration);

        // 3. Kết thúc
        movement.Stop();
        yield return new WaitForSeconds(0.5f); // Nghỉ sau khi húc

        movement.SetMove(true);
    }

    // --- CÁC SKILL PHỤ (Placeholder - Bạn dán code cũ của bạn vào đây nếu có) ---

    IEnumerator SkillShieldRegen()
    {
        movement.Stop();
        if (shieldVFX) shieldVFX.SetActive(true);
        // Debug.Log("Boss: Kích hoạt khiên - Bắt đầu hồi chiêu 10s");

        // Thời gian đứng hồi máu
        yield return new WaitForSeconds(2f); // Giảm xuống 2s cho đỡ lù đù (5s hơi lâu)

        if (health) health.Heal(healPercent);
        if (shieldVFX) shieldVFX.SetActive(false);
    }

    IEnumerator SkillSummonStun()
    {
        movement.Stop();
        movement.FacePlayer();
        anim.SetTrigger("skillCast");

        System.Collections.Generic.List<StunProjectile> projectiles = new System.Collections.Generic.List<StunProjectile>();

        Vector3[] spawnOffsets = {
            new Vector3(-1.5f, 1f, 0),
            new Vector3(0f, 2.5f, 0),
            new Vector3(1.5f, 1f, 0)
        };

        // --- GIAI ĐOẠN 1: TRIỆU HỒI ---
        if (SwordPrefab != null && SwordPrefab.Count > 0 && castPoint)
        {
            for (int i = 0; i < 3; i++)
            {
                int index = i % SwordPrefab.Count;
                GameObject selectedSword = SwordPrefab[index];

                // QUAN TRỌNG: Instantiate với parent là NULL (không làm con Boss)
                // Để kiếm không bị ảnh hưởng bởi Scale của Boss
                GameObject projObj = Instantiate(selectedSword, castPoint.position, Quaternion.identity, null);

                StunProjectile script = projObj.GetComponent<StunProjectile>();
                if (script != null)
                {
                    projectiles.Add(script);
                }
            }
        }

        // --- GIAI ĐOẠN 2: CHỜ & BÁM THEO THỦ CÔNG ---
        float timer = 0f;
        float prepareTime = 2f;

        while (timer < prepareTime)
        {
            if (player != null)
            {
                // Lấy hướng quay mặt của Boss (1 là phải, -1 là trái)
                // Giả sử Boss quay mặt bằng cách chỉnh Scale X
                float facingDir = Mathf.Sign(transform.localScale.x);

                // Dùng vòng lặp for để lấy được index 'i' (để biết kiếm nào đi với vị trí nào)
                for (int i = 0; i < projectiles.Count; i++)
                {
                    StunProjectile script = projectiles[i];
                    if (script == null) continue;

                    // 1. CẬP NHẬT VỊ TRÍ (Thủ công)
                    // Tính vị trí mong muốn: CastPoint + Offset (nhân với hướng quay để đảo bên)
                    Vector3 targetPos = castPoint.position;
                    targetPos.x += spawnOffsets[i].x * facingDir; // Đảo offset theo hướng Boss
                    targetPos.y += spawnOffsets[i].y;             // Giữ nguyên độ cao

                    // Gán vị trí (Kiếm sẽ dính chặt vào vị trí này dù không phải là con)
                    script.transform.position = targetPos;

                    // 2. XOAY MŨI KIẾM
                    Vector2 dir = (player.position - script.transform.position).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    script.transform.rotation = Quaternion.Lerp(script.transform.rotation, Quaternion.Euler(0, 0, angle), Time.deltaTime * 5f);
                }
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // --- GIAI ĐOẠN 3: PHÓNG KIẾM ---
        foreach (StunProjectile script in projectiles)
        {
            if (script != null && player != null)
            {
                script.Launch(player);
            }
            yield return new WaitForSeconds(0.15f);
        }
        anim.SetTrigger("endCast");
    }

    // Vẽ vòng tròn tầm đánh để dễ chỉnh trong Scene
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, meleeRange);
    }

    protected override IEnumerator SkillUtimateUlti()
    {
        throw new System.NotImplementedException();
    }
}