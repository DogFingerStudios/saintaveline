using System;
using UnityEngine;

public abstract class GameEntity : MonoBehaviour
{
    [SerializeField] public float Health = 100f;
    [SerializeField] public float MaxHealth = 100f;

    private Transform _entityTransform = null!;
    private Vector3 _lastPosition = Vector3.zero;
    public Vector3 Velocity { get; private set; }

    protected virtual void Awake()
    {
        _entityTransform = this.transform;
    }

    public virtual void Update()
    {
        Velocity = (_entityTransform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = _entityTransform.position;
    }

    public abstract float TakeDamage(float amount);
    public abstract float Heal(float amount);
    public virtual bool IsAlive { get => Health > 0; }

    public event Action<float> OnHealthChanged;
    protected void RaiseOnHealthChanged(float health)
    {
        OnHealthChanged?.Invoke(health);
    }
}