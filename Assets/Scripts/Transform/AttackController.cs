using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class AttackController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Input")]
    public InputAction attackAction;

    [Header("Attack Settings")]
    public float runAttackDisableTime = 0.5f; // Đổi từ 30f (quá dài) xuống thời gian hợp lý hơn
    public float runAttackDashForce = 15f;    // Lực lướt tới khi tấn công
    public float runAttackDashDuration = 0.1f; // Thời gian lướt trước khi ra đòn
    private bool isAttackLocked = false;

    private void Awake()
    {
        if (attackAction == null || attackAction.bindings.Count == 0)
        {
            attackAction = new InputAction("Attack", binding: "<Keyboard>/j");
        }
    }

    private void OnEnable()
    {
        attackAction?.Enable();
    }

    private void OnDisable()
    {
        attackAction?.Disable();
        isAttackLocked = false; // BẮT BUỘC: Reset lại lock nếu form bị tắt (transform sang form khác)
    }

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Nhấn J để tấn công và chưa bị khóa
        if (attackAction.WasPressedThisFrame() && !isAttackLocked)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        // Kiểm tra xem nhân vật có đang nhảy (trên không) không qua thông số Animator do TransformController gửi
        bool isJumping = animator != null && animator.GetBool("isJumping");

        // Kiểm tra xem nhân vật có đang di chuyển nhanh ở trục X không
        bool isRunning = rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        if (isRunning && !isJumping)
        {
            // Đang Run VÀ Đang đứng dưới đất -> bật đoạn chém lướt RunAttack
            StartCoroutine(CoRunAttack());
        }
        else
        {
            // Đang đứng yên HOẶC Đang nhảy trên không -> chỉ chạy Attack bình thường
            if (animator != null)
            {
                animator.SetTrigger("tAttack");
            }
        }
    }

    private IEnumerator CoRunAttack()
    {
        isAttackLocked = true;

        // BƯỚC 1: Gọi Animation chém NGAY LÚC BẮT ĐẦU
        if (animator != null)
        {
            animator.SetTrigger("tRunAttack");
        }

        if (rb != null)
        {
            // Lấy hướng nhân vật (+1 hoặc -1) dựa theo scale X
            float direction = transform.localScale.x > 0 ? 1f : -1f;

            // Xoá trọng lực để nhân vật lướt mượt theo đường thẳng
            float originalGravity = rb.gravityScale;
            rb.gravityScale = 0f;

            // BƯỚC 2: Vừa chiếu Animation vừa lướt tới
            float timePassed = 0f;
            while (timePassed < runAttackDashDuration)
            {
                rb.linearVelocity = new Vector2(direction * runAttackDashForce, 0f);
                timePassed += Time.deltaTime;
                yield return null; 
            }

            // Dừng lướt và trả lại trọng lực
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            rb.gravityScale = originalGravity;
        }

        // BƯỚC 3: Chờ nốt thời gian tung đòn (sau khi trừ hao thời gian đã dùng để lướt)
        float remainingWait = runAttackDisableTime - runAttackDashDuration;
        if (remainingWait > 0)
        {
            yield return new WaitForSeconds(remainingWait);
        }

        isAttackLocked = false;
    }

    // Biến trạng thái để scripts khác như TransformController có thể hỏi xem Player có đang bị lock input hay không
    public bool IsAttackLocked()
    {
        return isAttackLocked;
    }
}
