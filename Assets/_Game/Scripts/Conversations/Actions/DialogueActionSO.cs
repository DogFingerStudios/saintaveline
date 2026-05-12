using UnityEngine;

// AI: Abstract base class for all dialogue actions - enables polymorphic action system
public abstract class DialogueActionSO : ScriptableObject
{
    [SerializeField] private string _description;

    // AI: Override this to implement specific action behavior
    public abstract void Execute(DialogueActionContext context);

    // AI: Optional validation to check if action can be executed
    public virtual bool CanExecute(DialogueActionContext context)
    {
        return true;
    }
}

// AI: Context object passed to all actions - contains references to game systems
[System.Serializable]
public class DialogueActionContext
{
    public GameObject NPC;
    public GameObject Player;
    public DialogNodeSO CurrentNode;
    public ConversationSO Conversation;
    public DialogueActionExecutor Executor;

    // AI: Add more context as needed (inventory, quest system, etc.)
}
