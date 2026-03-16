using UnityEngine;

public class QuaiAI : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f; // Tốc độ di chuyển cơ bản 
    
    [Header("Check Points")]
    public Transform groundCheck; // Điểm kiểm tra dưới chân xem có đang đứng trên đất không
    public Transform edgeGroundCheck; // Điểm kiểm tra phía trước xem có đất không (để tránh nhảy vực)

    public float checkRadius = 0.2f; // Bán kính kiểm tra cho obstacleCheck để phát hiện tường dễ hơn
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.2f); // Kích thước hộp kiểm tra dưới chân để phát hiện đất tốt hơn
    public Vector2 groundEdgeGroundcheck = new Vector2(0.8f, 0.2f); // Kích thước hộp kiểm tra phía trước để phát hiện đất (tránh nhảy vực)

    // Các lớp (layer) để AI nhận diện đâu là đất, đâu là vật cản
    public LayerMask groundLayer;


    [Header("Patrol / Idle")]
    public float patrolTime = 3f; // Thời gian di chuyển tuần tra trước khi chuyển sang trạng thái đứng yên
    public float idleTime = 2f; // Thời gian đứng yên trước khi chuyển sang trạng thái di chuyển tuần tra

    [Header("Animation")]
    public Sprite[] idleFrames; // Các khung hình khi đứng yên
    public Sprite[] runFrames; // Các khung hình khi chạy
    //public Sprite jumpFrame; // Khung hình khi nhảy
    public float frameRate = 0.1f; // Tốc độ chuyển khung hình

    EnemyChaseAI chaseAI; // Script xử lý đuổi theo người chơi
    enum AIState { Patrol, Idle } // Định nghĩa trạng thái của AI: đang tuần tra hay đang đứng yên
    AIState currentState = AIState.Patrol; // Trạng thái hiện tại, mặc định là đang tuần tra

    Rigidbody2D rb; // Thành phần vật lý để điều khiển chuyển động của AI
    SpriteRenderer sr; // Thành phần để hiển thị sprite và thay đổi khung hình khi cần thiết
    EnemyRangedAttack ranged; // Script xử lý tấn công tầm xa, dùng để kiểm tra nếu đang tấn công thì không di chuyển

    float stateTimer;  // Bộ đếm thời gian để chuyển đổi giữa trạng thái tuần tra và đứng yên
    
    float animTimer; // Bộ đếm thời gian để điều khiển tốc độ chuyển khung hình trong animation
    int frameIndex; // Chỉ số của khung hình hiện tại trong mảng animation (0, 1, 2...)
    bool facingRight = true; // Kiểm tra xem AI đang quay mặt sang phải hay không.true = quay phải, false = quay trái
    bool wasMoving; // Lưu trạng thái di chuyển ở frame trước để reset animation khi thay đổi trạng thái

    // ===== BIẾN CÔNG KHAI CHO CÁC SCRIPT KHÁC =====
    public bool FacingRight => facingRight; // Trả về hướng mặt hiện tại. các file dùng gồm: EnemyChaseAI.cs, EnemyRangedAttack.cs, PlayerController.cs (để biết khi nào cần quay mặt theo hướng di chuyển của player)
    public void FlipPublic() => Flip(); // Cho phép script khác gọi hàm quay mặt

    // ================= START =================
    void Start()
    {
        // Khởi tạo các thành phần
        rb = GetComponent<Rigidbody2D>(); // Lấy thành phần vật lý để điều khiển chuyển động
        sr = GetComponent<SpriteRenderer>(); // Lấy thành phần hiển thị để thay đổi khung hình khi cần thiết
        ranged = GetComponent<EnemyRangedAttack>(); // Lấy script tấn công tầm xa để kiểm tra trạng thái tấn công
        chaseAI = GetComponent<EnemyChaseAI>(); // Lấy script đuổi theo để kiểm tra trạng thái đuổi theo

        stateTimer = patrolTime; // Bắt đầu với thời gian tuần tra
        ResetAnimation(); // Đưa animation về trạng thái ban đầu
    }

    // ================= UPDATE =================
    void Update()
    {
        // ===== HỆ THỐNG ĐUỔI THEO (CHASE) =====
        if (chaseAI != null) //nếu chaseAI tồn tại (đã gắn script EnemyChaseAI vào cùng GameObject) thì mới thực hiện logic đuổi theo, tránh lỗi nếu không có script này
        {
            chaseAI.Tick(); // Cập nhật logic đuổi theo mỗi khung hình (BẮT BUỘC GỌI TRƯỚC)
                            // lý do bắt buộc gọi trước là vì nếu đang trong trạng thái đuổi theo thì sẽ có logic di chuyển đặc biệt, nên cần cập nhật trạng thái đuổi theo trước khi xử lý di chuyển chung ở dưới.
                            // Nếu không gọi chaseAI.Tick() trước thì sẽ không cập nhật được trạng thái đuổi theo mới nhất và có thể dẫn đến việc AI không phản ứng đúng khi player vừa rời khỏi tầm bắn. 

            if (chaseAI.IsChasing)
            {
                int dir = chaseAI.ChaseDirection;// dùng để biết hướng di chuyển khi đang đuổi theo player
                                                 // (1 = phải, -1 = trái, 0 = không di chuyển).
                                                 // Nếu dir = 0 thì sẽ không di chuyển dù vẫn đang trong trạng thái đuổi theo.

                if (dir != 0)
                {
                    
                    if ((dir > 0) != facingRight)//nếu lớn hơn 0 là đang di chuyển sang phải, nếu nhỏ hơn 0 là đang di chuyển sang trái.
                            Flip();              //So sánh với facingRight để biết có cần quay mặt hay không. 
                        

                    float speed = moveSpeed * chaseAI.SpeedMultiplier; //tăng tốc độ dí người chơi
                    rb.linearVelocity = new Vector2(dir * speed, rb.linearVelocity.y);//di chuyển theo hướng chase 
                }

                CheckEnvironment(); // Kiểm tra vật cản/vực khi đang đuổi
                UpdateAnimation(); // Cập nhật hình ảnh
                return; // Thoát sớm nếu đang trong trạng thái đuổi theo
            }
        }

        

        // Kiểm tra xem người chơi có trong tầm bắn hay không
        bool playerInRange = ranged != null && ranged.PlayerInRange;

        // ===== XỬ LÝ KHI ĐANG TẤN CÔNG =====
        if (ranged != null && ranged.IsAttacking)//nếu player đang trong tầm bắn và đang thực hiện tấn công thì sẽ không di chuyển mà đứng yên để bắn
        {
            rb.linearVelocity = Vector2.zero; // Đứng yên để bắn
             
            return;
        }

        // ===== XỬ LÝ KHI PLAYER TRƯỚC MẶT =====
        if (playerInRange)//nếu player đang trong tầm bắn nhưng không phải đang tấn công thì sẽ đứng yên và quay mặt về phía player 
        {
            rb.linearVelocity = Vector2.zero; // Đứng yên khi player trong tầm

            // Quay mặt về phía player
            Vector2 playerPos = ranged.PlayerPosition;
            bool playerRight = playerPos.x > transform.position.x;

            if (playerRight != facingRight)
                Flip();

            currentState = AIState.Idle; // Chuyển sang đứng yên
            UpdateAnimation();
            return;
        }

        HandleState(); // Xử lý tuần tra / đứng nghỉ tự động

        if (currentState == AIState.Patrol)
            CheckEnvironment(); // Kiểm tra môi trường khi đang đi tuần

        UpdateAnimation(); // Cập nhật animation dựa trên vận tốc
    }

    // ================= STATE (QUẢN LÝ TRẠNG THÁI) =================
    void HandleState()
    {
        stateTimer -= Time.deltaTime;

        if (currentState == AIState.Patrol)
        {
            Move(); // Thực hiện di chuyển

            // Hết thời gian đi tuần -> Chuyển sang đứng yên
            if (stateTimer <= 0)
            {
                currentState = AIState.Idle;
                stateTimer = idleTime;
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                ResetAnimation();
            }
        }
        else // Trạng thái Idle
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

            // Hết thời gian nghỉ -> Quay lại đi tuần
            if (stateTimer <= 0)
            {
                currentState = AIState.Patrol;
                stateTimer = patrolTime;
                ResetAnimation();
            }
        }
    }

    // ================= MOVE (DI CHUYỂN) =================
    void Move()
    {
        float dir = facingRight ? 1 : -1;
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
    }

    // ================= ENV CHECK (KIỂM TRA MÔI TRƯỜNG) =================
    void CheckEnvironment()
    {
        LayerMask mask = groundLayer;

        // Các biến kiểm tra thực tế bằng tia (Raycast/Boxcast)
        bool isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, mask);
        
        bool groundAhead = Physics2D.OverlapBox(edgeGroundCheck.position, groundEdgeGroundcheck, 0, mask);

        
        // NẾU: Phía trước là vực (không có đất) VÀ Đang đứng trên đất
        if (!groundAhead && isGrounded)
        {
            if (chaseAI != null && chaseAI.IsChasing)
            {
                // Đang đuổi mà gặp vực -> Dừng lại (không nhảy xuống vực)
                chaseAI.StopChase();
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            else
            {
                // Đang tuần tra gặp vực -> Quay đầu đi hướng ngược lại
                Flip();
            }
        }
    }

    // ================= FLIP (QUAY MẶT) =================
    public void Flip()
    {
        facingRight = !facingRight;

        // Thay đổi Scale X để quay hình ảnh
        Vector3 s = transform.localScale;
        s.x = Mathf.Abs(s.x) * (facingRight ? 1 : -1);
        transform.localScale = s;
    }

    // ================= ANIMATION (XỬ LÝ HÌNH ẢNH) =================
    void UpdateAnimation()
    {
        // Kiểm tra xem có đang trên không không (để hiện animation nhảy)
        bool isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            checkRadius,
            groundLayer
        );

        // Kiểm tra xem có đang di chuyển thực tế không (vận tốc x > 0.1)
        bool isMoving = Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        // Nếu trạng thái di chuyển thay đổi (từ đứng sang chạy hoặc ngược lại) -> Reset bộ đếm
        if (isMoving != wasMoving)
        {
            ResetAnimation();
            wasMoving = isMoving;
        }

        if (isMoving)
            UpdateRunAnimation();
        else
            UpdateIdleAnimation();
    }

    void UpdateRunAnimation()
    {
        if (runFrames.Length == 0) return;

        animTimer += Time.deltaTime;
        if (animTimer >= frameRate)
        {
            animTimer = 0;
            frameIndex = (frameIndex + 1) % runFrames.Length; // Lặp lại các khung hình chạy
        }

        sr.sprite = runFrames[frameIndex]; //
    }

    void UpdateIdleAnimation()
    {
        if (idleFrames.Length == 0) return;

        animTimer += Time.deltaTime;
        if (animTimer >= frameRate)
        {
            animTimer = 0;
            frameIndex = (frameIndex + 1) % idleFrames.Length; // Lặp lại các khung hình đứng yên
        }

        sr.sprite = idleFrames[frameIndex];
    }

    void ResetAnimation()
    {
        animTimer = 0;
        frameIndex = 0;
    }

    // ================= GIZMOS (VẼ ĐỂ DỄ DEBUG TRONG UNITY EDITOR) =================
    void OnDrawGizmos()
    {
        // Màu xanh: Kiểm tra đất dưới chân
        Gizmos.color = Color.green;
        if (groundCheck)
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);

        // Màu xanh lơ: Kiểm tra vực phía trước
        Gizmos.color = Color.cyan;
        if (edgeGroundCheck)
            Gizmos.DrawWireCube(edgeGroundCheck.position, groundEdgeGroundcheck);
    }
}
