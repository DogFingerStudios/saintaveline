using UnityEngine;
using System.Collections.Generic;

// AI: Example dialogue manager showing how to integrate all three action systems
public class DialogueManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _currentNPC;
    [SerializeField] private GameObject _player;

    [Header("Action Systems")]
    [SerializeField] private DialogueActionExecutor _enumActionExecutor;
    [SerializeField] private DialogueCommandSystem _commandSystem;

    [Header("Current Conversation")]
    [SerializeField] private ConversationSO _currentConversation;
    [SerializeField] private DialogNodeSO _currentNode;

    // AI: Start a conversation with an NPC
    public void StartConversation(ConversationSO conversation, GameObject npc)
    {
        _currentConversation = conversation;
        _currentNPC = npc;
        _currentNode = conversation.RootLine;

        DisplayCurrentNode();
    }

    // AI: Player selects a dialogue option
    public void SelectOption(int optionIndex)
    {
        if (_currentNode == null || optionIndex >= _currentNode.Options.Count)
        {
            return;
        }

        DialogNodeSO selectedOption = _currentNode.Options[optionIndex];

        // AI: ===== SOLUTION 1: Execute enum-based actions =====
        if (selectedOption.OnSelectedActions != null && selectedOption.OnSelectedActions.Count > 0)
        {
            if (_enumActionExecutor != null)
            {
                _enumActionExecutor.ExecuteActions(selectedOption.OnSelectedActions, _currentNPC);
            }
        }

        // AI: ===== SOLUTION 2: Execute ScriptableObject actions =====
        if (selectedOption.OnSelectedActionsSO != null && selectedOption.OnSelectedActionsSO.Count > 0)
        {
            DialogueActionContext context = new DialogueActionContext
            {
                NPC = _currentNPC,
                Player = _player,
                CurrentNode = selectedOption,
                Conversation = _currentConversation,
                Executor = _enumActionExecutor
            };

            foreach (var action in selectedOption.OnSelectedActionsSO)
            {
                if (action != null && action.CanExecute(context))
                {
                    action.Execute(context);
                }
            }
        }

        // AI: ===== SOLUTION 3: Execute command strings =====
        if (!string.IsNullOrWhiteSpace(selectedOption.CommandString))
        {
            if (_commandSystem != null)
            {
                // AI: Can execute multiple commands separated by semicolons
                string[] commands = selectedOption.CommandString.Split(';');
                foreach (string command in commands)
                {
                    _commandSystem.ExecuteCommand(command.Trim(), _currentNPC);
                }
            }
        }

        // AI: Advance to next node
        _currentNode = selectedOption;
        DisplayCurrentNode();
    }

    private void DisplayCurrentNode()
    {
        if (_currentNode == null)
        {
            EndConversation();
            return;
        }

        PhrasingRef randomLine = _currentNode.GetRandomLine();
        if (randomLine != null)
        {
            Debug.Log($"NPC: {randomLine.GetText()}");
        }

        // AI: Auto-advance if configured
        if (_currentNode.AutoAdvance && _currentNode.Options.Count > 0)
        {
            _currentNode = _currentNode.Options[0];
            DisplayCurrentNode();
        }
        else if (_currentNode.Options.Count == 0)
        {
            EndConversation();
        }
    }

    private void EndConversation()
    {
        Debug.Log("Conversation ended");
        _currentConversation = null;
        _currentNode = null;
        _currentNPC = null;
    }
}
