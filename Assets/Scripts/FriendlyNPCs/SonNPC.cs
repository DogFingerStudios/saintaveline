using UnityEngine;

public class SonNPC : MonoBehaviour
{
    public Transform father;
    public float rotationSpeed = 90f;

    private NPCStateMachine stateMachine = new NPCStateMachine();

    private void Start()
    {
        stateMachine.SetState(new NPCIdleState(), this);
    }

    private void Update()
    {
        stateMachine.Update(this);
    }
}
