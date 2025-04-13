using UnityEngine;

public class BaseNPC : MonoBehaviour
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
    public float Health = 100f;
    
    [SerializeField]
    public float MaxHealth = 100f;

    public Transform target;

    public float TakeDamage(float damage)
    {
        Health -= damage;
        if (Health < 0) Health = 0;
        return Health;
    }
    
    public float Heal(float amount)
    {
        Health += amount;
        if (Health > MaxHealth) Health = MaxHealth;
        return Health;
    }

    public void setState(NPCState state)
    {
        stateMachine.SetState(state);
    }

    protected NPCStateMachine stateMachine = new NPCStateMachine();
    public NPCStateMachine StateMachine => stateMachine;

}
