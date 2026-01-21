using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BossMovement : MonoBehaviour
{

    [Header("Teleport Settings")]
    public GameObject teleportVFX;
    [Range(0.1f, 0.4f)]
    public float screenPadding = 0.15f;

    [Header("Trigger Logic")]
    public float damagePercentToTeleport = 0.05f;
    private float damageThreshold;
    private float accumulatedDamage = 0f;

    [Header("References")]
    public Animator anim;
    public Transform player;
    private BossHealth health;

    private Rigidbody2D rb;
    private bool isTeleporting = false;
    private float fixedY; // Đây sẽ là trục giữa để boss nhấp nhô quanh nó
    private SpriteRenderer sr;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        health = GetComponent<BossHealth>();

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    void Start()
    {
        // Lưu lại vị trí Y ban đầu làm "Trục giữa"
        fixedY = transform.position.y;

        if (health != null)
        {
            damageThreshold = health.maxHP * damagePercentToTeleport;
            health.OnDamageTaken += HandleDamageReceived;
        }
    }

    void OnDestroy()
    {
        if (health != null)
        {
            health.OnDamageTaken -= HandleDamageReceived;
        }
    }

    void Update()
    {
        // Chỉ lơ lửng và xoay mặt khi KHÔNG Teleport
        if (player != null && !isTeleporting)
        {
            LookAtPlayer();
            //Hover();
        }
    }

    // ---------------------------------------------------------
    // LOGIC NHẬN SÁT THƯƠNG
    // ---------------------------------------------------------
    void HandleDamageReceived(float damageAmount)
    {
        if (isTeleporting) return;

        accumulatedDamage += damageAmount;

        if (accumulatedDamage >= damageThreshold)
        {
            StartCoroutine(TeleportRoutine());
            accumulatedDamage -= damageThreshold;
        }
    }

    // ---------------------------------------------------------
    // LOGIC TELEPORT
    // ---------------------------------------------------------
    IEnumerator TeleportRoutine()
    {
        isTeleporting = true;

        anim.SetTrigger("teleport_start");
        if (teleportVFX) Instantiate(teleportVFX, transform.position, Quaternion.identity);

        yield return new WaitForSeconds(0.5f);

        if (sr) sr.enabled = false;

        Vector2 newPos = GetRandomPositionOnScreen();
        // Reset vị trí về đúng trục fixedY trước khi hiện ra
        transform.position = newPos;

        yield return new WaitForSeconds(0.2f);

        if (sr) sr.enabled = true;
        if (teleportVFX) Instantiate(teleportVFX, transform.position, Quaternion.identity);
        anim.SetTrigger("teleport_end");

        LookAtPlayer();

        yield return new WaitForSeconds(0.3f);
        isTeleporting = false;
    }

    Vector2 GetRandomPositionOnScreen()
    {
        float randX = Random.Range(screenPadding, 1f - screenPadding);
        Vector3 viewPos = new Vector3(randX, 0.5f, 10f);
        Vector2 worldPos = Camera.main.ViewportToWorldPoint(viewPos);

        // Luôn đảm bảo vị trí gốc là fixedY
        worldPos.y = fixedY;

        return worldPos;
    }

    void LookAtPlayer()
    {
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(-1, 1, 1);
        else
            transform.localScale = new Vector3(1, 1, 1);
    }
}