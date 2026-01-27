using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Hitboxes")]
    [SerializeField] private Collider2D hitboxJ;
    [SerializeField] private Collider2D hitboxK;
    [SerializeField] private Collider2D hitboxL;

    [Header("Active times")]
    [SerializeField] private float timeJ = 0.12f;
    [SerializeField] private float timeK = 0.15f;
    [SerializeField] private float timeL = 0.20f;

    private bool isAttacking;

    private void Start()
    {
        hitboxJ.enabled = false;
        hitboxK.enabled = false;
        hitboxL.enabled = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) TryAttack(hitboxJ, timeJ);
        if (Input.GetKeyDown(KeyCode.K)) TryAttack(hitboxK, timeK);
        if (Input.GetKeyDown(KeyCode.L)) TryAttack(hitboxL, timeL);
    }

    private void TryAttack(Collider2D box, float activeTime)
    {
        if (isAttacking) return;
        StartCoroutine(AttackRoutine(box, activeTime));
    }

    private IEnumerator AttackRoutine(Collider2D box, float activeTime)
    {
        isAttacking = true;

        // tắt hết cho chắc
        hitboxJ.enabled = false;
        hitboxK.enabled = false;
        hitboxL.enabled = false;

        box.enabled = true;
        yield return new WaitForSeconds(activeTime);
        box.enabled = false;

        isAttacking = false;
    }
}
