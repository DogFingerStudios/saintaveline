using UnityEngine;

public class CharacterEntity : GameEntity
{
    public override float Heal(float amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;
        onHealthChanged(Health);
        return Health;
    }

    public override float TakeDamage(float amount)
    {
        Health -= amount;
        if (Health < 0) Health = 0;
        onHealthChanged(Health);
        return Health;
    }
}