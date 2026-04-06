using System;
using UnityEngine;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; } = null!;

    [SerializeField] private PanelManager _panelManager = null!;
    private PhrasingSO _currentDialogLine;

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

    public void StartConversation(ConversationSO conversation)
    {
        InputManager.Instance.SetInputState(InputState.Conversation);
        UIManager.Instance.SetState(false, CursorLockMode.None, true);

        _panelManager.EnableAll();
        _currentDialogLine = conversation.RootLine;
        _panelManager.SetText(conversation.GetCurrentLine());
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
