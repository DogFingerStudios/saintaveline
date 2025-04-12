using Unity.VisualScripting;
using UnityEngine;

public abstract class FriendlyNPC : MonoBehaviour, Interactable
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

    public Transform target;
    public CommandMenu commandMenu;

    public string HelpText => "Press [E] to interact";

    public void OnFocus()
    {
        // Optional: highlight outline, play sound, etc.
    }

    public void OnDefocus()
    {
        // Cleanup when not hovered
    }

    public void Interact()
    {
        commandMenu.Open(this);
    }

    public void setState(NPCState state)
    {
        stateMachine.SetState(state, this);
    }

    protected NPCStateMachine stateMachine = new NPCStateMachine();
    public NPCStateMachine StateMachine => stateMachine;
}
