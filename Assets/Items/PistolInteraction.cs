#nullable enable
using UnityEngine;
using System.Collections;

public class PistolInteraction : ItemInteraction
{
    private Quaternion _defaultRotation;
    private Coroutine? _attackCoroutine;
    private PistolItemData? _pistolItemData;
    private AudioSource? _audioSource;
    private Camera? _mainCamera;
    private LineRenderer _lineRenderer;

    // this is called AFTER the item is equipped
    public override void onEquipped()
    {
        _defaultRotation = this.gameObject.transform.localRotation;
        if (_hitCollider) _hitCollider.enabled = false;
    }

    protected override void Start()
    {
        base.Start();

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
        _mainCamera = Camera.main;
        _lineRenderer = this.gameObject.GetComponent<LineRenderer>();
    }

    public override void Attack()
    {
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

        // capture your start & target rotations
        Quaternion startRot = _defaultRotation;
        // tilt back 20° around the local X axis (you can flip the sign or axis)
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
        _attackCoroutine = null;
    }

    // DO THIS LATER TODAY - BRINGING CODE FROM ENEMYATTACKSTATE INTO THIS SCRIPT
    // TO ALLOW THE PLAYER TO SHOOT -- WE STILL NEED TO ADD THE RAYCAST EFFECT
    // TO DETECT HITS AND APPLY DAMAGE, AND WE STILL NEED TO DRAW THE LINE (I.E
    // THE BULLET). WE HAVE ADDED A FIREPOINT TO THE SCRIPTABLE OBJECT, AND NOW
    // WE NEED TO CALCULATE THE "FORWARD" DIRECTION 

    private Vector3 GetFireDirection()
    {
        Vector3 screenCenter = new Vector3(_mainCamera!.pixelWidth / 2f, _mainCamera.pixelHeight / 2f, 0f);
        Ray ray = _mainCamera.ScreenPointToRay(screenCenter);
        Vector3 targetPoint = ray.GetPoint(_pistolItemData!.FireRange);
        return (targetPoint - _pistolItemData!.FirePoint).normalized;
    }

    void Shoot()
    {
        Vector3 direction = GetFireDirection();

        if (Physics.Raycast(_pistolItemData!.FirePoint, direction, out RaycastHit hit, _pistolItemData!.FireRange))
        {
            StartCoroutine(FireRayEffect(hit.point));
        }

    //     var direction = this.NPC!.target.position - _firePoint.position;
        // if (Physics.Raycast(_firePoint.position, direction, out RaycastHit hit, range))
        // {
        //     this.NPC!.StartCoroutine(FireRayEffect(hit.point));

        //     // get the distance from the fire point to the hit point
        //     float distance = Vector3.Distance(_firePoint.position, hit.point);
        //     int damage = Mathf.RoundToInt(defaultDamage * (1 - (distance / range)));

        //     if (hit.collider.GetComponent<IHasHealth>() != null)
        //     {
        //         hit.collider.GetComponent<IHasHealth>().TakeDamage(damage);
        //     }
        // }
        //     else
        //     {
        //         this.NPC!.StartCoroutine(FireRayEffect(_firePoint.position + direction * range));
        //     }

        //     _audioSource.PlayOneShot(_gunshotSounds[UnityEngine.Random.Range(0, _gunshotSounds.Length)]);
    }

    IEnumerator FireRayEffect(Vector3 hitPoint)
    {
        _lineRenderer.SetPosition(0, _pistolItemData!.FirePoint);
        _lineRenderer.SetPosition(1, hitPoint);
        _lineRenderer.enabled = true;

        yield return new WaitForSeconds(0.05f);

        _lineRenderer.enabled = false;
        // // OnGunFired?.Invoke();
    }
}
