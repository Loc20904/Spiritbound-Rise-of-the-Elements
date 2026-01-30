using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossWaterAttack : BossAttackBase
{
    public float worldMinX = -12f;
    public float worldMaxX = 12f;

    [Header("Dependencies")]
    public BossWaterMovement movement;

    [Header("Combat Settings")]
    public float attackRange = 10f;
    public float restTime = 1.5f;


    /// <summary>
    /// Skill 1: HYDRO PUMP
    /// </summary>
    [Header("Skill 1: Hydro Pump")]
    public Transform beamPivot;
    public GameObject beamVisual;
    public GameObject waterGun;

    public float angleOffset = 0f;
    public float beamDuration = 2.0f;
    public float aimDuration = 1.0f;
    public float sweepSpeed = 30f;

    /// <summary>
    /// Skill 2: GEYSER
    /// </summary>
    [Header("Skill 2: Geyser")]
    public GameObject geyserWarningPrefab;
    public GameObject geyserDamagePrefab;
    public float warningTime = 1.2f;
    public AudioClip castSkill2;

    /// <summary>
    /// SKILL 3: SUMMON MINIONS
    /// </summary>
    [Header("Skill 3: Summon Minions")]
    public List<GameObject> minionPrefabs;
    public Transform[] summonPoints;
    public int maxMinions = 6;

    /// <summary>
    /// SKILL ULTIMATE: WATER BARRAGE
    /// </summary>
    [Header("Skill Ultimate: Water Barrage")]
    public GameObject ultiProjectilePrefab; // Prefab viên đạn
    public Transform firePoint;             // Điểm bắn (miệng/tay)
    public int barrageCount = 10;           // Số lượng đạn bắn ra
    public float fireRate = 0.1f;           // Tốc độ bắn (càng nhỏ càng nhanh)
    public GameObject castUlti;

    public AudioClip UlticastSound;

    // ---------------------------------------------------------
    // INTERNAL VARIABLES
    // ---------------------------------------------------------
    private bool isBusy = false;
    private float nextTeleportTime = 0f;

    protected override void Awake()
    {
        base.Awake();
        movement = GetComponent<BossWaterMovement>();

        if (anim == null) anim = GetComponent<Animator>();

        if (beamVisual) beamVisual.SetActive(false);
    }

    /// <summary>
    /// core attack routine
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator PerformAttackRoutine()
    {
        if (isBusy || player == null) yield break;
        isBusy = true;

        float dist = Vector2.Distance(transform.position, player.position);

        // 1. CHECK TELEPORT (Giữ khoảng cách)
        // Nếu Player lại quá gần (< 4m) HOẶC đến giờ dịch chuyển định kỳ
        if (dist < 4f || Time.time > nextTeleportTime)
        {
            if (movement != null)
            {
                yield return StartCoroutine(movement.TeleportRandomly());
            }
            // Reset thời gian dịch chuyển (Random 6-10s)
            nextTeleportTime = Time.time + Random.Range(6f, 10f);
        }

        // 2. CHỌN SKILL (AI Decision)
        float rand = Random.value;
        bool skillPerformed = false;

        // --- PHASE 2 (Máu < 50%) ---
        if (isPhase2)
        {
            // Tăng tần suất AOE và Summon
            if (rand < 0.4f)
            {
                yield return StartCoroutine(SkillHydroPump()); // 40% Vòi rồng
                skillPerformed = true;
            }
            else if (rand < 0.7f)
            {
                yield return StartCoroutine(SkillGeyserAttack()); // 30% Cột nước
                skillPerformed = true;
            }
            else
            {
                yield return StartCoroutine(SkillSummonMinions()); // 30% Gọi đệ
                skillPerformed = true;
            }
        }
        // --- PHASE 1 (Cơ bản) ---
        else
        {
            if (rand < 0.2f)
            {
                yield return StartCoroutine(SkillHydroPump()); // 60% Vòi rồng
                skillPerformed = true;
            }
            else if (rand < 0.9f)
            {
                yield return StartCoroutine(SkillGeyserAttack()); // 30% Cột nước
                skillPerformed = true;
            }
            else
            {
                yield return StartCoroutine(SkillSummonMinions()); // 10% Gọi đệ
                skillPerformed = true;
            }
        }

        if (!skillPerformed)
        {
            // Fallback nếu có lỗi logic
            yield return StartCoroutine(SkillHydroPump());
        }

        // Nghỉ ngơi sau khi đánh
        yield return new WaitForSeconds(restTime);
        isBusy = false;
    }

    /// <summary>
    /// skill 1: HYDRO PUMP
    /// </summary>
    /// <returns></returns>
    IEnumerator SkillHydroPump()
    {
        if (movement) movement.Stop(); // Khóa di chuyển
        anim.SetTrigger("castBeam");

        float timer = 0f;

        // Lưu lại hướng scale ban đầu để xử lý logic góc quét
        float initialScaleX = transform.localScale.x;
        waterGun.SetActive(true);
        PlaySound(castSound);
        while (timer < aimDuration)
        {
            if (player != null && beamPivot != null)
            {
                // 1. Tính vector hướng
                Vector3 dir = (player.position - beamPivot.position).normalized;

                // 2. Tính góc cơ bản
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

                // 3. XỬ LÝ LẬT HÌNH (FIX LỖI BẮN NGƯỢC)
                // Nếu Boss đang lật mặt sang trái (Scale X < 0)
                if (transform.localScale.x > 0)
                {
                    // Lật ngược góc tính toán: 180 - angle
                    // Giải thích: Khi cha scale -1, con xoay 0 độ sẽ thành hướng trái. 
                    // Muốn con hướng phải (theo Player) thì phải xoay con 180 độ.
                    angle = 180 - angle;

                    // Đảo chiều vector vẽ Line Renderer cho khớp
                    dir.x = -dir.x;
                }

                // 4. Áp dụng góc xoay (Dùng localRotation an toàn hơn rotation)
                // Cộng thêm angleOffset để bạn tự chỉnh nếu hình vẽ bị lệch
                beamPivot.localRotation = Quaternion.Euler(0, 0, angle + angleOffset);

            }
            timer += Time.deltaTime;
            yield return null;
        }
        waterGun.SetActive(false);

        // --- BƯỚC 2: BẮN VÀ QUÉT (FIRING & SWEEPING) ---
        if (beamVisual)
        {
            beamVisual.SetActive(true);
            PlaySound(shootSound, 1);

            beamPivot.Rotate(0, 0, -30f);

            float sweepTimer = 0f;
            while (sweepTimer < beamDuration)
            {
                // Quét tia nước
                beamPivot.Rotate(Vector3.forward * sweepSpeed * Time.deltaTime);

                sweepTimer += Time.deltaTime;
                yield return null;
            }

            beamVisual.SetActive(false);
        }

        if (beamPivot) beamPivot.localRotation = Quaternion.identity;
        anim.SetTrigger("returnIdle");
        // QUAN TRỌNG: Mở lại di chuyển cho Boss (Code cũ của bạn thiếu dòng này sẽ làm Boss đứng yên mãi)
        if (movement) movement.SetMove(true);
    }

    /// <summary>
    /// skill 2: GEYSER ATTACK
    /// </summary>
    /// <returns></returns>
    IEnumerator SkillGeyserAttack()
    {
        anim.SetTrigger("attackCast");
        yield return new WaitForSeconds(0.5f); // Chờ animation vung tay

        // Số lượng Phase 2 nhiều hơn
        int geyserCount = isPhase2 ? 5 : 3;

        for (int i = 0; i < geyserCount; i++)
        {
            if (player == null) break;

            // Logic vị trí: 
            // 1 cái luôn nhắm trúng Player (ép chạy)
            // Các cái còn lại random xung quanh 
            Vector2 spawnPos;
            if (i == 0)
            {
                if (Physics.Raycast(player.position, Vector3.down, out RaycastHit hit, 5f))
                {
                    spawnPos = hit.point;
                }
                else
                {
                    spawnPos = player.position;
                }
            }
            else
            {
                float randX = Random.Range(-10f, 10f);
                float targetX = player.position.x + randX;

                // giữ vị trí trong world
                float clampedX = Mathf.Clamp(targetX, worldMinX, worldMaxX);

                spawnPos = new Vector2(clampedX, player.position.y);
            }
            StartCoroutine(SpawnSingleGeyser(spawnPos));

            // Delay nhỏ giữa các cột nước để tạo nhịp điệu
            yield return new WaitForSeconds(0.2f);
        }
        anim.SetTrigger("returnIdle");
    }

    IEnumerator SpawnSingleGeyser(Vector2 pos)
    {
        // 1. Cảnh báo
        GameObject warning = null;
        if (geyserWarningPrefab)
            warning = Instantiate(geyserWarningPrefab, pos, Quaternion.identity);

        yield return new WaitForSeconds(warningTime);

        if (warning) Destroy(warning);

        // 2. Nổ damage
        if (geyserDamagePrefab)
        {
            PlaySound(castSkill2, 1f);
            GameObject damage = Instantiate(geyserDamagePrefab, pos + new Vector2(0, 0.58f), Quaternion.identity);
            Destroy(damage, 1.5f); // Tự hủy sau 1s
        }
    }

    /// <summary>
    /// skill 3: SUMMON MINIONS
    /// </summary>
    /// <returns></returns>
    IEnumerator SkillSummonMinions()
    {
        // Kiểm tra số lượng quái hiện tại để tránh spam lag game
        int currentEnemies = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (currentEnemies >= maxMinions + 1) // +1 là tính cả Boss
        {
            // Nếu đã đầy quái -> Đổi sang dùng Skill AOE thay thế ngay lập tức
            yield return StartCoroutine(SkillGeyserAttack());
            yield break;
        }

        anim.SetTrigger("attackSummon");
        yield return new WaitForSeconds(1.0f);

        if (minionPrefabs != null && minionPrefabs.Count > 0 && summonPoints.Length > 0)
        {
            int amountToSpawn = isPhase2 ? 2 : 1;

            for (int i = 0; i < amountToSpawn; i++)
            {
                // Chọn ngẫu nhiên vị trí và loại quái
                Transform spawnPoint = summonPoints[Random.Range(0, summonPoints.Length)];
                GameObject minionToSpawn = minionPrefabs[Random.Range(0, minionPrefabs.Count)];

                Instantiate(minionToSpawn, spawnPoint.position, Quaternion.identity);
                yield return new WaitForSeconds(0.3f);
            }
        }
    }

    /// <summary>
    /// skill Ultimate: WATER BARRAGE
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator SkillUtimateUlti()
    {
        // 1. Chuẩn bị
        if (movement) movement.Stop(); // Boss đứng yên
        anim.SetTrigger("castUlti"); // Hoặc animation xả đạn riêng nếu có

        GameObject cast = Instantiate(castUlti, firePoint.position - new Vector3(0.5f, 0f, 0f), Quaternion.identity);

        yield return new WaitForSeconds(2.0f); // Chờ animation chuẩn bị xả đạn

        PlaySound(UlticastSound, 0.5f);
        // 2. Vòng lặp bắn liên thanh
        for (int i = 0; i < barrageCount; i++)
        {
            if (player == null) break;

            // --- TÍNH TOÁN VỊ TRÍ SPAWN NGẪU NHIÊN ---
            // Lấy vị trí gốc + random Y trong khoảng -1 đến 1
            float randomY = Random.Range(-2f, 2f);
            Vector3 spawnPos = firePoint.position + new Vector3(0, randomY, 0);

            // --- SINH ĐẠN ---
            GameObject proj = Instantiate(ultiProjectilePrefab, spawnPos, Quaternion.identity);

            // --- TÍNH HƯỚNG BAY ---
            // Đạn bay thẳng đến vị trí hiện tại của Player
            Vector2 dir = (player.position - spawnPos).normalized;

            // Xoay đạn theo hướng bay (để đầu đạn hướng về Player)
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            proj.transform.rotation = Quaternion.Euler(0, 0, angle);

            // Chờ một chút trước khi bắn viên tiếp theo
            yield return new WaitForSeconds(fireRate);
        }
        Destroy(cast);
        // 3. Kết thúc
        yield return new WaitForSeconds(0.5f); // Nghỉ mệt sau khi xả skill
        if (movement) movement.SetMove(true);  // Mở lại di chuyển
        isBusy = false;
        anim.SetTrigger("returnIdle");
    }

    // Helper: Vẽ gizmos để dễ setup
    //void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.cyan;
    //    Gizmos.DrawWireSphere(transform.position, attackRange);
    //}
}