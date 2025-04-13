using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public class NPCShooter : MonoBehaviour
{
    public Transform firePoint;
    public float fireRate = 1f;
    public float range = 50f;
    public float damage = 10f;
    public float defaultDamage = 10f;
    public LayerMask targetMask;

    private float nextFireTime = 0f;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
    }
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
        if (Physics.Raycast(firePoint.position, firePoint.forward, out RaycastHit hit, range, targetMask))
        {
            StartCoroutine(FireRayEffect(hit.point));

            // get the distance from the fire point to the hit point
            float distance = Vector3.Distance(firePoint.position, hit.point);
            int damage = Mathf.RoundToInt(defaultDamage * (1 - (distance / range)));

            if (hit.collider.GetComponent<PlayerStats>() != null)
            {
                hit.collider.GetComponent<PlayerStats>()?.TakeDamage(damage);
            }
            else if (hit.collider.GetComponent<FriendlyNPC>() != null)
            {
                hit.collider.GetComponent<FriendlyNPC>()?.TakeDamage(damage);
            }
        }
        else
        {
            StartCoroutine(FireRayEffect(firePoint.position + firePoint.forward * range));
        }
    }

    IEnumerator FireRayEffect(Vector3 hitPoint)
    {
        lineRenderer.SetPosition(0, firePoint.position);
        lineRenderer.SetPosition(1, hitPoint);
        lineRenderer.enabled = true;

        yield return new WaitForSeconds(0.05f);

        lineRenderer.enabled = false;
    }
}
