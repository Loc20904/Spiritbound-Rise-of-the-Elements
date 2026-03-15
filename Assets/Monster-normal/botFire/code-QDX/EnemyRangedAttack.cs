using UnityEngine;
using System.Collections;

public class EnemyRangedAttack : MonoBehaviour
{
    [Header("Detect Box (Attack Range)")]
    public float detectWidth = 6f;   // ngang
    public float detectHeight = 2f;  // dọc
    public LayerMask playerLayer;

    [Header("Shoot")]
    public GameObject arrowPrefab;
    public Transform firePoint;
    public float attackCooldown = 2f;

    [Header("Attack Animation")]
    public Sprite[] attackFrames;
    public float frameRate = 0.05f;
    public int[] fireFrameIndexes;

    public bool PlayerInRange { get; private set; }
    public Vector2 PlayerPosition { get; private set; }
    public bool IsAttacking => isAttacking;
    Coroutine attackRoutine;
    SpriteRenderer sr;
    bool isAttacking;
    float cooldownTimer;

    // ================= START =================
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // ================= UPDATE =================
    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (isAttacking || cooldownTimer > 0)
            return;

        DetectPlayer();
    }

    // ================= DETECT (BOX) =================
    void DetectPlayer()
    {
        PlayerInRange = false;

        Vector2 center = transform.position;

        Collider2D hit = Physics2D.OverlapBox(
            center,
            new Vector2(detectWidth, detectHeight),
            0,
            playerLayer
        );

        if (!hit)
            return;

        Vector2 playerPos = hit.transform.position;

        // ⭐ chỉ phía trước
        float dirX = transform.localScale.x > 0 ? 1 : -1;// nếu lớn hơn 0 thì lấy 

        bool isFront = (playerPos.x - transform.position.x) * dirX > 0; // tính hiệu số giữa vị trí player và enemy, nhân với dirX để xác định xem player có đang ở phía trước enemy hay không. Nếu kết quả lớn hơn 0, nghĩa là player đang ở phía trước enemy theo hướng mà enemy đang đối mặt.

        if (!isFront)
            return;

        PlayerInRange = true;
        PlayerPosition = playerPos;// lưu vị trí player để có thể sử dụng trong các phần khác của code nếu cần thiết (ví dụ: để tính toán hướng bắn hoặc di chuyển).

        attackRoutine = StartCoroutine(AttackRoutine());
    }

    // ================= ATTACK =================
    IEnumerator AttackRoutine()
    {
        isAttacking = true;
        cooldownTimer = attackCooldown;

        for (int i = 0; i < attackFrames.Length; i++)
        {
            sr.sprite = attackFrames[i];

            foreach (int index in fireFrameIndexes)
            {
                if (i == index)
                {
                    FireArrow();
                    break;
                }
            }

            yield return new WaitForSeconds(frameRate);// đợi một khoảng thời gian nhất định trước khi chuyển sang frame tiếp theo của animation tấn công. Điều này giúp tạo ra hiệu ứng mượt mà và đồng bộ giữa việc thay đổi sprite và việc bắn mũi tên.
        }

        isAttacking = false;
    }

    // ================= FIRE =================
    void FireArrow()
    {
        Vector2 dir = transform.localScale.x > 0 ? Vector2.right : Vector2.left;// quay mặt FirePoint theo hướng enemy đang đối mặt, nếu localScale.x > 0 thì hướng là Vector2.right (phải), ngược lại là Vector2.left (trái). Điều này đảm bảo rằng mũi tên sẽ được bắn ra theo hướng mà enemy đang nhìn về phía trước.

        GameObject arrow = Instantiate(arrowPrefab, firePoint.position, arrowPrefab.transform.rotation);// tạo một instance mới của prefab mũi tên tại vị trí của firePoint và với rotation mặc định của prefab. Điều này sẽ tạo ra một mũi tên mới mỗi khi enemy tấn công.
        ArrowBullet bullet = arrow.GetComponent<ArrowBullet>();// Lấy component ArrowBullet từ prefab mũi tên vừa được tạo ra. Component này có thể chứa logic để di chuyển mũi tên, xử lý va chạm, hoặc các hiệu ứng khác liên quan đến mũi tên.

        if (bullet != null)
            bullet.Init(dir, GetComponent<Collider2D>());// gọi phương thức Init trên component ArrowBullet, truyền vào hướng bắn (dir) và collider của enemy. Điều này có thể được sử dụng để thiết lập hướng di chuyển của mũi tên và để tránh việc mũi tên va chạm với chính enemy khi được bắn ra.
    }
    public void StopAttack()
    {
        isAttacking = false;

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);
    }
    // ================= GIZMO =================
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector2 center = transform.position;

        Gizmos.DrawWireCube(center, new Vector2(detectWidth, detectHeight));
    }
}
