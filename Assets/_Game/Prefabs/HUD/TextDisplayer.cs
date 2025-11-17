using System.Collections;
using TMPro;
using UnityEngine;

public class TextDisplayer : MonoBehaviour
{
    public IEnumerator FadeTo(float target, float seconds, TextMeshProUGUI _text, bool _useUnscaledTime)
    {
        if (seconds <= 0f)
        {
            _text.alpha = target;
            yield break;
        }

        float start = _text.alpha;
        float t = 0f;

        while (t < seconds)
        {
            t += DeltaTime(_useUnscaledTime);
            _text.alpha = Mathf.Lerp(start, target, t / seconds);
            yield return null;
        }

        _text.alpha = target;
    }

    public float DeltaTime(bool _useUnscaledTime)
    {
        return _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }
}