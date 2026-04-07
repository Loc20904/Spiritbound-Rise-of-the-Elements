using System;
using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    // CÁI CHUÔNG BÁO ĐỘNG: Bất cứ khi nào chỉ số đổi, event này sẽ reo lên
    public event Action OnStatsChanged;

    [Header("Combat Stats")]
    // 1. Giấu biến thật đi (backing field), dùng SerializeField để vẫn chỉnh được trên Inspector
    [SerializeField] private int _damage = 50;
    [SerializeField] private int _armor = 5;

    // 2. Tạo "cánh cửa" (Property) để truy cập và theo dõi thay đổi
    public int Damage
    {
        get => _damage;
        set
        {
            if (_damage != value) // Chỉ báo động nếu giá trị THỰC SỰ thay đổi
            {
                _damage = value;
                OnStatsChanged?.Invoke(); // Rung chuông!
            }
        }
    }

    public int Armor
    {
        get => _armor;
        set
        {
            if (_armor != value)
            {
                _armor = value;
                OnStatsChanged?.Invoke(); // Rung chuông!
            }
        }
    }

    [Header("Respawn")]
    public float respawnDelay = 2f;
    [SerializeField] private Transform respawnPoint;

    [Header("Hurt / i-frames")]
    [SerializeField] private float hurtInvincibleTime = 0.35f;
    private float nextHurtTime;

    private Animator anim;
    private PlayerController controller;
    private Rigidbody2D rb;

    private PlayerHealth health;

    private bool dead = false;
    private bool respawning = false;

    // Lấy tốc độ từ Controller. 
    // LƯU Ý: Nếu tốc độ bên Controller bị đổi, bạn cũng cần gọi OnStatsChanged?.Invoke()
    public int getSpeed()
    {
        if (controller != null)
            return controller.getSpeed(); // Giả sử controller.getSpeed() trả về int
        return 0;
    }

    // Hàm tiện ích để ép gọi check thủ công (ví dụ khi vừa load game xong)
    public void ForceTriggerStatsChange()
    {
        OnStatsChanged?.Invoke();
    }

    private void Start()
    {
        health = GetComponent<PlayerHealth>();
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        rb = GetComponent<Rigidbody2D>();

        if (health == null)
            Debug.LogError("[PlayerStats] Missing PlayerHealth on this GameObject!");
    }

    private void Update()
    {
        if (dead || respawning) return;
        if (health == null) return;

        if (health.currentHP <= 0)
            OnDead();
    }

    public void TakeDamage(int incomingDamage)
    {
        if (dead || respawning || health == null) return;

        if (Time.time < nextHurtTime) return;
        nextHurtTime = Time.time + hurtInvincibleTime;

        // Dùng biến _armor bên trong class
        int finalDamage = Mathf.Max(incomingDamage - _armor, 0);

        if (finalDamage > 0)
            health.TakeDamage(finalDamage, DamageType.Boss);

        Debug.Log($"[PlayerStats] Incoming:{incomingDamage} Armor:{_armor} Final:{finalDamage}");

        controller?.TakeHit();
    }

    private void OnDead()
    {
        if (dead) return;
        dead = true;

        if (anim != null) anim.SetBool("Isdead", true);
        if (controller != null) controller.enabled = false;

        StartCoroutine(RespawnRoutine());
    }

    private IEnumerator RespawnRoutine()
    {
        respawning = true;
        yield return new WaitForSeconds(respawnDelay);

        if (respawnPoint != null) transform.position = respawnPoint.position;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        if (health != null) health.ResetHealth();

        if (anim != null)
        {
            anim.SetBool("Isdead", false);
            anim.Play("Player_Idle", 0, 0f);
        }

        if (controller != null) controller.enabled = true;

        dead = false;
        respawning = false;
        nextHurtTime = Time.time + 0.5f;
    }

    // Hàm này được Unity tự động gọi mỗi khi có giá trị thay đổi trên Inspector
    private void OnValidate()
    {
        // Chúng ta chỉ nên rung chuông nếu game ĐANG CHẠY (Play mode).
        // Nếu game chưa chạy (đang thiết kế) mà rung chuông thì sẽ bị lỗi (NullReference)
        // vì SkillManager và các script khác chưa được khởi tạo.
        if (Application.isPlaying)
        {
            // Gọi hàm ép rung chuông mà chúng ta đã tạo ở bài trước
            ForceTriggerStatsChange();
        }
    }
}