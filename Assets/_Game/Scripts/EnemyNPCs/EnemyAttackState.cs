#nullable enable

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[NPCStateTag("EnemyAttack")]
public class EnemyAttackState : NPCState
{  
    public float fireRate = 1f;
    public float range = 50f;
    public float damage = 10f;
    public float defaultDamage = 10f;

    [Header("Audio")]
    [Tooltip("Audio clip to play when the gun is fired.")]
    private AudioSource _audioSource;

    private float _nextFireTime = 0f;
    private LineRenderer _lineRenderer;
    private readonly GameEntity _targetEntity;

    public EnemyAttackState(BaseNPC npc, GameEntity target)
        : base(npc)
    {
        if (this.NPC!.Target == null)
        {
            throw new System.Exception("BaseNPC is not an EnemyNPC. Cannot enter attack state.");
        }

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

        _audioSource = this.NPC!.GetComponent<AudioSource>();
        _audioSource.spatialBlend = 1f; // 3D sound
        _audioSource.rolloffMode = AudioRolloffMode.Logarithmic; // More realistic falloff
        _audioSource.playOnAwake = false;
        _audioSource.SetCustomCurve(AudioSourceCurveType.CustomRolloff, rolloff);
        _audioSource.maxDistance = 50f; // Can tweak based on how far you want it heard

        _targetEntity = target;
    }

    public override void Enter()
    {
        // nothing to do
    }

    public override void Exit()
    {
        // nothing to do
    }

    public override NPCStateReturnValue? Update()
    {
        if (_targetEntity!.IsAlive == false)
        {
            // Target is dead, go back to idle state
            return new NPCStateReturnValue(NPCStateReturnValue.ActionType.PopState);
        }

        float distance = Vector3.Distance(this.NPC!.transform.position, this.NPC.Target.position);
        if (distance > this.NPC!.stopDistance)
        {
            // Target is out of range, go back to last state
            return new NPCStateReturnValue(NPCStateReturnValue.ActionType.PopState);
        }

        // turn in the direction of the Target
        Vector3 direction = this.NPC.Target.position - this.NPC.transform.position;
        direction.y = 0f; // Keep rotation flat
        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            this.NPC.transform.rotation = Quaternion.RotateTowards(
                this.NPC.transform.rotation,
                targetRotation,
                this.NPC.rotationSpeed * Time.deltaTime
            );
        }

        if (Time.time >= _nextFireTime)
        {
            this.NPC!.EquippedItem?.Attack();

            // get a random time between 0.5 and 1.5 seconds
            float randomTime = UnityEngine.Random.Range(0.5f, 1.5f);
            _nextFireTime = Time.time + randomTime;
        }

        return null;
    }
}
