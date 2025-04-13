using UnityEngine;


public class SonNPC : FriendlyNPC
{   
    private void Start()
    {
        stateMachine.SetState(new NPCIdleState(this));
    }

    private void Update()
    {
        stateMachine.Update();
    }
}
