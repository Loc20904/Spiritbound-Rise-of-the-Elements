using UnityEngine;

public class ProjectileUp : MonoBehaviour
{
    private Transform target;
    private float moveSpeed;

    private float distanceToTargetToDestroyProjectile = 1f;
    private void Update()
    {
        Vector3 moveDirNormalized = (target.position - transform.position).normalized;
        transform.position += moveDirNormalized * moveSpeed;

        if (Vector3.Distance(transform.position, target.position) < distanceToTargetToDestroyProjectile)
        {
            Destroy(gameObject);
        }
    }

    public void InitializeProjectileUp(Transform target, float moveSpeed)
    {
        this.target = target;
        this.moveSpeed = moveSpeed;
    }
}