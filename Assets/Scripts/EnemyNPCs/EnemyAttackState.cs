#nullable enable

using UnityEngine;

[NPCStateTag("EnemyAttack")]
public class EnemyAttackState : NPCState
{
   
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
        Debug.Log("Enemy is attacking.");
        return null;
    }
}
