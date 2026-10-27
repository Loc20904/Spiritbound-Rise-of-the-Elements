using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    public float damageMultiplier = 1f;
    public LayerMask enemyLayer;
    private readonly HashSet<IDamageable> hitTargets = new HashSet<IDamageable>();
    public void BeginAttack() => hitTargets.Clear();
    private void OnEnable() => BeginAttack();
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;
        var owner = PlayerHealth.Resolve(this);
        var target = other.GetComponentInParent<IDamageable>();
        if (owner == null || !owner.CanAct || target == null || !hitTargets.Add(target)) return;
        target.TakeDamage(Mathf.Max(0, Mathf.RoundToInt(owner.Damage * damageMultiplier)));
    }
}
