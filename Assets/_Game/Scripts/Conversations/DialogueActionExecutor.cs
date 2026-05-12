using UnityEngine;
using System.Collections.Generic;

// AI: Executes dialogue actions based on enum type - acts as centralized action dispatcher
public class DialogueActionExecutor : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _npcManager;

    // AI: Dictionary to track NPC disposition scores for dynamic behavior
    private Dictionary<string, int> _npcDispositionScores = new Dictionary<string, int>();
    private Dictionary<string, bool> _storyFlags = new Dictionary<string, bool>();

    public void ExecuteAction(DialogueAction action, GameObject npc)
    {
        if (action == null || action.ActionType == DialogueActionType.None)
        {
            return;
        }

        switch (action.ActionType)
        {
            case DialogueActionType.SetStoryFlag:
                SetStoryFlag(action.StringParam, true);
                break;

            case DialogueActionType.StartQuest:
                StartQuest(action.StringParam);
                break;

            case DialogueActionType.EndConversation:
                EndConversation(npc);
                break;

            case DialogueActionType.MakeNPCAggressive:
                MakeNPCAggressive(npc);
                break;

            case DialogueActionType.DropItem:
                DropItem(npc, action.StringParam);
                break;

            case DialogueActionType.UnlockDoor:
                UnlockDoor(action.StringParam);
                break;

            case DialogueActionType.TriggerAlarm:
                TriggerAlarm(npc);
                break;

            case DialogueActionType.SpawnEnemies:
                SpawnEnemies(action.StringParam, action.IntParam);
                break;

            case DialogueActionType.GiveItem:
                GiveItem(action.StringParam, action.IntParam);
                break;

            case DialogueActionType.TakeMoney:
                TakeMoney(action.IntParam);
                break;

            case DialogueActionType.ChangeNPCDisposition:
                ChangeNPCDisposition(npc, action.IntParam);
                break;
        }
    }

    // AI: Execute multiple actions in sequence
    public void ExecuteActions(List<DialogueAction> actions, GameObject npc)
    {
        foreach (var action in actions)
        {
            ExecuteAction(action, npc);
        }
    }

    private void SetStoryFlag(string flagName, bool value)
    {
        _storyFlags[flagName] = value;
        Debug.Log($"Story flag '{flagName}' set to {value}");
    }

    public bool GetStoryFlag(string flagName)
    {
        return _storyFlags.ContainsKey(flagName) && _storyFlags[flagName];
    }

    private void StartQuest(string questId)
    {
        Debug.Log($"Starting quest: {questId}");
        // AI: Hook into your quest system here
    }

    private void EndConversation(GameObject npc)
    {
        Debug.Log("Ending conversation");
        // AI: Hook into your dialogue UI controller
    }

    private void MakeNPCAggressive(GameObject npc)
    {
        Debug.Log($"NPC {npc.name} is now aggressive!");
        // AI: Change NPC state to hostile
        var npcController = npc.GetComponent<NPCController>();
        if (npcController != null)
        {
            npcController.SetAggressive(true);
        }
    }

    private void DropItem(GameObject npc, string itemId)
    {
        Debug.Log($"NPC dropping item: {itemId}");
        // AI: Spawn item at NPC location
    }

    private void UnlockDoor(string doorId)
    {
        Debug.Log($"Unlocking door: {doorId}");
        // AI: Find door by ID and unlock it
    }

    private void TriggerAlarm(GameObject npc)
    {
        Debug.Log("Alarm triggered!");
        // AI: Alert nearby NPCs or security system
    }

    private void SpawnEnemies(string enemyType, int count)
    {
        Debug.Log($"Spawning {count} enemies of type {enemyType}");
        // AI: Use your enemy spawn system
    }

    private void GiveItem(string itemId, int quantity)
    {
        Debug.Log($"Giving player {quantity}x {itemId}");
        // AI: Add to player inventory
    }

    private void TakeMoney(int amount)
    {
        Debug.Log($"Taking {amount} money from player");
        // AI: Deduct from player currency
    }

    public void ChangeNPCDisposition(GameObject npc, int delta)
    {
        string npcId = npc.GetInstanceID().ToString();

        if (!_npcDispositionScores.ContainsKey(npcId))
        {
            _npcDispositionScores[npcId] = 0;
        }

        _npcDispositionScores[npcId] += delta;

        Debug.Log($"NPC {npc.name} disposition changed by {delta}. New score: {_npcDispositionScores[npcId]}");

        // AI: Make NPC aggressive if disposition drops too low
        if (_npcDispositionScores[npcId] <= -10)
        {
            MakeNPCAggressive(npc);
        }
    }

    public int GetNPCDisposition(GameObject npc)
    {
        string npcId = npc.GetInstanceID().ToString();
        return _npcDispositionScores.ContainsKey(npcId) ? _npcDispositionScores[npcId] : 0;
    }
}

// AI: Placeholder for NPC controller interface
public class NPCController : MonoBehaviour
{
    public void SetAggressive(bool isAggressive)
    {
        // AI: Implement your NPC aggression logic
    }
}
