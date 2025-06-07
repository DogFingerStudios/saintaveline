#nullable enable

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[NPCStateTag("EnemyAttack")]
public class EnemyAttackState : NPCState
{  
    public Transform _firePoint = new GameObject("FirePoint").transform;
    public AudioClip[] gunshotSounds;

    public float fireRate = 1f;
    public float range = 50f;
    public float damage = 10f;
    public float defaultDamage = 10f;
    public LayerMask targetMask = LayerMask.GetMask("Player", "FriendlyNPC");

    [Header("Audio")]
    [Tooltip("Audio clip to play when the gun is fired.")]
    private AudioSource _audioSource;

    private float nextFireTime = 0f;
    private LineRenderer _lineRenderer;

    public EnemyAttackState(BaseNPC? npc = null) 
        : base(npc)
    {
        _firePoint.SetParent(this.NPC!.transform);
        _firePoint.localPosition = new Vector3(0.004f, 0.6019999f, 0.425f);
        _firePoint.localRotation = Quaternion.Euler(0f, 0f, 0f);
        _firePoint.localScale = new Vector3(1f, 0.5555556f, 1f);
    }

    public override void Enter()
    {
        _lineRenderer = this.NPC!.GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;

        // set the volume dropoff curve
        AnimationCurve rolloff = new AnimationCurve();
        rolloff.AddKey(0f, 1f);    // Full volume at 0 distance
        rolloff.AddKey(10f, 0.8f); // 80% volume at 10 units
        rolloff.AddKey(30f, 0.3f); // 30% volume at 30 units
        rolloff.AddKey(50f, 0.15f);// almost silent at 50 units

        _audioSource = this.NPC!.gameObject.AddComponent<AudioSource>();
        _audioSource.spatialBlend = 1f; // 3D sound
        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // More realistic falloff
        _audioSource.playOnAwake = false;
        _audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloff);
        _audioSource.maxDistance = 50f; // Can tweak based on how far you want it heard
    }

    public override void Exit()
    {
        Debug.Log("Enemy has exited the attack state.");
    }

    public override NPCStateReturnValue? Update()
    {
        float distance = Vector3.Distance(this.NPC!.transform.position, this.NPC.target.position);
        if (distance > this.NPC!.stopDistance)
        {
            // target is out of range, go back to last state
            return new NPCStateReturnValue(NPCStateReturnValue.ActionType.PopState);
        }

        if (Time.time >= nextFireTime)
        {
            Shoot();
            // get a random time between 0.5 and 1.5 seconds
            float randomTime = UnityEngine.Random.Range(0.5f, 1.5f);
            nextFireTime = Time.time + randomTime;
        }

        return null;
    }

    void Shoot()
    {
        Debug.Log("Enemy is from position: " + _firePoint.position);
        if (Physics.Raycast(_firePoint.position, _firePoint.forward, out RaycastHit hit, range, targetMask))
        {
            this.NPC!.StartCoroutine(FireRayEffect(hit.point));

            // get the distance from the fire point to the hit point
            float distance = Vector3.Distance(_firePoint.position, hit.point);
            int damage = Mathf.RoundToInt(defaultDamage * (1 - (distance / range)));

            if (hit.collider.GetComponent<IHasHealth>() != null)
            {
                hit.collider.GetComponent<IHasHealth>().TakeDamage(damage);
            }
        }
        else
        {
            this.NPC!.StartCoroutine(FireRayEffect(_firePoint.position + _firePoint.forward * range));
        }

        // _audioSource.PlayOneShot(gunshotSounds[UnityEngine.Random.Range(0, gunshotSounds.Length)]);
    }

    IEnumerator FireRayEffect(Vector3 hitPoint)
    {
        _lineRenderer.SetPosition(0, _firePoint.position);
        _lineRenderer.SetPosition(1, hitPoint);
        _lineRenderer.enabled = true;

        yield return new WaitForSeconds(0.05f);

        _lineRenderer.enabled = false;
        // // OnGunFired?.Invoke();
    }
}
