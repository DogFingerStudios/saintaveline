using System;
using System.Collections;
using Unity.VisualScripting;
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
    
    [Header("Audio")]
    [Tooltip("Audio clip to play when the gun is fired.")]
    private AudioSource audioSource;
    public AudioClip[] gunshotSounds;

    private float nextFireTime = 0f;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;

        // set the volume dropoff curve
        AnimationCurve rolloff = new AnimationCurve();
        rolloff.AddKey(0f, 1f);    // Full volume at 0 distance
        rolloff.AddKey(10f, 0.8f); // 80% volume at 10 units
        rolloff.AddKey(30f, 0.3f); // 30% volume at 30 units
        rolloff.AddKey(50f, 0.15f);// almost silent at 50 units

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f; // 3D sound
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // More realistic falloff
        audioSource.playOnAwake = false;
        audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloff);
        audioSource.maxDistance = 50f; // Can tweak based on how far you want it heard
    }

    void Update()
    {
        if (Time.time >= nextFireTime)
        {
            Shoot();
            // get a random time between 0.5 and 1.5 seconds
            float randomTime = UnityEngine.Random.Range(0.5f, 1.5f);
            nextFireTime = Time.time + randomTime;
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

            if (hit.collider.GetComponent<IHasHealth>() != null)
            {
                hit.collider.GetComponent<IHasHealth>().TakeDamage(damage);
            }
        }
        else
        {
            StartCoroutine(FireRayEffect(firePoint.position + firePoint.forward * range));
        }

        audioSource.PlayOneShot(gunshotSounds[UnityEngine.Random.Range(0, gunshotSounds.Length)]);
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
