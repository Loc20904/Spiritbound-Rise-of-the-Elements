using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    TextMeshPro text;   // ⭐ dùng TextMeshPro (KHÔNG UGUI)

    float life = 1f;

    void Awake()
    {
        text = GetComponent<TextMeshPro>(); // ⭐ đúng loại trong ảnh bạn
    }

    public void Setup(int damage)
    {
        text.text = "-" + damage;
    }

    void Update()
    {
        transform.position += Vector3.up * 2f * Time.deltaTime;

        life -= Time.deltaTime;
        if (life <= 0)
            Destroy(gameObject);
    }
}
