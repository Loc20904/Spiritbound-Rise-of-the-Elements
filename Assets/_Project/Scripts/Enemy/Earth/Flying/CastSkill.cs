using UnityEngine;
using System.Collections;

public class CastSkill : MonoBehaviour
{
    [Header("Detect")]
    public float attackRadius = 4f;
    public Vector2 detectOffset;
    public LayerMask playerLayer;

    [Header("Summon")]
    public GameObject earthSummonPrefab;
    public Transform firePoint;
    public Transform firePoint1;
    public float groundY = -2.5f;

    [Header("Cooldown")]
    public float castCooldown = 3f;

    [Header("Reference")]
    public RangerBotFly rangerBot;

    private bool isCasting = false;
    private float castTimer;

    /// <summary>BotFlyAI đọc: true khi player trong tầm → bot đứng yên, chạy Breath + cast.</summary>
    public bool PlayerInRange { get; private set; }
    /// <summary>BotFlyAI đọc: true khi đang cast → bot đứng yên.</summary>
    public bool IsCasting => isCasting;

    void Update()
    {
        castTimer -= Time.deltaTime;

        Vector2 center = (Vector2)transform.position + detectOffset;
        Collider2D playerCol = Physics2D.OverlapCircle(center, attackRadius, playerLayer);
        PlayerInRange = (playerCol != null);

        if (!isCasting && castTimer <= 0f && PlayerInRange)
        {
            DetectAndCast();
        }
    }

    void DetectAndCast()
    {
        Vector2 center = (Vector2)transform.position + detectOffset;
        Collider2D playerCol = Physics2D.OverlapCircle(center, attackRadius, playerLayer);
        if (playerCol == null) return;

        StartCoroutine(CastRoutine(playerCol.transform));
    }

    IEnumerator CastRoutine(Transform player)
    {
        isCasting = true;

        yield return new WaitForSeconds(0.5f); // thời gian vận skill

        // Spawn 2 đá: mỗi cái bay lên firePoint / firePoint1, đổi frame 3, báo xong
        bool done1 = false, done2 = false;
        Vector3 spawn1 = new Vector3(firePoint.position.x, groundY, 0);
        GameObject e1 = Instantiate(earthSummonPrefab, spawn1, Quaternion.identity);
        e1.GetComponent<EarthSummon>().Init(firePoint.position, rangerBot, player, () => done1 = true, keepMergedForSecondFirePoint: false);

        Vector3 spawn2 = new Vector3(firePoint1.position.x, groundY, 0);
        GameObject e2 = Instantiate(earthSummonPrefab, spawn2, Quaternion.identity);
        e2.GetComponent<EarthSummon>().Init(firePoint1.position, rangerBot, player, () => done2 = true, keepMergedForSecondFirePoint: true);

        // Chờ CẢ HAI đá lên firePoint + đổi frame 3 xong rồi mới bắn (không bắn lúc cast)
        yield return new WaitUntil(() => done1 && done2);

        rangerBot.Shoot(player.position);

        castTimer = castCooldown;
        isCasting = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector2 center = (Vector2)transform.position + detectOffset;
        Gizmos.DrawWireSphere(center, attackRadius);
    }
}