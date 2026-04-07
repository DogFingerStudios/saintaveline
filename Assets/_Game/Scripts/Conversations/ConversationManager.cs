using System;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; } = null!;

    [SerializeField] private PanelManager _panelManager = null!;

    private CharacterEntity _currentCharacter = null!;
    private DialogNodeSO _currentNode = null!;

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

        InputManager.Instance.RegisterInputHandler(InputState.Conversation, ProcessInput);
        _panelManager.DisableAll();
    }

    public void StartConversation(CharacterEntity character, ConversationSO conversation)
    {
        InputManager.Instance.SetInputState(InputState.Conversation);
        UIManager.Instance.SetState(false, CursorLockMode.None, true);

        _currentCharacter = character;
        _currentNode = conversation.RootLine;
        _panelManager.EnableAll();

        _panelManager.SetText(character, _currentNode.GetRandomLine());
        _panelManager.SetOptions(_currentNode.Options);
    }

    public void ProcessInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            _panelManager.DisableAll();
            InputManager.Instance.SetInputState(InputState.Gameplay);
            UIManager.Instance.SetState(true, CursorLockMode.Locked, false);
        }
    }
}
