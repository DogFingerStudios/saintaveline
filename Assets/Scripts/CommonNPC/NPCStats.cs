using System;

public interface IHasHealth 
{
    float Health { get; set; }
    float MaxHealth { get; set; }

    float TakeDamage(float amount);
    float Heal(float amount);

    event Action<float> OnHealthChanged;
}
