#nullable enable
using System;
using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class Pistol1Entity : ItemEntity
{
    // TODO: improve the way that NPCs "hear" gunshots
    //public static event Action<Vector3> OnGunFired = null!;

    private Quaternion _defaultRotation;
    private Coroutine? _attackCoroutine;
    private PistolItemData? _pistolItemData;
    private AudioSource? _audioSource;
    private LineRenderer _lineRenderer;
    private Transform? _firePoint;
    private bool _canFire = true;

    // this is called AFTER the item is equipped
    public override void OnEquipped()
    {
        _defaultRotation = this.gameObject.transform.localRotation;
        if (_hitCollider) _hitCollider.enabled = false;
    }

    protected override void Awake()
    {
        base.Awake();

        if (this.ItemData == null)
        {
            throw new System.Exception("PistolInteraction: ItemData is null.");
        }

        _pistolItemData = this.ItemData as PistolItemData;
        if (_pistolItemData == null)
        {
            throw new System.Exception($"PistolInteraction: Item '{this.ItemData!.ItemName}' is not a PistolItemData.");
        }

        _audioSource = Instantiate(_pistolItemData!.AudioSourcePrefab);

        _firePoint = new GameObject("FirePoint").transform;
        _firePoint.SetParent(this.transform);
        _firePoint.localPosition = _pistolItemData.FirePoint;
        _firePoint.localRotation = Quaternion.Euler(0f, 0f, 0f);
        _firePoint.localScale = new Vector3(1f, 0.5555556f, 1f);

        _lineRenderer = this.gameObject.GetComponent<LineRenderer>();
        _lineRenderer.enabled = false;
        _lineRenderer.positionCount = 2;
        _lineRenderer.startWidth = 0.05f;
        _lineRenderer.endWidth = 0.05f;
        _lineRenderer.startColor = Color.black;
        _lineRenderer.endColor = Color.black;
    }

    public override void Attack()
    {
        if (!_canFire) return;
        _canFire = false;

        if (_attackCoroutine != null)
        {
            StopCoroutine(_attackCoroutine);
        }

        _attackCoroutine = StartCoroutine(AnimateAttack());
    }

    protected override void OnStartAttack()
    {
        base.OnStartAttack();
        Shoot();
        _audioSource!.PlayOneShot(_pistolItemData!.FireSound);
    }

    private IEnumerator AnimateAttack()
    {
        OnStartAttack();

        float recoilDuration = _pistolItemData!.RecoilDuration;
        float holdDuration = _pistolItemData!.HoldDuration;
        float returnDuration = _pistolItemData!.ReturnDuration;

        Quaternion startRot = _defaultRotation;
        Quaternion targetRot = _defaultRotation * Quaternion.Euler(-20f, 0f, 0f);

        float elapsed = 0f;

        // → Recoil: tilt back
        while (elapsed < recoilDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / recoilDuration;
            transform.localRotation = Quaternion.Slerp(startRot, targetRot, t);
            yield return null;
        }

        // ── Hold the recoil 
        yield return new WaitForSeconds(holdDuration);

        // ← Return: rotate back to default
        elapsed = 0f;
        while (elapsed < returnDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / returnDuration;
            transform.localRotation = Quaternion.Slerp(targetRot, startRot, t);
            yield return null;
        }

        // snap exactly back, end attack
        transform.localRotation = _defaultRotation;
        OnEndAttack();
        _canFire = true;
        _attackCoroutine = null;
    }

    void Shoot()
    {
        Assert.IsNotNull(this.OwnerEntity, "Pistol1Entity.Shoot: OwnerEntity is null.");

        var owner = this.OwnerEntity as CharacterEntity;
        Assert.IsNotNull(owner, "Pistol1Entity.Shoot: OwnerEntity is not a CharacterEntity.");

        Vector3 direction = owner!.DirectionToTarget(true);

        if (Physics.Raycast(_firePoint!.position, direction, out RaycastHit hit, _pistolItemData!.FireRange))
        {
            var entity = hit.collider.GetComponent<GameEntity>();
            if (entity != null)
            {
                entity.TakeDamage(_pistolItemData!.DamageScore);
            }

            StartCoroutine(FireRayEffect(hit.point));
        }
        else
        {
            StartCoroutine(FireRayEffect(_firePoint!.position + (direction * _pistolItemData.FireRange)));
        }
    }

    IEnumerator FireRayEffect(Vector3 hitPoint)
    {
        _lineRenderer.SetPosition(0, _firePoint!.position);
        _lineRenderer.SetPosition(1, hitPoint);
        _lineRenderer.enabled = true;

        yield return new WaitForSeconds(0.05f);

        _lineRenderer.enabled = false;
        StimulusBus.Emit2(new SoundStimulus
        {
            Position = this.transform.position,
            Kind = StimulusKind.Gunshot,
            HearingRange = _pistolItemData!.AudioRange
        });
    }
}
