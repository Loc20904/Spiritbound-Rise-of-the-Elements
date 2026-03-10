using UnityEngine;
using System.Collections;

public class PlayerFireDOT : MonoBehaviour
{
    Coroutine burnRoutine;

    public void ApplyBurn(int damagePerTick, float duration, float tickRate)
    {
        // Nếu đang cháy → reset
        if (burnRoutine != null)
            StopCoroutine(burnRoutine);

        burnRoutine = StartCoroutine(Burn(damagePerTick, duration, tickRate));
        Debug.Log("🔥 Player bị đốt!");
    }

    IEnumerator Burn(int dmg, float duration, float tickRate)
    {
        float timer = 0f;

        while (timer < duration)
        {
            // ⭐ Gửi damage với type FireDOT để hiển thị đúng màu (cam) trong DamagePopup
            PlayerHealth health = GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(dmg, DamageType.FireDOT);
            }
            else
            {
                // Fallback nếu không có PlayerHealth
                SendMessage("TakeDamage", dmg, SendMessageOptions.DontRequireReceiver);
            }

            yield return new WaitForSeconds(tickRate);
            timer += tickRate;
        }

        burnRoutine = null;
    }
}