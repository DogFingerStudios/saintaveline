using System;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; } = null!;

    // [SerializeField]
    // private GameObject _conversationPanel = null!;

    public void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void StartConversation(ConversationSO conversation)
    {
    
    }
}
