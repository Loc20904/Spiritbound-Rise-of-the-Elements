using System.Collections;
using UnityEngine;

public class FinalBossAttack : BossAttackBase
{
    [Header("Energy Boom Settings")]
    public GameObject energyBoomPrefab; // Prefab chứa hiệu ứng đếm ngược và nổ
    public int boomCount = 5;           // Số lượng quả cầu mỗi lần triệu hồi
    public float explosionDelay = 3f;   // Thời gian chờ nổ
    public float boomRadius = 3f;       // Bán kính vụ nổ

    [Header("Dragon Skill Settings")]
    public GameObject finalBoss_Skill2_1; // Hiệu ứng gồng tại Boss (vòng ma pháp, tụ lực...)
    public GameObject finalBoss_Skill2_2; // Hiệu ứng đường dẫn trên đất (trồi lên từ ground)
    public GameObject finalBoss_Skill2_3; // Hiệu ứng Rồng trồi lên cắn (tại chân Player)
    public AudioClip dragonCastSFX;
    public AudioClip groundRumbleSFX;
    public AudioClip dragonBiteSFX;
    public LayerMask groundLayer;
    public LayerMask playerLayer;

    [Header("Kunai Skill Settings")]
    public GameObject kunaiPrefab;      // Prefab phi tiêu
    public float kunaiSpeed = 12f;      // Tốc độ bay
    public float spreadAngle = 15f;     // Góc lệch giữa 3 phi tiêu
    public AudioClip kunaiThrowSFX;

    [Header("Skill 4: Dark Pillar Settings")]
    public GameObject finalBoss_Skill4_Warning; // Prefab tia sáng tím mỏng (frames đầu của Skill 4)
    public GameObject finalBoss_Skill4_Boom;    // Prefab cột lửa đầu lâu tím (frames bùng nổ)
    public float skill4_WarningTime = 1f;       // Thời gian cảnh báo
    public AudioClip darkMagicCastSFX;
    public AudioClip darkExplosionSFX;
    public float worldMinX = -12f; // Tọa độ X bên trái cùng của map
    public float worldMaxX = 12f;  // Tọa độ X bên phải cùng của map
    public int pillarCount = 5;    // Số lượng cột năng lượng

    private bool isAttacking = false;

    // --- LOGIC CHÍNH ---
    protected override IEnumerator PerformAttackRoutine()
    {
        if (isAttacking) yield break;
        isAttacking = true;

        //FacePlayer();

        // Random từ 0 đến 3 (tương ứng 4 chiêu thức)
        int rand = UnityEngine.Random.Range(0, 4);

        switch (rand)
        {
            case 0: yield return StartCoroutine(EnergyOverloadRoutine()); break;
            case 1: yield return StartCoroutine(DragonBiteRoutine()); break;
            case 2: yield return StartCoroutine(KunaiBurstRoutine()); break;
            case 3: yield return StartCoroutine(DarkPillarRoutine()); break;
        }

        yield return new WaitForSeconds(1.5f);
        isAttacking = false;
    }
    public IEnumerator TriggerBossAction()
    {
        yield return StartCoroutine(PerformAttackRoutine());
    }
    // --- ULTIMATE: CHAIN PRISON ---
    // --- ULTIMATE: CHAIN PRISON ---
    [Header("Ultimate: Chain Prison Settings")]
    public GameObject ultiChainPrefab;    // Prefab sợi xích thực sự đâm ra
    public int ultiChainCount = 6;        // Số lượng xích triệu hồi
    public float ultiRadius = 3.5f;       // Bán kính xuất hiện quanh Player
    public AudioClip chainStrikeSFX;      // Âm thanh xích đâm

    protected override IEnumerator SkillUtimateUlti()
    {
        // Thêm kiểm tra isAttacking để tránh Boss cast Ulti đè lên các chiêu khác
        if (player == null) yield break;

        isAttacking = true;

        Vector3 playerPos = player.position;
        Vector3[] spawnPositions = new Vector3[ultiChainCount];

        // --- BƯỚC KHÔI PHỤC: Tính toán vị trí bao vây Player ---
        for (int i = 0; i < ultiChainCount; i++)
        {
            // Lấy vị trí ngẫu nhiên xung quanh Player
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle.normalized * ultiRadius;
            randomPoint *= UnityEngine.Random.Range(0.6f, 1.2f);
            spawnPositions[i] = playerPos + new Vector3(randomPoint.x, randomPoint.y, 0);
        }

        // --- PHASE 2 & 3: Gọi xích ra ---
        if (ultiChainPrefab != null)
        {
            PlaySound(chainStrikeSFX, 1.2f);
            for (int i = 0; i < ultiChainCount; i++)
            {
                Vector3 direction = (playerPos - spawnPositions[i]).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion chainRotation = Quaternion.Euler(0, 0, angle);

                Instantiate(ultiChainPrefab, spawnPositions[i], chainRotation, skillHolder);
                yield return new WaitForSeconds(0.1f);

            }
        }

        // Đợi Boss phục hồi dáng đứng (Recovery phase của Boss)
        yield return new WaitForSeconds(1.5f);
        isAttacking = false;
    }

