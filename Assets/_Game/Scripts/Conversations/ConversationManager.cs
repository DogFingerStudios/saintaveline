using System;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; } = null!;

    [SerializeField] private PanelManager _panelManager = null!;

    private CharacterEntity _currentCharacter = null!;
    private CharacterEntity _playerEntity = null!;
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

        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            throw new Exception("Player GameObject not found. Make sure the Player has the 'Player' tag.");
        }

        _playerEntity = player.GetComponent<CharacterEntity>();
        if (_playerEntity == null)
        {
            throw new Exception("CharacterEntity script not found on Player. Make sure the Player has the CharacterEntity component.");
        }

    }

    public void StartConversation(CharacterEntity character, ConversationSO conversation)
    {
        InputManager.Instance.SetInputState(InputState.Conversation);
        UIManager.Instance.SetState(false, CursorLockMode.None, true);

        _currentCharacter = character;
        _currentNode = conversation.RootLine;
        _panelManager.EnableAll();
        this.SetNode(_currentNode);
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

    public void SetNode(DialogNodeSO node)
    {
        _currentNode = node;
        var speaker = node.IsPlayerSpeaking ? _playerEntity : _currentCharacter;
        _panelManager.SetText(speaker, _currentNode.GetRandomLine());
        _panelManager.SetOptions(_currentNode.Options, (DialogNodeSO node) =>
        {
            this.SetNode(node);
        });
    }
}
