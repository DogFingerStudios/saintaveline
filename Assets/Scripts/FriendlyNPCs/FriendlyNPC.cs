using Unity.VisualScripting;
using UnityEngine;

public abstract class FriendlyNPC : Interactable
{
    public float rotationSpeed = 90f;
    public Transform leader;
    public CommandMenu commandMenu;

    public void setState(NPCState state)
    {
        stateMachine.SetState(state, this);
    }
    
    protected NPCStateMachine stateMachine = new NPCStateMachine();
}
