using UnityEngine;


public class SonNPC : FriendlyNPC
{   
    public override void OnFocus()
    {
        // Optional: highlight outline, play sound, etc.
    }

    public override void OnDefocus()
    {
        // Cleanup when not hovered
    }

    public override void Interact()
    {
        commandMenu.Open(this);
    }

    private void Start()
    {
        stateMachine.SetState(new NPCIdleState(), this);
        // agent = GetComponent<UnityEngine.AI.NavMeshAgent>(); // Should be done from base
    }

    private void Update()
    {
        stateMachine.Update(this);
    }
}
