using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseNPC : MonoBehaviour, IHasHealth
{
    [SerializeField]
    [Tooltip("The AudioSource component for playing NPC sounds")]
    private AudioSource _audioSource;
    public AudioSource AudioSource { get => _audioSource; }

    [SerializeField] 
    EntityProfile _entityProfile;
    public EntityProfile Profile
    {
        get => _entityProfile;
        set => _entityProfile = value;
    }

    private Animator _animator;
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
    public float detectionDistance = 5f;

    [SerializeField]
    [Tooltip("The distance at which the NPC will stop moving towards the target")]
    public float stopDistance = 1f;

    [SerializeField]
    float _health = 100f;

    public float Health 
    {
        get => _health;
        set => _health = value;
    }

    [SerializeField]
    float _maxHealth = 100f;
    public float MaxHealth 
    {
        get => _maxHealth;
        set => _maxHealth = value;
    }

    [Header("Debug Health Slider")]
    private Transform _playerTransform;
    public Slider HealthSlider;
    public TextMeshProUGUI DistanceText;

    public Transform target;

    public void setState(NPCState state)
    {
        stateMachine.SetState(state);
    }

    public event Action<float> OnHealthChanged;

    public float TakeDamage(float damage)
    {
        Health -= damage;
        UpdateHealthSlider();

        if (Health < 0) Health = 0;
        this.OnHealthChanged?.Invoke(Health);
        if (Health <= 0)
        {
            onDeath();
        }
        // else
        // {
        //     // Play hurt animation or sound
        //     _animator.SetTrigger("Hurt");
        // }
        return Health;
    }

    private void onDeath()
    {
        Debug.Log($"{this.name} has died.");
        this.setState(new NPCDeathState(this));
    }

    float IHasHealth.Heal(float amount)
    {
        Health += amount;
        UpdateHealthSlider();

        if (Health > MaxHealth) Health = MaxHealth;
        this.OnHealthChanged?.Invoke(Health);
        return Health;
    }

    protected NPCStateMachine stateMachine = new NPCStateMachine();
    public NPCStateMachine StateMachine => stateMachine;

    protected virtual void Start()
    {
        _animator = GetComponent<Animator>();
        _audioSource = GetComponent<AudioSource>();
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform; // I don't like this

        SetUpHealthSlider();
    }

    public void Panic()
    {
         StartCoroutine(BlinkTwiceCoroutine());
    }

    private IEnumerator BlinkTwiceCoroutine()
    {
        if (_animator == null)
        {
            yield break;
        }

        _animator.SetTrigger("RefuseBlink");
        yield return new WaitForSeconds(0.4f); // Match animation clip duration
        _animator.SetTrigger("RefuseBlink");
        yield return new WaitForSeconds(0.4f);
        
        // Optional: force back to Idle (if needed)
        _animator.Play("Idle");
    }

    protected virtual void Update()
    {
        if (stateMachine == null) return;
        stateMachine.Update();
    }

    private void LateUpdate()
    {
        if (DistanceText != null)
        {
            float distance = Vector3.Distance(transform.position, _playerTransform.position);
            DistanceText.text = $"{distance:F2} m";
        }
    }

    private void SetUpHealthSlider()
    {
        if (HealthSlider == null)
        {
            Debug.LogError("HealthSlider not assigned on NPC: " + name);
            return;
        }

        HealthSlider.minValue = 0;
        HealthSlider.maxValue = MaxHealth;
        HealthSlider.value = Health;
    }

    private void UpdateHealthSlider()
    {
        HealthSlider.value = Health;
    }
}
