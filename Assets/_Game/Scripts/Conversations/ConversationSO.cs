using UnityEngine;

[CreateAssetMenu(fileName = "Conversation", menuName = "Game/Dialogs/Conversation")]
public class ConversationSO : ScriptableObject
{
    public DialogNodeSO RootLine;
}
