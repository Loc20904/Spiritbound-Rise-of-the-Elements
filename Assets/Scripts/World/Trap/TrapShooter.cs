using UnityEngine;

public class TrapShooter : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrelab;
    [SerializeField] private Transform target;

    [SerializeField] private float shootRate;
    [SerializeField] private float projectileMoveSpeed;
    private float shootTimer;

    private void Update()
    {
        shootTimer -= Time.deltaTime;

        if (shootTimer <= 0)
        {
            shootTimer = shootRate;
            ProjectileUp projectile = Instantiate(projectilePrelab, transform.position, Quaternion.identity).GetComponent<ProjectileUp>();
            projectile.InitializeProjectileUp(target, projectileMoveSpeed);
        }
    }
}