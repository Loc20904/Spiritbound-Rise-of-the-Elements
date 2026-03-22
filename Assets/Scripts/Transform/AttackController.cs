using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class AttackController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody2D rb;

    [Header("Input")]
    public InputAction attackAction;
    public InputAction skillAction;
    public InputAction skillActionL;

    [Header("Attack Settings")]
    public float runAttackDisableTime = 0.5f; // Đổi từ 30f (quá dài) xuống thời gian hợp lý hơn
    public float runAttackDashForce = 15f;    // Lực lướt tới khi tấn công
    public float runAttackDashDuration = 0.1f; // Thời gian lướt trước khi ra đòn

    [Header("Normal Attack Hitbox J")]
    [Tooltip("Kéo object chứa BoxCollider chém thường vào đây")]
    public GameObject normalAttackHitbox;
    [Tooltip("Thời gian tồn tại của nhát chém thường (giây)")]
    public float normalAttackDuration = 0.3f;

    [Header("Form Animation States")]
    public string idleStateName = "Transform01_Idle";
    public string runStateName = "Transform01_Run";

    // Khai báo biến khoá đòn đánh
    private bool isAttackLocked = false;

    [Header("Skill 1 (Nhấn K)")]
    public float skillWindupTime = 0.5f; // Thời gian Natsu há miệng (Vd vạch 0:30 = 0.5 giây)
    public float skillFireDuration = 1.0f; // Thời gian lửa cháy để thu đòn

    [Tooltip("Cách 1: Nếu lửa là đạn bay ra ngoài. Kéo Prefab lửa vào đây")]
    public GameObject firePrefab;
    public Transform firePoint;

    [Tooltip("Cách 2: Nếu lửa dính liền trên người. Kéo cục lửa GameObject vào đây")]
    public GameObject skillHitboxK; // Kéo cái AttackHitBox[K] ở thư mục con vào đây nhé!

    [Header("Skill 2 (Nhấn L)")]
    public float skillL_WindupTime = 0.5f; 
    public float skillL_FireDuration = 1.0f; 

    [Tooltip("Cách 1: Kéo Prefab skill L vào đây")]
    public GameObject firePrefabL;
    public Transform firePointL;

    [Tooltip("Cách 2: Kéo GameObject skill L vào đây")]
    public GameObject skillHitboxL; 

    private void Awake()
    {
        // Đề phòng inspector đang lưu giá trị cũ (30s)
        runAttackDisableTime = Mathf.Min(runAttackDisableTime, 1f); 

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        if (attackAction == null || attackAction.bindings.Count == 0)
        {
            attackAction = new InputAction("Attack", binding: "<Keyboard>/j");
        }

        if (skillAction == null || skillAction.bindings.Count == 0)
        {
            skillAction = new InputAction("Skill", binding: "<Keyboard>/k");
        }

        if (skillActionL == null || skillActionL.bindings.Count == 0)
        {
            skillActionL = new InputAction("SkillL", binding: "<Keyboard>/l");
        }
    }

    private void OnEnable()
    {
        attackAction?.Enable();
        skillAction?.Enable();
        skillActionL?.Enable();
    }

    private void OnDisable()
    {
        attackAction?.Disable();
        skillAction?.Disable();
        skillActionL?.Disable();
        isAttackLocked = false; // BẮT BUỘC: Reset lại lock nếu form bị tắt (transform sang form khác)
        
        // Tắt luôn lửa và kiếm nếu đang xài dở mà bị đổi form
        if (skillHitboxK != null) skillHitboxK.SetActive(false); 
        if (skillHitboxL != null) skillHitboxL.SetActive(false); 
        if (normalAttackHitbox != null) normalAttackHitbox.SetActive(false);
    }

    void Update()
    {
        // Nhấn J để tấn công và chưa bị khóa
        if (attackAction.WasPressedThisFrame() && !isAttackLocked)
        {
            PerformAttack();
        }

        // Nhấn K để dùng Skill và chưa bị khóa
        if (skillAction != null && skillAction.WasPressedThisFrame() && !isAttackLocked)
        {
            PerformSkill();
        }

        // Nhấn L để dùng Skill 2 và chưa bị khóa
        if (skillActionL != null && skillActionL.WasPressedThisFrame() && !isAttackLocked)
        {
            PerformSkillL();
        }
    }

    private void PerformSkill()
    {
        // Kiểm tra xem có đang nhảy không
        bool isJumping = animator != null && animator.GetBool("isJumping");
        
        if (!isJumping && animator != null)
        {
            // Khoá các input khác lại
            isAttackLocked = true; 
            
            // Yêu cầu Animator phát clip phun lửa
            animator.SetTrigger("tSkill"); 

            // Phát thủ công và chờ lửa bằng Coroutine:
            StartCoroutine(CoSkillWait());
        }
    }

    private IEnumerator CoSkillWait()
    {
        // 1. Chờ clip Skill chạy đến ngưng ở frame cuối cùng
        yield return new WaitForSeconds(skillWindupTime);

        // (Đã xoá lệnh đóng băng animator.speed, để nhân vật tự đứng im ở frame cuối cùng nhờ LoopTime=false)

        // 2. Khạc lửa ra
        if (skillHitboxK != null)
        {
            skillHitboxK.SetActive(true); 
        }
        else if (firePrefab != null && firePoint != null)
        {
            Instantiate(firePrefab, firePoint.position, transform.rotation);
        }

        // 3. Lửa cháy trong mồm
        yield return new WaitForSeconds(skillFireDuration);

        // 4. Rút lửa
        if (skillHitboxK != null)
        {
            skillHitboxK.SetActive(false);
        }

        // 5. Ép Natsu về trạng thái Chạy/Đứng bình thường
        if (animator != null)
        {
            if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                animator.Play(runStateName); 
            }
            else
            {
                animator.Play(idleStateName); 
            }
        }

        // 6. Mở nút khóa
        isAttackLocked = false; 
    }

    private void PerformSkillL()
    {
        // Kiểm tra xem có đang nhảy không
        bool isJumping = animator != null && animator.GetBool("isJumping");
        
        if (!isJumping && animator != null)
        {
            // Khoá các input khác lại
            isAttackLocked = true; 
            
            // Yêu cầu Animator phát clip Skill 2
            animator.SetTrigger("tSkillL"); 

            // Phát thủ công và chờ lửa bằng Coroutine:
            StartCoroutine(CoSkillWaitL());
        }
    }

    private IEnumerator CoSkillWaitL()
    {
        // 1. Chờ clip Skill chạy đến ngưng ở frame cuối cùng
        yield return new WaitForSeconds(skillL_WindupTime);

        // 3. Khạc lửa ra
        if (skillHitboxL != null)
        {
            skillHitboxL.SetActive(true); 
        }
        else if (firePrefabL != null && firePointL != null)
        {
            Instantiate(firePrefabL, firePointL.position, transform.rotation);
        }

        // 4. Lửa cháy trong mồm
        yield return new WaitForSeconds(skillL_FireDuration);

        // 5. Rút lửa
        if (skillHitboxL != null)
        {
            skillHitboxL.SetActive(false);
        }

        // 6. Ép Natsu về trạng thái Chạy/Đứng bình thường
        if (animator != null)
        {
            if (rb != null && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
            {
                animator.Play(runStateName); 
            }
            else
            {
                animator.Play(idleStateName); 
            }
        }

        // 7. Mở nút khóa
        isAttackLocked = false; 
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
            StartCoroutine(CoNormalAttack());
        }
    }

    private IEnumerator CoNormalAttack()
    {
        // Gọi Animation chém
        if (animator != null)
        {
            animator.SetTrigger("tAttack");
        }

        // Bật hitbox chém
        if (normalAttackHitbox != null) normalAttackHitbox.SetActive(true);

        // Chờ đúng thời lượng chém (vd 0.3s)
        yield return new WaitForSeconds(normalAttackDuration);

        // Rút kiếm, tắt hitbox
        if (normalAttackHitbox != null) normalAttackHitbox.SetActive(false);
    }

    private IEnumerator CoRunAttack()
    {
        isAttackLocked = true;

        // BƯỚC 1: Gọi Animation chém NGAY LÚC BẮT ĐẦU
        if (animator != null)
        {
            animator.SetTrigger("tRunAttack");
        }

        // MỚI THÊM: Bật kiếm lên lúc đang lao tới
        if (normalAttackHitbox != null) normalAttackHitbox.SetActive(true);

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

        // MỚI THÊM: Sau khi lao tới chém xong (hết Duration lướt), tắt kiếm đi
        if (normalAttackHitbox != null) normalAttackHitbox.SetActive(false);

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
