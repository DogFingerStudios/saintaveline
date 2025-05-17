using UnityEngine;

public class NPCDeathState : NPCState
{
    public override void Enter()
    {
        Debug.Log($"{this.NPC.name} has died.");
        Rigidbody rb = this.NPC!.GetComponent<Rigidbody>();
        if  (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.constraints = RigidbodyConstraints.None; 
            rb.linearDamping = 2f;  
            rb.angularDamping = 1f;  
            rb.AddTorque(Vector3.right * 5f, ForceMode.Impulse);
        }

        UnityEngine.AI.NavMeshAgent navAgent = this.NPC!.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (navAgent != null)
        {
            navAgent.enabled = false;
        }
    }

    public override INPCState? Update()
    {
        // Nothing to do in the death state
        return null;
    }

    public override void Exit()
    {
        // Nothing to do in the death state
    }
}
