using UnityEngine;
using TMPro;

public class PlayerHPNumber : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public TextMeshProUGUI hpText;

    void Start()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<PlayerHealth>();
    }

    void Update()
    {
        if (playerHealth == null) return;

        hpText.text = playerHealth.CurrentHP.ToString();
    }
}