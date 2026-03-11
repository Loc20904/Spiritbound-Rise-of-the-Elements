using UnityEngine;

public class BossPillar : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 200f;
    private float currentHealth;

    [Header("Visuals")]
    public GameObject pillarVisual; // Kéo Sprite/Model cái trụ vào đây
    public Collider2D pillarCollider; // Kéo Collider của trụ vào đây (để tắt va chạm khi vỡ)
    public Animator aim;

    public bool IsBroken { get; private set; } = false;

    // Sự kiện để báo cho Manager biết trụ đã vỡ
    public System.Action OnPillarBroken;

    void Start()
    {
        Revive();
    }

    public void TakeDamage(float damage)
    {
        if (IsBroken) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            BreakPillar();
        }
    }

    void BreakPillar()
    {
        IsBroken = true;

        // Tắt hình ảnh và va chạm (giả vờ như đã biến mất)
        if (pillarVisual) pillarVisual.SetActive(false);
        if (pillarCollider) pillarCollider.enabled = false;

        // Hiệu ứng nổ
        aim.SetTrigger("isDeath");

        // Báo cho Manager biết
        OnPillarBroken?.Invoke();
    }

    public void Revive()
    {
        IsBroken = false;
        currentHealth = maxHealth;

        // Bật lại hình ảnh và va chạm
        if (pillarVisual) pillarVisual.SetActive(true);
        if (pillarCollider) pillarCollider.enabled = true;

        // Hiệu ứng hồi sinh (nếu có)
    }
}