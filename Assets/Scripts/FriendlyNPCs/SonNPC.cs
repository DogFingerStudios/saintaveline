using UnityEngine;


public class SonNPC : Interactable
{
    public Transform father;
    public float rotationSpeed = 90f;
    private NPCStateMachine stateMachine = new NPCStateMachine();

    public CommandMenu commandMenu;

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
    }

    private void Update()
    {
        stateMachine.Update(this);
    }
}
