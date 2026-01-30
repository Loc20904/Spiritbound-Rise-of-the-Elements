using System.Collections;
using UnityEngine;

public class BossFireAttack : BossAttackBase // Kế thừa từ lớp cha
{
    [Header("Fire Skills")]
    public GameObject fireballPrefab;   // Đạn thường
    public GameObject specialPrefab;    // Đạn đặc biệt (nếu có)
    public Transform firePoint;         // Vị trí bắn

    // GHI ĐÈ logic tấn công của cha
    protected override IEnumerator PerformAttackRoutine()
    {
        FacePlayer(); // Gọi hàm hỗ trợ từ cha để quay mặt về phía Player

        // ==========================================================
        // PHASE 1: Tấn công cơ bản
        // ==========================================================
        if (!isPhase2)
        {
            // Random 50/50 chọn kỹ năng
            if (Random.value > 0.5f)
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
            int randSkill = Random.Range(0, 3);

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
                        float randomX = Random.Range(-8f, 8f);
                        Vector3 spawnPos = new Vector3(randomX, firePoint.position.y + 5f, 0);

                        GameObject rain = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);

                        // Xoay đạn cắm đầu xuống đất (-90 độ)
                        rain.transform.rotation = Quaternion.Euler(0, 0, -90);

                        base.PlaySound(shootSound);

                        // Rơi rải rác chứ không rơi cùng lúc
                        yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
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
        GameObject spell = Instantiate(prefab, firePoint.position, Quaternion.identity);

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
        throw new System.NotImplementedException();
    }
}