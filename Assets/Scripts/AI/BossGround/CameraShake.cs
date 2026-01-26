using UnityEngine;

public class CameraShake : MonoBehaviour
{
    // Singleton ?? d? g?i t? b?t c? ?âu n?u c?n (tùy ch?n)
    public static CameraShake instance;

    private Vector3 originalPos;
    private float shakeTimer;
    private float shakeAmount;

    void Awake()
    {
        instance = this;
        // L?u v? trí ban ??u c?a camera
        originalPos = transform.localPosition;
    }

    void Update()
    {
        if (shakeTimer > 0)
        {
            // Rung b?ng cách d?ch chuy?n ng?u nhiên trong hình c?u bán kính shakeAmount
            transform.localPosition = originalPos + Random.insideUnitSphere * shakeAmount;

            shakeTimer -= Time.deltaTime;
        }
        else
        {
            shakeTimer = 0f;
            // Tr? v? v? trí c? khi h?t rung
            transform.localPosition = originalPos;
        }
    }

    // --- HÀM NÀY S? ???C G?I T? TIMELINE ---
    public void Shake(float duration, float amount)
    {
        shakeTimer = duration;
        shakeAmount = amount;
    }

    // Hàm overload ??n gi?n ?? dùng cho Signal Receiver (ch? nh?n 1 tham s? ho?c không tham s?)
    // Timeline Signal ?ôi khi khó truy?n 2 tham s?, nên ta làm s?n các hàm c?ng
    public void ShakeLight()
    {
        Shake(0.2f, 0.1f); // Rung nh?, nhanh
    }

    public void ShakeHeavy()
    {
        Shake(1.8f, 0.3f); // Rung m?nh, lâu (Dùng cho Boss d?m chân)
    }
}