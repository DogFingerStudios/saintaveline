using UnityEngine;

[CreateAssetMenu(fileName = "Conversation", menuName = "Game/Conversation")]
public class ConversationSO : ScriptableObject
{
    public DialogLineSO RootLine;
}
