using UnityEngine;
using System.Collections;

public class BotFlyAI : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 3f;
    public float moveTime = 2f;

    [Header("Time")]
    public float breathTime = 1.2f;
    public float idleTime = 1f;

    [Header("Check Points")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.2f);
    public LayerMask groundLayer;
    // public LayerMask obstacleLayer;

    [Header("Map Boundary")]
    [Tooltip("Background dùng làm vùng giới hạn map (dùng position + scale)")]
    public Transform mapBoundaryBackground;

    [Header("Frames")]
    public Sprite[] breathFrames;
    public Sprite[] moveFrames;
    public Sprite[] idleFrames;

    public Sprite[] moveUpFrames;    // ⭐ đi lên
    public Sprite[] moveDownFrames;  // ⭐ đi xuống
    public float frameRate = 0.12f;

    //[Header("Cast (giữ bot đứng yên khi thấy player)")]
    //[Tooltip("Nếu gán: khi player trong tầm + đang cast thì bot đứng yên, chỉ chạy Breath (firePoint không dịch chuyển)")]
    public CastSkill castSkill;

    Rigidbody2D rb;
    SpriteRenderer sr;

    LayerMask CheckMask => groundLayer; //| obstacleLayer

    public bool isGrounded =>
        groundCheck != null &&
        Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, CheckMask);

    public void Flip()
    {
        sr.flipX = !sr.flipX;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        if (castSkill == null)
            castSkill = GetComponent<CastSkill>();

        StartCoroutine(StateLoop());
    }

    // =========================================
    // MAIN LOOP
    // =========================================
    IEnumerator StateLoop()
    {
        while (true)
        {
            // Player trong tầm hoặc đang cast → đứng yên, chỉ Breath (firePoint/firePoint1 không dịch)
            while (castSkill != null && (castSkill.PlayerInRange || castSkill.IsCasting))
            {
                rb.linearVelocity = Vector2.zero;
                yield return Idle();
            }

            yield return Breath();
            yield return Move();
            yield return Idle();
        }
        
    }

    // =========================================
    IEnumerator Breath()
    {
        rb.linearVelocity = Vector2.zero;
        // Chạy hết frame breath rồi mới qua Move
        yield return PlayAnimOnce(breathFrames, breathTime);
    }

    // =========================================
    IEnumerator Idle()
    {
        rb.linearVelocity = Vector2.zero;
        // Chạy hết frame idle rồi mới quay lại Breath
        yield return PlayAnimOnce(idleFrames, idleTime);
    }

    // =========================================
    IEnumerator Move()
{
    Vector2 dir = GetRandomDirection();

    // ⭐ CHỌN FRAME THEO HƯỚNG
    Sprite[] currentFrames = moveFrames;

    if (Mathf.Abs(dir.y) > 0.8f) // gần như thẳng đứng
    {
        if (dir.y > 0)
            currentFrames = moveUpFrames;     // đi lên
        else
            currentFrames = moveDownFrames;   // đi xuống
    }

    // flip chỉ khi có X
    if (dir.x != 0)
        sr.flipX = dir.x < 0;

    float duration = (currentFrames != null && currentFrames.Length > 0)
        ? currentFrames.Length * frameRate
        : moveTime;
    float timer = 0f;
    int frameIndex = 0;

    while (timer < duration)
    {
        Vector2 vel = dir * moveSpeed;
        rb.linearVelocity = vel;

        // Check biên map theo scale của background → chạm biên thì Flip + đảo hướng
        if (mapBoundaryBackground != null)
        {
            float halfW = mapBoundaryBackground.localScale.x * 0.5f;
            float halfH = mapBoundaryBackground.localScale.y * 0.5f;
            float left  = mapBoundaryBackground.position.x - halfW;
            float right = mapBoundaryBackground.position.x + halfW;
            float down  = mapBoundaryBackground.position.y - halfH;
            float up    = mapBoundaryBackground.position.y + halfH;

            Vector2 p = transform.position;

            if (p.x <= left && dir.x < 0) { dir.x = -dir.x; Flip(); }
            if (p.x >= right && dir.x > 0) { dir.x = -dir.x; Flip(); }
            if (p.y <= down && dir.y < 0) dir.y = -dir.y;
            if (p.y >= up && dir.y > 0) dir.y = -dir.y;
        }

        if (currentFrames != null && currentFrames.Length > 0)
        {
            sr.sprite = currentFrames[frameIndex];
            frameIndex = (frameIndex + 1) % currentFrames.Length;
        }

        timer += frameRate;
        yield return new WaitForSeconds(frameRate);
    }

    rb.linearVelocity = Vector2.zero;
}


    // =========================================
    // PLAY ONCE (breath / move / idle)
    // =========================================
    IEnumerator PlayAnimOnce(Sprite[] frames, float fallbackDuration)
    {
        // Nếu không có frame thì chỉ đợi theo time để không phá flow
        if (frames == null || frames.Length == 0)
        {
            if (fallbackDuration > 0f)
                yield return new WaitForSeconds(fallbackDuration);
            yield break;
        }

        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameRate);
        }
    }

    // =========================================
    Vector2 GetRandomDirection()
    {
        Vector2[] dirs =
        {
            Vector2.up,
            Vector2.down,
            Vector2.left,
            Vector2.right,
            new Vector2(1,1).normalized,
            new Vector2(-1,1).normalized,
            new Vector2(1,-1).normalized,
            new Vector2(-1,-1).normalized
        };

        return dirs[Random.Range(0, dirs.Length)];
    }

    // =========================================
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (groundCheck != null)
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        if (mapBoundaryBackground != null)
        {
            Gizmos.color = Color.yellow;
            Vector3 c = mapBoundaryBackground.position;
            Vector3 s = mapBoundaryBackground.localScale;
            Gizmos.DrawWireCube(c, s);
        }
    }
}
