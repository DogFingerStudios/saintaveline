using UnityEngine;

// AI: Changes NPC disposition based on dialogue choice - enables reactive NPCs
[CreateAssetMenu(fileName = "ChangeDispositionAction", menuName = "Game/Dialogue Actions/Change NPC Disposition")]
public class ChangeDispositionAction : DialogueActionSO
{
    [SerializeField] private int _dispositionDelta = -1;
    [SerializeField] private int _aggressionThreshold = -10;

    public override void Execute(DialogueActionContext context)
    {
        if (context.NPC == null)
        {
            Debug.LogWarning("ChangeDispositionAction: NPC is null!");
            return;
        }

        if (context.Executor != null)
        {
            context.Executor.ChangeNPCDisposition(context.NPC, _dispositionDelta);

            // AI: Check if NPC should become aggressive
            int currentDisposition = context.Executor.GetNPCDisposition(context.NPC);
            if (currentDisposition <= _aggressionThreshold)
            {
                var npcController = context.NPC.GetComponent<NPCController>();
                if (npcController != null)
                {
                    npcController.SetAggressive(true);
                    Debug.Log($"{context.NPC.name} has become aggressive due to low disposition!");
                }
            }
        }
    }
}
