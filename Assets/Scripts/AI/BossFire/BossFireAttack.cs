using System;
using System.Collections;
using UnityEngine;

public class BossFireAttack : BossAttackBase // Kế thừa từ lớp cha
{
    public LayerMask playerLayer;

    [Header("Fire Skills")]
    public GameObject fireballPrefab;   // Đạn thường
    public GameObject specialPrefab;    // Đạn đặc biệt (nếu có)
    public Transform firePoint;         // Vị trí bắn

    [Header("Ultimate: Supernova")]
    public GameObject sunPrefab;       // Quả cầu lửa trung tâm
    public GameObject sunBulletPrefab; // Đạn bắn ra từ "Mặt trời"
    public GameObject BoomVFX;
    public AudioClip BoomSFX;

    public float rotateSpeed = 1000f;   // Tốc độ xoay của tia đạn

    public Boolean isBusy = false;

    // GHI ĐÈ logic tấn công của cha
    protected override IEnumerator PerformAttackRoutine()
    {
        FacePlayer(); // Gọi hàm hỗ trợ từ cha để quay mặt về phía Player

        if (isBusy) yield break;

        // ==========================================================
        // PHASE 1: Tấn công cơ bản
        // ==========================================================
        if (!isPhase2)
        {
            // Random 50/50 chọn kỹ năng
            if (UnityEngine.Random.value > 0.5f)
            {
                // Skill 1: Bắn nhanh 3 phát
                anim.SetTrigger("fastFireball");
                base.PlaySound(castSound); // Dùng base.PlaySound để phát tiếng cast
                yield return new WaitForSeconds(0.5f); // Chờ animation vung tay
                yield return StartCoroutine(BurstFire(3, 0.3f, fireballPrefab));
            }
            else
            {
                // Skill 2: Bắn đạn to/đặc biệt
                anim.SetTrigger("fastFireball"); // Bạn kiểm tra lại tên trigger trong Animator nhé
                base.PlaySound(castSound);
                yield return new WaitForSeconds(0.8f); // Chờ tụ lực lâu hơn
                SpawnSpell(specialPrefab ?? fireballPrefab, 0); // Nếu không có specialPrefab thì dùng fireballPrefab
            }
        }
        // ==========================================================
        // PHASE 2: Boss nổi điên (Thêm chiêu mới)
        // ==========================================================
        else
        {
            // Random 3 chiêu thức: 0 = Tỏa, 1 = Bắn nhanh, 2 = Mưa thiên thạch
            int randSkill = UnityEngine.Random.Range(0, 3);

            switch (randSkill)
            {
                case 0: // Bắn tỏa 3 hướng (Spread)
                    anim.SetTrigger("fireRain");
                    base.PlaySound(castSound);
                    yield return new WaitForSeconds(0.5f);

                    // Bắn 3 viên: -30 độ, 0 độ, +30 độ
                    SpawnSpell(fireballPrefab, -30f);
                    SpawnSpell(fireballPrefab, 0f);
                    SpawnSpell(fireballPrefab, 30f);
                    break;

                case 1: // Bắn liên thanh (Rapid Fire)
                    anim.SetTrigger("fastFireball");
                    base.PlaySound(castSound);
                    yield return new WaitForSeconds(0.3f);
                    yield return StartCoroutine(BurstFire(5, 0.15f, fireballPrefab));
                    break;

                case 2: // Mưa thiên thạch (Fire Rain) - Logic cũ của bạn
                    anim.SetTrigger("fireRain");
                    base.PlaySound(castSound);
                    yield return new WaitForSeconds(0.5f);

                    for (int i = 0; i < 10; i++)
                    {
                        if (player == null) break;

                        // Tạo mưa rơi từ trên cao xuống ngẫu nhiên quanh Player
                        // Lấy vị trí X ngẫu nhiên trong khoảng màn hình (ước lượng +/- 8 đơn vị)
                        float randomX = UnityEngine.Random.Range(-8f, 8f);
                        Vector3 spawnPos = new Vector3(randomX, firePoint.position.y + 5f, 0);

                        GameObject rain = Instantiate(fireballPrefab, spawnPos, Quaternion.identity, skillHolder);

                        // Xoay đạn cắm đầu xuống đất (-90 độ)
                        rain.transform.rotation = Quaternion.Euler(0, 0, -90);

                        base.PlaySound(shootSound);

                        // Rơi rải rác chứ không rơi cùng lúc
                        yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.3f));
                    }
                    break;
            }
        }

        yield return new WaitForSeconds(0.5f); // Nghỉ một chút sau khi đánh xong
    }

    // --- CÁC HÀM HỖ TRỢ RIÊNG CHO BOSS LỬA ---

    // Hàm bắn một viên đạn theo góc
    void SpawnSpell(GameObject prefab, float angleOffset)
    {
        if (prefab == null || firePoint == null || player == null) return;

        // 1. Sinh ra đạn
        GameObject spell = Instantiate(prefab, firePoint.position, Quaternion.identity, skillHolder);

        // 2. Phát tiếng bắn (Gọi hàm từ cha)
        base.PlaySound(shootSound);

        // 3. Tính toán hướng bay về phía Player
        Vector2 dir = (player.position - firePoint.position).normalized;

        // Tính góc xoay cơ bản
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Cộng thêm góc lệch (ví dụ bắn tỏa thì lệch +/- 30 độ)
        spell.transform.rotation = Quaternion.Euler(0, 0, angle + angleOffset);
    }

    // Coroutine bắn liên tục (Burst)
    IEnumerator BurstFire(int count, float delay, GameObject ammo)
    {
        for (int i = 0; i < count; i++)
        {
            if (player == null) break;

            SpawnSpell(ammo, 0); // Góc 0 là bắn thẳng vào player

            yield return new WaitForSeconds(delay);
        }
    }

    protected override IEnumerator SkillUtimateUlti()
    {

        isBusy = true;

        // --- BƯỚC 1: BAY VÀO TRUNG TÂM HOẶC GỒNG TẠI CHỖ ---
        anim.SetTrigger("fireRain"); // Dùng anim gồng chiêu
        Vector3 centerPos = new Vector3(0, 0, 0); // Vị trí trung tâm Map hoặc giữa sân

        // Tạo quả cầu mặt trời nhân tạo
        GameObject sun = Instantiate(sunPrefab, centerPos, Quaternion.identity, skillHolder);
        sun.transform.localScale = Vector3.zero; // Bắt đầu từ tí hon

        // Phóng to quả cầu dần dần trong 1s
        float t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime;
            //Mathf.Lerp is used to smoothly transition the scale from 0 to 0.6 over time t
            sun.transform.localScale = Vector3.one * Mathf.Lerp(0, 0.6f, t);
            yield return null;
        }

        // --- BƯỚC 2: PHÁT TÁN TIA LỬA (BULLET HELL) ---
        float duration = 5f; // Bắn liên tục trong 5 giây
        float timer = 0f;
        float currentAngle = 0f;

        while (timer < duration)
        {
            // Tạo 4 tia đạn theo hình chữ thập (+) xoay tròn
            for (int i = 0; i < 4; i++)
            {
                float shotAngle = currentAngle + (i * 90f);
                FireSunBullet(sun.transform.position, shotAngle);
            }

            currentAngle += rotateSpeed * Time.deltaTime; // Xoay góc bắn
            timer += 0.2f; // Tốc độ ra đạn (delay giữa các đợt tia)
            yield return new WaitForSeconds(0.1f);
        }

        // --- BƯỚC 3: VỤ NỔ SIÊU TÂN TINH (SUPERNOVA) ---
        // Thu nhỏ lại một chút để "nén" năng lượng trước khi nổ
        yield return StartCoroutine(ScaleOverTime(sun.transform, sun.transform.localScale, Vector3.one * 0.5f, 0.2f));

        GameObject boomVFX = null;
        // Tạo VFX nổ lớn tràn màn hình
        if (BoomVFX) boomVFX = Instantiate(BoomVFX, centerPos, Quaternion.identity, skillHolder);
        Destroy(sun);
        yield return new WaitForSeconds(0.1f);
        PlaySound(BoomSFX);
        // Nổ tung!
        // Gây sát thương diện rộng (có thể dùng OverlapCircleAll)
        HandleSupernovaDamage(centerPos, 15f);
        StartCoroutine(TriggerWhiteout(3f));
        Destroy(boomVFX);

        yield return new WaitForSeconds(1f);

        isBusy = false;
    }

    // Hàm phụ bắn đạn từ tâm mặt trời
    void FireSunBullet(Vector3 position, float angle)
    {
        GameObject bullet = Instantiate(sunBulletPrefab, position, Quaternion.Euler(0, 0, angle), skillHolder);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            rb.linearVelocity = dir * 8f; // Tốc độ bay của tia lửa
        }
    }

    // Hàm phụ thay đổi kích thước mượt mà
    IEnumerator ScaleOverTime(Transform target, Vector3 start, Vector3 end, float time)
    {
        float elapsed = 0;
        while (elapsed < time)
        {
            target.localScale = Vector3.Lerp(start, end, elapsed / time);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    void HandleSupernovaDamage(Vector3 center, float radius)
    {
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(center, radius, playerLayer);
        foreach (Collider2D p in hitPlayers)
        {
            // p.GetComponent<PlayerHealth>()?.TakeDamage(50); // Sát thương cực lớn
            Debug.Log("Player bị nổ tung bởi Supernova!");
        }
    }

    [Header("Whiteout Effect")]
    public UnityEngine.UI.Image flashImage;

    IEnumerator TriggerWhiteout(float duration)
    {
        // 1. Hiện trắng ngay lập tức (Alpha = 1)
        Color c = flashImage.color;
        c.a = 1f;
        flashImage.color = c;

        // 2. Mờ dần về trong suốt
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, elapsed / duration);
            flashImage.color = c;
            yield return null;
        }
    }
}