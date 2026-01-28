#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class BaseNPC : CharacterEntity, IHearingSensor
{
    private static WaitForSeconds _waitForSeconds0_4 = new WaitForSeconds(0.4f);

    // TODO: Come back in here and clean up these ugly variable names
    [SerializeField]
    [Tooltip("The AudioSource component for playing NPC sounds")]
    private AudioSource _audioSource = null!;
    public AudioSource AudioSource { get => _audioSource; }

    [SerializeField]
    EntityProfile _entityProfile = null!;
    public EntityProfile Profile
    {
        get => _entityProfile;
        set => _entityProfile = value;
    }

    private Animator _animator = null!;
    public Animator Animator
    {
        get => _animator;
        set => _animator = value;
    }

    [SerializeField]
    [Tooltip("The rate at which the NPC rotates towards the target")]
    public float rotationSpeed = 90f;

    [SerializeField]
    [Tooltip("The speed at which the NPC moves")]
    public float moveSpeed = 3.5f;

    [SerializeField]
    [Tooltip("The distance at which the NPC will detect the target")]
    public float DetectionDistance = 5f;

    [SerializeField]
    [Tooltip("The distance at which the NPC will stop moving towards the target")]
    public float stopDistance = 1f;

    // The target the NPC is interested in (e.g., the NPC this object is attacking)
    public Transform Target = null!;

    [HideInInspector]
    public Partnership Partnership = null!;
    
    [SerializeField]
    public List<BaseNPC> Partners = new();

    #region Interactable Interface Implementation

    public virtual string HelpText => $"{this.name}";
    public virtual void OnFocus() { }
    public virtual void OnDefocus() { }
    public virtual void Interact(GameEntity? interactor = null) { }

#endregion

#region State Management
    public void setState(NPCState state)
    {
        stateMachine.SetState(state);
    }

    public void PushState(NPCState state)
    {
        stateMachine.StateStack.Push(state);
    }

    public NPCState? PopState()
    {
        if (stateMachine.StateStack.Count > 0)
        {
            return stateMachine.StateStack.Pop();
        }

        return null;
    }
#endregion

    public override float TakeDamage(float damage)
    {
        base.TakeDamage(damage);
        if (Health <= 0) onDeath();
        return Health;
    }

    private void onDeath()
    {
        this.setState(new NPCDeathState(this));
    }

    protected NPCStateMachine stateMachine = new NPCStateMachine();
    public NPCStateMachine StateMachine => stateMachine;

    private void validatePartnership()
    {
        var partners = this.Partnership.Partners;
        if (partners.Count == 0) return;

        if (partners.Contains(this)) return;

        if (partners.Any(p => !p.Partnership.Partners.Contains(this)))
        {
            throw new Exception("Partnership validation failed: NPC is not included in partner's partnership list.");
        }
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    protected override void Awake()
    {
        base.Awake();
        validatePartnership();
        PartnershipManager.Instance.RegisterPartnership(this, Partners);
    }

    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
    }

    public void Panic()
    {
        StopCoroutine(BlinkTwiceCoroutine());
        StartCoroutine(BlinkTwiceCoroutine());
    }

    private IEnumerator BlinkTwiceCoroutine()
    {
        if (_animator == null)
        {
            yield break;
        }

        _animator.SetTrigger("RefuseBlink");
        yield return _waitForSeconds0_4; // Match animation clip duration
        _animator.SetTrigger("RefuseBlink");
        yield return _waitForSeconds0_4;
        
        // Optional: force back to Idle (if needed)
        _animator.Play("Idle");
    }

    protected override void Update()
    {
        if (stateMachine == null) return;
        stateMachine.Update();
    }

    private void OnEnable()
    {
        StimulusBus.Register(this);
        StimulusBus.OnSoundEmitted += HandleSound;
    }

    private void OnDisable()
    {
        StimulusBus.OnSoundEmitted -= HandleSound;
        StimulusBus.Unregister(this);
    }

    public Vector3 Position => transform.position;
    public virtual void HandleSound(SoundStimulus stim)
    {
        // string objectName = this.name;
        // Debug.Log($"Object {objectName} heard a {stim.Kind} at {stim.Position}");
    }

    public override Vector3 DirectionToTarget(bool addFuzziness = false)
    {
        Assert.IsNotNull(_entityProfile, "BaseNPC.DirectionToTarget: EntityProfile is null.");
        if (Target == null) return Vector3.zero;
        if (!addFuzziness) return (Target.position - transform.position).normalized;

        Vector3 direction = (Target.position - transform.position).normalized;

        float fuzziness = Mathf.Lerp(0.7f, 0f, (_entityProfile.MentalState.Calmness + 1f) / 2f);
        
        if (fuzziness > 0f)
        {
            direction += new Vector3(
                UnityEngine.Random.Range(-fuzziness, fuzziness),
                UnityEngine.Random.Range(-fuzziness, fuzziness),
                UnityEngine.Random.Range(-fuzziness, fuzziness)
            );

            direction.Normalize();
        }

        return direction;
    }
}