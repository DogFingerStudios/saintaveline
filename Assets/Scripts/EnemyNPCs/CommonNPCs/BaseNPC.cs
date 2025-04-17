using UnityEngine;
using System.Collections.Generic;

public class BaseNPC : MonoBehaviour, IHasHealth
{
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

    [SerializeField]
    EntityTraits _entityTraits = new EntityTraits();
    public EntityTraits EntityTraits 
    {
        get => _entityTraits;
        set => _entityTraits = value;
    }

    [SerializeField] // in case Dictionary ever becomes serialized
    private Dictionary<GameObject, RelationshipTraits> _relationships = new();
    public Dictionary<GameObject, RelationshipTraits> Relationships 
    {
        get => _relationships;
        set => _relationships = value;
    }

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

    public Transform target;

    public void setState(NPCState state)
    {
        stateMachine.SetState(state);
    }

    float IHasHealth.TakeDamage(float damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;
        return Health;
    }

    float IHasHealth.Heal(float amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;
        return Health;
    }

    protected NPCStateMachine stateMachine = new NPCStateMachine();
    public NPCStateMachine StateMachine => stateMachine;

}