    // --- CHIÊU 1: ENERGY OVERLOAD ---
    private IEnumerator EnergyOverloadRoutine()
    {
        base.PlaySound(castSound);

        // Triệu hồi các điểm nổ
        for (int i = 0; i < boomCount; i++)
        {
            Vector3 spawnPos = GetRandomArenaPosition();

            // Tạo quả cầu năng lượng
            GameObject boom = Instantiate(energyBoomPrefab, spawnPos, Quaternion.identity, skillHolder);

            yield return new WaitForSeconds(0.2f); // Delay nhỏ giữa mỗi lần spawn cho đẹp
        }
        yield return new WaitForSeconds(explosionDelay);
    }

    // --- CHIÊU 2: DRAGON BITE ---
    private IEnumerator DragonBiteRoutine()
    {
        if (player == null) yield break;

        // BƯỚC 1: Hiệu ứng gồng tại Boss
        // anim.SetTrigger("dragonAttack"); // Nếu có anim
        PlaySound(dragonCastSFX, 1f);
        if (finalBoss_Skill2_1 != null)
        {
            Instantiate(finalBoss_Skill2_1, transform.position, Quaternion.identity, skillHolder);
        }
        yield return new WaitForSeconds(0.8f);

        // BƯỚC 2: Hiệu ứng trồi lên mặt đất (Dùng Raycast từ Boss xuống Ground)
        Vector3 groundPos = transform.position;
        // Bắt đầu bắn tia từ dưới chân Boss thay vì từ tâm
        Vector3 startPos = transform.position + Vector3.down * 2f;
        RaycastHit2D hit = Physics2D.Raycast(startPos, Vector2.down, 10f, groundLayer);
        if (hit.collider != null)
        {
            groundPos = hit.point;
        }

        PlaySound(groundRumbleSFX, 0.8f);
        if (finalBoss_Skill2_2 != null)
        {
            Instantiate(finalBoss_Skill2_2, groundPos + new Vector3(0, 1f, 0), Quaternion.identity, skillHolder);
        }

        // BƯỚC 3: Đợi 2s và triệu hồi Rồng cắn dưới chân Player
        yield return new WaitForSeconds(2.0f);

        if (player != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 playerPos = player.position;
                startPos = playerPos + Vector3.down * 2f;
                // Có thể dùng Raycast lần nữa để đảm bảo rồng mọc lên từ mặt đất dưới chân Player
                RaycastHit2D playerGroundHit = Physics2D.Raycast(startPos, Vector2.down, 5f, groundLayer);
                Vector3 spawnPos = playerGroundHit.collider != null ? (Vector3)playerGroundHit.point : playerPos;

                PlaySound(dragonBiteSFX, 1.2f);
                if (finalBoss_Skill2_3 != null)
                {

                    GameObject dragon = Instantiate(finalBoss_Skill2_3, spawnPos + new Vector3(0, 1.8f, 0), Quaternion.identity, skillHolder);

                    // Xử lý gây sát thương (nếu script dragon cắn có logic sát thương riêng)
                    HandleDragonDamage(spawnPos, 2f);

                }
                yield return new WaitForSeconds(0.5f); // Delay giữa các lần cắn
            }
        }
    }

    // --- CHIÊU 3: KUNAI BURST ---
    private IEnumerator KunaiBurstRoutine()
    {
        if (player == null) yield break;

        for (int wave = 0; wave < 3; wave++) // 3 đợt
        {
            base.PlaySound(kunaiThrowSFX);

            // Tính toán hướng về phía người chơi tại thời điểm ném
            Vector2 targetDir = (player.position - transform.position).normalized;
            float baseAngle = Mathf.Atan2(targetDir.y, targetDir.x) * Mathf.Rad2Deg;

            // Bắn 3 phi tiêu: -spreadAngle, 0, +spreadAngle
            for (int i = -1; i <= 1; i++)
            {
                float finalAngle = baseAngle + (i * spreadAngle);
                SpawnKunai(finalAngle);
            }

            yield return new WaitForSeconds(0.4f); // Khoảng cách giữa các đợt bắn
        }
    }

    // --- CHIÊU 4: DARK PILLAR ---
    private IEnumerator DarkPillarRoutine()
    {
        // BƯỚC 1: Tính toán các vị trí spawn dựa trên chiều rộng map
        Vector3[] spawnPositions = new Vector3[pillarCount];

        // Chia đều khoảng cách: worldMaxX - worldMinX
        float totalWidth = worldMaxX - worldMinX;
        float segment = totalWidth / (pillarCount - 1);

        for (int i = 0; i < pillarCount; i++)
        {
            float targetX = worldMinX + (segment * i);
            // Bắn Raycast từ trên cao xuống tại mỗi điểm X để tìm mặt đất (Y)
            Vector2 rayStart = new Vector2(targetX, transform.position.y + 5f);
            RaycastHit2D hit = Physics2D.Raycast(rayStart, Vector2.down, 20f, groundLayer);
            hit.point += new Vector2(0, -1.2f);

            if (hit.collider != null)
                spawnPositions[i] = hit.point;
            else
                spawnPositions[i] = new Vector3(targetX, transform.position.y, 0);

            // Chỉnh sửa Offset Y nếu cần (như code cũ của bạn)
            spawnPositions[i] += new Vector3(0, 0.5f, 0);
        }

        // BƯỚC 2: Warning - Hiện tất cả tia sáng mỏng cùng lúc
        PlaySound(darkMagicCastSFX, 0.8f);
        GameObject[] warnings = new GameObject[pillarCount];

        for (int i = 0; i < pillarCount; i++)
        {
            if (finalBoss_Skill4_Warning != null)
            {
                warnings[i] = Instantiate(finalBoss_Skill4_Warning, spawnPositions[i] + new Vector3(0, -0.2f, 0), Quaternion.identity, skillHolder);
            }
            yield return new WaitForSeconds(0.1f);
        }

        // Đợi thời gian cảnh báo
        yield return new WaitForSeconds(skill4_WarningTime);

        // BƯỚC 3: Boom - Nổ đồng loạt
        PlaySound(darkExplosionSFX, 1.2f);
        for (int i = 0; i < pillarCount; i++)
        {
            // Xóa Warning
            if (warnings[i] != null) Destroy(warnings[i]);

            // Tạo Boom
            if (finalBoss_Skill4_Boom != null)
            {
                GameObject boom = Instantiate(finalBoss_Skill4_Boom, spawnPositions[i], Quaternion.identity, skillHolder);
                Destroy(boom, 2f);

                // Gây sát thương tại mỗi vị trí nổ
                HandleDarkDamage(spawnPositions[i], 2.0f);
            }
        }
    }

    private void HandleDarkDamage(Vector3 pos, float radius)
    {
        Collider2D hit = Physics2D.OverlapCircle(pos, radius, playerLayer);
        if (hit != null)
        {
            Debug.Log("<color=purple>Final Boss:</color> Player trúng lời nguyền hắc ám!");
            // hit.GetComponent<PlayerHealth>()?.TakeDamage(40);
        }
    }

    private void SpawnKunai(float angle)
    {
        // Tạo phi tiêu tại vị trí Boss (hoặc firePoint nếu bạn có)
        GameObject kunai = Instantiate(kunaiPrefab, transform.position, Quaternion.Euler(0, 0, angle), skillHolder);

        Rigidbody2D rb = kunai.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            // Chuyển góc thành Vector hướng bay
            Vector2 moveDir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            rb.linearVelocity = moveDir * kunaiSpeed;
        }
    }

    private void HandleDragonDamage(Vector3 pos, float radius)
    {
        Collider2D hit = Physics2D.OverlapCircle(pos, radius, playerLayer);
        if (hit != null)
        {
            // hit.GetComponent<PlayerHealth>()?.TakeDamage(damage);
            Debug.Log("Player bị rồng cắn!");
        }
    }



    // Hàm hỗ trợ lấy vị trí ngẫu nhiên trong vùng chiến đấu
    private Vector3 GetRandomArenaPosition()
    {
        // Bạn có thể tùy chỉnh phạm vi này khớp với Map của bạn
        float x = UnityEngine.Random.Range(-10f, 10f);
        float y = UnityEngine.Random.Range(-5f, 5f);
        return new Vector3(x, y, 0);
    }

    private void OnEnable()
    {
        // Đảm bảo mỗi khi Boss được bật lại (hoặc hồi sinh, chuyển phase), biến này luôn được reset!
        isAttacking = false;
    }
}