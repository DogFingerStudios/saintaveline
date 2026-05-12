using UnityEngine;

// AI: Enum defining all possible dialogue actions
public enum DialogueActionType
{
    None,
    SetStoryFlag,
    StartQuest,
    EndConversation,
    MakeNPCAggressive,
    DropItem,
    UnlockDoor,
    TriggerAlarm,
    SpawnEnemies,
    GiveItem,
    TakeMoney,
    ChangeNPCDisposition
}

// AI: Data container for dialogue actions - serializable in Unity Inspector
[System.Serializable]
public class DialogueAction
{
    [SerializeField] private DialogueActionType _actionType = DialogueActionType.None;

    // AI: Generic parameters that can be used differently per action type
    [SerializeField] private string _stringParam;
    [SerializeField] private int _intParam;
    [SerializeField] private float _floatParam;
    [SerializeField] private GameObject _objectParam;

    public DialogueActionType ActionType => _actionType;
    public string StringParam => _stringParam;
    public int IntParam => _intParam;
    public float FloatParam => _floatParam;
    public GameObject ObjectParam => _objectParam;
}
