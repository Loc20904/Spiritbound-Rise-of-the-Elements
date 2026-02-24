using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    public GameObject damagePopupPrefab;

    void OnTakeDamage(DamageInfo info)
    {
        GameObject go = Instantiate(
            damagePopupPrefab,
            transform.position + Vector3.up * 1.5f,
            Quaternion.identity
        );

        go.GetComponent<DamagePopup>().Setup(info.damage);
    }
}
