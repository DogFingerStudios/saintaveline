using UnityEngine;
using System.Collections.Generic;

// AI: Executes multiple actions in sequence - allows complex multi-step behaviors
[CreateAssetMenu(fileName = "CompositeAction", menuName = "Game/Dialogue Actions/Composite Action")]
public class CompositeDialogueAction : DialogueActionSO
{
    [SerializeField] private List<DialogueActionSO> _actions = new List<DialogueActionSO>();
    [SerializeField] private bool _stopOnFirstFailure = false;

    public override void Execute(DialogueActionContext context)
    {
        foreach (var action in _actions)
        {
            if (action == null)
            {
                continue;
            }

            if (!action.CanExecute(context))
            {
                if (_stopOnFirstFailure)
                {
                    Debug.LogWarning($"CompositeAction: Action {action.name} cannot execute. Stopping.");
                    break;
                }
                continue;
            }

            action.Execute(context);
        }
    }

    public override bool CanExecute(DialogueActionContext context)
    {
        if (_stopOnFirstFailure)
        {
            // AI: All actions must be executable
            foreach (var action in _actions)
            {
                if (action != null && !action.CanExecute(context))
                {
                    return false;
                }
            }
        }

        return true;
    }
}
