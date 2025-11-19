using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EasterEggWatcher : MonoBehaviour
{
    [System.Serializable]
    public class TrackedStringEvent
    {
        // AI: The string to watch for (e.g., "capac", "help", etc.)
        public string StringToWatch;

        // AI: Event to invoke when this string is detected
        public UnityEvent OnStringTyped;
    }

    [SerializeField]
    private List<TrackedStringEvent> _trackedStrings = new List<TrackedStringEvent>();

    [SerializeField]
    private int _maxBufferLength = 32;

    // AI: Rolling buffer of the most recent typed characters
    private string _buffer = string.Empty;

    private void Update()
    {
        // AI: Get all characters typed this frame
        string input = Input.inputString;

        if (!string.IsNullOrEmpty(input))
        {
            foreach (char c in input)
            {
                // AI: Handle backspace
                if (c == '\b')
                {
                    if (_buffer.Length > 0)
                    {
                        _buffer = _buffer.Substring(0, _buffer.Length - 1);
                    }
                }
                else
                {
                    // AI: Ignore control characters like return, escape, etc.
                    if (!char.IsControl(c))
                    {
                        _buffer += c;

                        // AI: Trim buffer if it gets too long
                        if (_buffer.Length > _maxBufferLength)
                        {
                            _buffer = _buffer.Substring(_buffer.Length - _maxBufferLength);
                        }

                        // AI: Check if buffer now ends with any watched string
                        CheckBufferForMatches();
                    }
                }
            }
        }
    }

    private void CheckBufferForMatches()
    {
        for (int i = 0; i < _trackedStrings.Count; i++)
        {
            TrackedStringEvent tracked = _trackedStrings[i];

            if (string.IsNullOrEmpty(tracked.StringToWatch))
            {
                continue;
            }

            // AI: Case-insensitive compare so "CAPAC" also works
            if (_buffer.EndsWith(tracked.StringToWatch, System.StringComparison.OrdinalIgnoreCase))
            {
                if (tracked.OnStringTyped != null)
                {
                    tracked.OnStringTyped.Invoke();
                }

                // AI: Optional: clear buffer or leave it so overlapping detections can work
                _buffer = string.Empty;
            }
        }
    }
}
