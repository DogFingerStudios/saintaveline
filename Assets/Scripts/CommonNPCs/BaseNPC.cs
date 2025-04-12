using UnityEngine;

public class BaseNPC : MonoBehaviour
{
    public Transform target;

    public void setState(NPCState state)
    {
        stateMachine.SetState(state, this);
    }

    protected NPCStateMachine stateMachine = new NPCStateMachine();
    public NPCStateMachine StateMachine => stateMachine;

}
