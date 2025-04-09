using UnityEngine;

public abstract class FriendlyNPC : Interactable
{
    public virtual float rotationSpeed {get; set;} = 90f;

    public Transform leader;
    public CommandMenu commandMenu;

    protected NPCStateMachine stateMachine = new NPCStateMachine();
}
