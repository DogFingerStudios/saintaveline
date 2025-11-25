using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public enum MessageType
{
    Info,
    Warning,
    Error
}

public struct MessageEntry
{
    public string Text;
    public MessageType Type;
    public MessageEntry(string text, MessageType type)
    {
        Text = text;
        Type = type;
    }
}

public sealed class BottomTypewriter : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _text;

    [Header("Typing")]
    [SerializeField] private float _charactersPerSecond = 50f;
    [SerializeField] private float _postMessageHoldSeconds = 1.0f;
    [SerializeField] private bool _useUnscaledTime = true;

    [Header("Behavior")]
    [SerializeField] private bool _escSkipsToFullThenClears = true;
    [SerializeField] private float _fadeSeconds = 0.12f;

    [Header("Events")]
    [SerializeField] private UnityEvent<char> _onTypedChar;
    [SerializeField] private UnityEvent<string> _onMessageShown;

    private readonly Queue<MessageEntry> _queue = new Queue<MessageEntry>();
    private Coroutine _runner;
    private TextDisplayer _textDisplayer;
    private bool _isVisible;
    private bool _isTyping;
    private bool _wasEscPressedOnceDuringCurrentMessage;

    public static BottomTypewriter Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _textDisplayer = new TextDisplayer();

        if (_canvasGroup == null)
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
            {
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }

        if (_text == null)
        {
            Debug.LogError("BottomTypewriter: Text reference not assigned.");
        }

        // Start hidden and clamped
        _text.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;
        if (_text != null)
        {
            _text.text = string.Empty;
            _text.maxVisibleCharacters = 0;
        }
        _isVisible = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEsc();
        }
    }

    public void Enqueue(string message, MessageType type = MessageType.Info)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        _queue.Enqueue(new MessageEntry(message, type));
        TryRun();
    }

    public void EnqueueWarning(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        _queue.Enqueue(new MessageEntry(message, MessageType.Warning));
        TryRun();
    }

    public void EnqueueError(string message)
    {
        ErrorMessage.Instance.ShowError(message);
    }

    public void DismissAll()
    {
        _queue.Clear();
        if (_runner != null)
        {
            StopCoroutine(_runner);
            _runner = null;
        }
        _isTyping = false;
        _wasEscPressedOnceDuringCurrentMessage = false;
        if (_text != null)
        {
            _text.maxVisibleCharacters = 0;
            _text.text = string.Empty;
        }
        StartCoroutine(_textDisplayer.FadeTo(0f, _fadeSeconds, _text, _useUnscaledTime));
        _isVisible = false;
    }

    private void HandleEsc()
    {
        if (!_isVisible) return;

        if (_isTyping)
        {
            if (_escSkipsToFullThenClears)
            {
                if (!_wasEscPressedOnceDuringCurrentMessage)
                {
                    _text.maxVisibleCharacters = _text.textInfo.characterCount;
                    _wasEscPressedOnceDuringCurrentMessage = true;
                    return;
                }
                else
                {
                    DismissAll();
                    return;
                }
            }
            else
            {
                DismissAll();
                return;
            }
        }
        else
        {
            DismissAll();
        }
    }

    private void TryRun()
    {
        if (_runner == null && _queue.Count > 0)
        {
            _runner = StartCoroutine(RunQueue());
        }
    }

    private IEnumerator RunQueue()
    {
        while (_queue.Count > 0)
        {
            var entry = _queue.Dequeue();
            PrepareHidden(entry);

            if (!_isVisible)
            {
                yield return _textDisplayer.FadeTo(1f, _fadeSeconds, _text, _useUnscaledTime);
                _isVisible = true;
            }

            yield return RevealCurrentText();

            float hold = _postMessageHoldSeconds;
            float t = 0f;
            while (t < hold)
            {
                t += _textDisplayer.DeltaTime(_useUnscaledTime);
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    DismissAll();
                    yield break;
                }
                yield return null;
            }
        }

        yield return _textDisplayer.FadeTo(0f, _fadeSeconds, _text, _useUnscaledTime);
        _isVisible = false;
        _runner = null;
    }

    private void PrepareHidden(MessageEntry entry)
    {
        _text.color = entry.Type switch
        {
            MessageType.Info => Color.white,
            MessageType.Warning => Color.yellow,
            MessageType.Error => Color.red,
            _ => Color.white
        };
        _text.maxVisibleCharacters = 0;     // Clamp first
        _text.text = entry.Text;            // Then assign
        _text.ForceMeshUpdate();
    }

    private IEnumerator RevealCurrentText()
    {
        _isTyping = true;
        _wasEscPressedOnceDuringCurrentMessage = false;

        int total = _text.textInfo.characterCount;
        if (total <= 0)
        {
            _isTyping = false;
            yield break;
        }

        float cps = Mathf.Max(1f, _charactersPerSecond);
        float perChar = 1f / cps;

        for (int i = 0; i < total; i++)
        {
            _text.maxVisibleCharacters = i + 1;

            if (_onTypedChar != null)
            {
                char c = _text.text[i];
                _onTypedChar.Invoke(c);
            }

            float t = 0f;
            while (t < perChar)
            {
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    if (_escSkipsToFullThenClears && !_wasEscPressedOnceDuringCurrentMessage)
                    {
                        _text.maxVisibleCharacters = total;
                        _wasEscPressedOnceDuringCurrentMessage = true;
                        i = total - 1;
                        break;
                    }
                    else
                    {
                        DismissAll();
                        yield break;
                    }
                }

                t += _textDisplayer.DeltaTime(_useUnscaledTime);
                yield return null;
            }
        }

        _isTyping = false;
        if (_onMessageShown != null)
        {
            _onMessageShown.Invoke(_text.text);
        }
    }
}