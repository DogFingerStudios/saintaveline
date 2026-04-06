using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Conversationer
{
    [SerializeField]
    private List<ConversationSO> _conversations = new();
    public List<ConversationSO> Conversations
    {
        get => _conversations;
        set => _conversations = value;
    }

    public ConversationSO GetConversation()
    {
        if (Conversations.Count == 0) return null;
        return Conversations[0];
    }
}
