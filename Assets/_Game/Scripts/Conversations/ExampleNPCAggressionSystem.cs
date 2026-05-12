using UnityEngine;

// AI: Example showing realistic NPC aggression system based on dialogue choices
public class ExampleNPCAggressionSystem : MonoBehaviour
{
    [Header("Example Scenario")]
    [SerializeField] private GameObject _shopkeeperNPC;
    [SerializeField] private ConversationSO _shopkeeperConversation;
    [SerializeField] private DialogueManager _dialogueManager;

    // AI: This example demonstrates three ways to implement the same feature:
    // "Shopkeeper becomes aggressive if player is repeatedly rude"

    void Start()
    {
        // AI: Start conversation when player interacts with NPC
        _dialogueManager.StartConversation(_shopkeeperConversation, _shopkeeperNPC);
    }

    // ========== EXAMPLE DIALOGUE TREE STRUCTURE ==========

    /*
     * CONVERSATION FLOW:
     * 
     * Shopkeeper: "Welcome to my shop! What can I get you?"
     * 
     * Options:
     * 1. [Polite] "I'd like to see your wares, please."
     *    → Actions: ChangeDisposition +1
     *    → Next: Show shop inventory
     * 
     * 2. [Neutral] "Just browsing."
     *    → Actions: (none)
     *    → Next: Continue conversation
     * 
     * 3. [Rude] "Your prices are a ripoff!"
     *    → Actions: ChangeDisposition -3
     *    → Next: Shopkeeper response (annoyed)
     * 
     * ---
     * 
     * If player continues being rude (disposition reaches -10):
     * 
     * Shopkeeper: "I've had enough of your attitude! Get out!"
     * → Actions: MakeAggressive; EndConversation
     * → NPC attacks player
     * 
     * ---
     * 
     * IMPLEMENTATION OPTIONS:
     * 
     * Option 1 - Enum Actions:
     * DialogNodeSO "RudeResponse"
     * └── OnSelectedActions
     *     └── DialogueAction
     *         ├── ActionType: ChangeNPCDisposition
     *         └── IntParam: -3
     * 
     * Option 2 - ScriptableObject Actions:
     * DialogNodeSO "RudeResponse"
     * └── OnSelectedActionsSO
     *     └── ChangeDispositionAction.asset
     *         ├── DispositionDelta: -3
     *         └── AggressionThreshold: -10
     * 
     * Option 3 - Command Strings:
     * DialogNodeSO "RudeResponse"
     * └── CommandString: "ChangeDisposition -3"
     * 
     * ---
     * 
     * MORE COMPLEX EXAMPLE - Multi-step persuasion:
     * 
     * DialogNodeSO "PersuadeToDropWeapon"
     * Enum Actions:
     *   1. ChangeDisposition +5
     * SO Actions:
     *   1. CheckSkillAction (Persuasion, DC 15)
     *   2. ConditionalAction
     *      └── OnSuccess: GiveItemAction (weapon)
     *      └── OnFailure: MakeAggressiveAction
     * Command Strings:
     *   "SetFlag persuasion_attempted; PlaySound persuade_success"
     * 
     */
}

// AI: Example of a conditional action for skill checks
[CreateAssetMenu(fileName = "ConditionalAction", menuName = "Game/Dialogue Actions/Conditional")]
public class ConditionalDialogueAction : DialogueActionSO
{
    [SerializeField] private DialogueActionSO _condition;
    [SerializeField] private DialogueActionSO _onSuccess;
    [SerializeField] private DialogueActionSO _onFailure;

    public override void Execute(DialogueActionContext context)
    {
        // AI: Execute condition check
        bool conditionMet = _condition != null && _condition.CanExecute(context);

        if (conditionMet)
        {
            _onSuccess?.Execute(context);
        }
        else
        {
            _onFailure?.Execute(context);
        }
    }
}

// AI: Example skill check action
[CreateAssetMenu(fileName = "SkillCheckAction", menuName = "Game/Dialogue Actions/Skill Check")]
public class SkillCheckAction : DialogueActionSO
{
    [SerializeField] private string _skillName = "Persuasion";
    [SerializeField] private int _difficultyClass = 15;

    private bool _lastCheckPassed = false;

    public override void Execute(DialogueActionContext context)
    {
        // AI: Roll skill check
        int playerSkill = GetPlayerSkill(context.Player, _skillName);
        int roll = Random.Range(1, 21); // d20
        int total = playerSkill + roll;

        _lastCheckPassed = total >= _difficultyClass;

        Debug.Log($"Skill Check: {_skillName} {total} vs DC {_difficultyClass} - {(_lastCheckPassed ? "SUCCESS" : "FAILURE")}");
    }

    public override bool CanExecute(DialogueActionContext context)
    {
        return _lastCheckPassed;
    }

    private int GetPlayerSkill(GameObject player, string skillName)
    {
        // AI: Hook into your character stat system
        return 5; // Placeholder
    }
}
