using System.Collections;
using UnityEngine;

public enum AttackType { J, K }

public class PlayerAttack : MonoBehaviour
{
    [Header("Hitboxes")]
    [SerializeField] private Collider2D hitboxJ;
    [SerializeField] private Collider2D hitboxK;

    [Header("Active times")]
    [SerializeField] private float timeJ = 0.12f;
    [SerializeField] private float timeK = 0.15f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        // đảm bảo tắt hết ngay từ đầu
        if (hitboxJ) hitboxJ.enabled = false;
        if (hitboxK) hitboxK.enabled = false;
    }

    /// <summary>
    /// Được PlayerController gọi khi bấm J (combo).
    /// </summary>
    public void PlayAttack(AttackType type)
    {
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(AttackRoutine(type));
    }

    private IEnumerator AttackRoutine(AttackType type)
    {
        // tắt hết cho chắc
        if (hitboxJ) hitboxJ.enabled = false;
        if (hitboxK) hitboxK.enabled = false;

        switch (type)
        {
            case AttackType.J:
                if (hitboxJ) hitboxJ.enabled = true;
                yield return new WaitForSeconds(timeJ);
                if (hitboxJ) hitboxJ.enabled = false;
                break;

            case AttackType.K:
                if (hitboxK) hitboxK.enabled = true;
                yield return new WaitForSeconds(timeK);
                if (hitboxK) hitboxK.enabled = false;
                break;
        }

        currentRoutine = null;
    }
}