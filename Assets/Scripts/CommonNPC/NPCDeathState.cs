using UnityEngine;

public class NPCDeathState : NPCState
{
    public NPCDeathState(BaseNPC baseNpc) : base(baseNpc)
    {
        // nothing to do
    }

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

    private float _timer = 0f;
    bool _destroyed = false;
    public override INPCState? Update()
    {
        _timer += 1.0F * Time.deltaTime;
        if (!_destroyed && _timer >= 5f)
        {
            _destroyed = true;
            GameObject.Destroy(this.NPC.gameObject);
        }

        return null;
    }

    public override void Exit()
    {
        // Nothing to do in the death state
    }
}
