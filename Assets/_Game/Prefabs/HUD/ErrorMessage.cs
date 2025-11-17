using System.Collections;
using TMPro;
using UnityEngine;

public sealed class ErrorMessage : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _text;

    [Header("Behavior")]
    [SerializeField] private float _fadeSeconds = 0.12f;
    [SerializeField] private float _displaySeconds = 5.0f;
    [SerializeField] private bool _useUnscaledTime = true;

    private Coroutine _routine;
    private TextDisplayer _textDisplayer;

    public static ErrorMessage Instance { get; private set; }

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
            Debug.LogError("ErrorMessage: Text reference not assigned!");
            return;
        }

        _text.alpha = 0f;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        _text.text = string.Empty;
    }

    public void ShowError(string message)
    {

        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        if (_routine != null)
        {
            StopCoroutine(_routine);
        }

        _routine = StartCoroutine(ShowErrorRoutine(message));
    }

    //THIS FUNCTION IS TEMPORARY. Should be removed after the implementation of the text field into the Canvas_Main (prefab missing).
    public void InitErrEssentials()
    {
        this._canvasGroup = GameObject.Find("BottomMessageRoot").GetComponent<CanvasGroup>();
        GameObject btwText = GameObject.Find("MessageText");
        GameObject copy = Instantiate(
            btwText,
            new Vector3(btwText.transform.position.x, btwText.transform.position.y - 100f, btwText.transform.position.z),
            btwText.transform.rotation,
            btwText.transform.parent
        );
        copy.name = "ErrorText";
        this._text = copy.GetComponent<TextMeshProUGUI>();
    }

    private IEnumerator ShowErrorRoutine(string message)
    {
        _text.color = Color.red;
        _text.text = message;

        yield return _textDisplayer.FadeTo(1f, _fadeSeconds, _text, _useUnscaledTime);

        float t = 0f;
        while (t < _displaySeconds)
        {
            t += _textDisplayer.DeltaTime(_useUnscaledTime);
            yield return null;
        }

        yield return _textDisplayer.FadeTo(0f, _fadeSeconds, _text, _useUnscaledTime);

        _text.text = string.Empty;
        _routine = null;
    }
}