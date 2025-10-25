using System;
using UnityEngine;

public abstract class GameEntity : MonoBehaviour
{
    [SerializeField] public float Health = 100f;
    [SerializeField] public float MaxHealth = 100f;

    public abstract float TakeDamage(float amount);
    public abstract float Heal(float amount);
    public virtual bool IsAlive { get => Health > 0; }

    public event Action<float> OnHealthChanged;
    protected void RaiseOnHealthChanged(float health)
    {
        OnHealthChanged?.Invoke(health);
    }
}