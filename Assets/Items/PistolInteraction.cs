#nullable enable
using UnityEngine;
using System.Collections;

public class PistolInteraction : ItemInteraction
{
    private Quaternion _defaultRotation;
    private Coroutine? _attackCoroutine;
    private PistolItemData? _pistolItemData;
    private AudioSource? _audioSource;

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

}
