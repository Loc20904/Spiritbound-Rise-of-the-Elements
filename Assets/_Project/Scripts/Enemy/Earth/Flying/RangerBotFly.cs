using UnityEngine;
using System.Collections;

public class RangerBotFly : MonoBehaviour
{   
    [Header("Detect Box (Attack Range)")]
    public float attackRadius = 4f;
    public Vector2 detectOffset;          // chỉnh vị trí nếu cần
    public LayerMask playerLayer;

    [Header("Shoot")]
    public GameObject dirtBulletPrefab;
    public Transform firePoint;
    public Transform firePoint1;
    public float attackCooldown = 2f;
    [Tooltip("Thời gian chờ (giây) sau khi bắn firePoint rồi mới bắn firePoint1")]
    public float delayBetweenFirePoints = 3f;

    private float attackTimer;
    private Coroutine secondShotRoutine;
    private Vector2 lastFirstShotDirection; // hướng viên đầu, dùng cho firePoint1 khi player ra khỏi tầm
    private GameObject pendingMergedAtFirePoint1; // merged stone giữ tại firePoint1 đến khi bắn

    void Update()
    {
        attackTimer -= Time.deltaTime;
        // Không tự detect & bắn — chỉ bắn khi CastSkill gọi Shoot() sau khi cast xong
    }

    /// <summary>Gọi từ CastSkill khi cast xong (2 đá đã lên firePoint + frame 3). Không bắn lúc đang cast.</summary>
    public void Shoot(Vector2 playerPos)
    {
        
        if (secondShotRoutine != null) return;

        // Bắn từ firePoint ngay
        Vector2 dir = (playerPos - (Vector2)firePoint.position).normalized;
        lastFirstShotDirection = dir; // lưu để firePoint1 bắn cùng hướng nếu player ra khỏi tầm
        GameObject bullet = Instantiate(dirtBulletPrefab, firePoint.position, Quaternion.identity);
        bullet.GetComponent<DirtBullet>().Init(dir, GetComponent<Collider2D>());

        attackTimer = attackCooldown;
        // Sau delayBetweenFirePoints giây mới bắn từ firePoint1
        secondShotRoutine = StartCoroutine(ShootFirePoint1AfterDelay());
    }
    /// <summary>EarthSummon gọi khi merged tại firePoint1 — giữ đến lúc bắn mới hủy</summary>
    public void SetPendingMergedAtFirePoint1(GameObject mergedObj)
    {
        pendingMergedAtFirePoint1 = mergedObj;
    }

    IEnumerator ShootFirePoint1AfterDelay()
    {
        yield return new WaitForSeconds(delayBetweenFirePoints);

        // Hủy merged tại firePoint1 ngay trước khi bắn (chuyển mượt thành đạn)
        if (pendingMergedAtFirePoint1 != null)
        {
            Destroy(pendingMergedAtFirePoint1);
            pendingMergedAtFirePoint1 = null;
        }

        // Re-detect player để bắn đúng hướng (hoặc bắn theo hướng cũ nếu player đã ra khỏi tầm)
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Vector2 center = col != null ? (Vector2)col.bounds.center : (Vector2)transform.position;
        center += detectOffset;

        Collider2D player = Physics2D.OverlapCircle(center, attackRadius, playerLayer);
        Vector2 dir1 = player != null
            ? ((Vector2)player.transform.position - (Vector2)firePoint1.position).normalized
            : lastFirstShotDirection; // cùng hướng viên đầu → không còn bắn lệch sang phải

        GameObject bullet1 = Instantiate(dirtBulletPrefab, firePoint1.position, Quaternion.identity);
        bullet1.GetComponent<DirtBullet>().Init(dir1, GetComponent<Collider2D>());

        secondShotRoutine = null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        BoxCollider2D col = GetComponent<BoxCollider2D>();
        Vector2 center = col != null ? (Vector2)col.bounds.center : (Vector2)transform.position;

        center += detectOffset;

        Gizmos.DrawWireSphere(center, attackRadius);
    }
}