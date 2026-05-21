#nullable enable
using Miniscript;
using System;
using System.IO;
using Unity.VisualScripting.YamlDotNet.Core;
using UnityEngine;

internal class MiniscriptImplementation
{
    [MiniscriptFunction("adjust_npc_mentalstate", 3)]
    public void AdjustNpcAttribute(CharacterEntity npc, string stat, double delta)
    {
        Console.WriteLine("arg1: " + npc.ToString() + " arg2: " + stat + " arg3: " + delta.ToString());
    }

    [MiniscriptFunction("echo", 1)]
    public void Echo(object arg)
    {
        Console.WriteLine($"Echo: {arg} (Type: {arg.GetType().Name})");
    }
}

public class ConversationManager : MonoBehaviour
{
    public static ConversationManager Instance { get; private set; } = null!;

    [SerializeField] private PanelManager _panelManager = null!;

    private CharacterEntity _currentCharacter = null!;
    private CharacterEntity _playerEntity = null!;
    private DialogNodeSO _currentNode = null!;

    private Miniscript.Miniscript? _vm;

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

    Miniscript.Scanner _scanner = new Miniscript.Scanner();
    Miniscript.Parser _parser = new Miniscript.Parser();

    public void SetNode(DialogNodeSO node)
    {
        _currentNode = node;
        _scanner.Tokens.Clear();
        _parser.Statements.Clear();

        using (TextReader sr = new StringReader(node.MiniscriptText))
        {
            _scanner.Scan(sr);
        }

        _parser.Parse(_scanner.Tokens);

        _vm = new(_parser.Statements, new MiniscriptImplementation());
        _vm.SpecialVariables["target"] = _currentCharacter;
        _vm.Run();

        var speaker = node.IsPlayerSpeaking ? _playerEntity : _currentCharacter;
        _panelManager.SetText(speaker, _currentNode.GetRandomLine());
        _panelManager.SetOptions(_currentNode.Options, (DialogNodeSO node) =>
        {
            this.SetNode(node);
        });
    }
}
