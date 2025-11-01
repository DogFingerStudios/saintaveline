using TMPro;
using UnityEngine;

public class TaskLabelController : MonoBehaviour
{
    public Color CompletedColor;
    public Color FailedColor;

    private Color _originalColor = Color.white;
    private Color _pulseColor = Color.yellow; // Close to white for pulse effect
    private float _pulseSpeed = 5.5f; // Speed of the pulsating animation
    private TextMeshProUGUI _textMesh;

    public void Init()
    {
        _textMesh = GetComponent<TextMeshProUGUI>();
    }

    public void SetText(string text, TaskState taskState)
    {
        if (_textMesh == null)
        {
            throw new System.Exception("TaskLabelController requires a TextMeshProUGUI component.");
        }

        switch (taskState)
        {
            case TaskState.Completed:
                _textMesh.text = "-  " + text;
                _textMesh.color = CompletedColor;
                _pulseSpeed = 0f; // Stop pulsing
            break;

            case TaskState.Failed:
                _textMesh.text = "-  " + text;
                _textMesh.color = FailedColor;
                _pulseSpeed = 0f; // Stop pulsing
            break;

            case TaskState.InProgress:
                _textMesh.text = "-  <b>" + text + "</b>";
                _originalColor = _textMesh.color;
                _pulseSpeed = 2f; // Start pulsing
            break;

            default:
            break;
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (_pulseSpeed > 0f)
        {
            float pulseValue = (Mathf.Sin(Time.time * _pulseSpeed) + 1f) / 2f; // AI: Oscillates between 0 and 1
            Color currentColor = Color.Lerp(_originalColor, _pulseColor, pulseValue);
            _textMesh.color = currentColor;
        }
    }
}
