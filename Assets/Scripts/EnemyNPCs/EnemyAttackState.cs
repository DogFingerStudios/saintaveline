#nullable enable

using UnityEngine;

[NPCStateTag("EnemyAttack")]
public class EnemyAttackState : NPCState
{  
    public EnemyAttackState(BaseNPC? npc = null) 
        : base(npc)
    {
        // nothing to do
    }

    public override void Enter()
    {
        Debug.Log("Enemy is now in attack state.");
    }

    public override void Exit()
    {
        Debug.Log("Enemy has exited the attack state.");
    }

    public override NPCStateReturnValue? Update()
    {
        float distance = Vector3.Distance(this.NPC!.transform.position, this.NPC.target.position);
        if (distance > this.NPC!.stopDistance)
        {
            // target is out of range, go back to last state
            return new NPCStateReturnValue(NPCStateReturnValue.ActionType.PopState);
        }

        return null;
    }
}
