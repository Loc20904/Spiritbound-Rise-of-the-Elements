using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;

    private Vector3 originalPos;
    private float shakeTimer;
    private float shakeAmount;

    void Awake()
    {
        instance = this;
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

            transform.localPosition = originalPos;
        }
    }

    public void Shake(float duration, float amount)
    {
        shakeTimer = duration;
        shakeAmount = amount;
    }

    public void ShakeLight()
    {
        Shake(0.2f, 0.1f); // Rung nh?, nhanh
    }

    public void ShakeHeavy()
    {
        Shake(1.8f, 0.3f); // Rung m?nh, lâu (Dùng cho Boss d?m chân)
    }
}