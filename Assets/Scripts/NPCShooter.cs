using UnityEngine;

public class NPCShooter : MonoBehaviour
{
    public Transform firePoint;
    public float fireRate = 1f;
    public float range = 50f;
    public float damage = 10f;
    public LayerMask targetMask;

    private float nextFireTime = 0f;

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void Shoot()
    {
        Debug.DrawRay(firePoint.position, firePoint.forward * range, Color.red, 0.5f);
        Debug.Log(gameObject.name + " is shooting!");

        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range, targetMask))
        {
            Debug.Log(gameObject.name + " hit: " + hit.collider.name);

            // Optional: call a damage script
            // hit.collider.GetComponent<Health>()?.TakeDamage(damage);
        }
    }
}
