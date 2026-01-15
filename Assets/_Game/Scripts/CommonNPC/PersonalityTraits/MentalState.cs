using System;
using UnityEngine;

// dynamic modifiers of the entity's personality
[System.Serializable]
public class MentalState
{
    // The environment can control whether or not the NPCs should be able
    // to recover these states
    public Func<bool> ShouldCalmDown;
    public Func<bool> ShouldRegainComfort;

    [Tooltip("The entity's comfort level, ranging from -1 (uncomfortable) to 1 (comfortable)")]
    [Range(-1f, 1f)] public float Comfort = 0.5f;

    [Tooltip("How quickly the entity recovers comfort over time (higher = faster recovery)")]
    [Range(0f, 1f)] public float BaseComfortRate = 0.1f;

    // AI: The entity's panic level, ranging from -1 (panicked) to 1 (calm).
    [Tooltip("The entity's panic level, ranging from -1 (panicked) to 1 (calm)")]
    [Range(-1f, 1f)] public float Calmness = 0f;

    [Tooltip("How quickly the entity recovers calmness over time (higher = faster recovery)")]
    [Range(0f, 1f)] public float BaseCalmRate = 0.1f;

    private float _timer = 0f;
    public void Tick()
    {
        _timer += Time.deltaTime;
        if (_timer >= 1f)
        {
            adjustCalmness();
            adjustComfort();
            _timer = 0f;
        }
    }

    private void adjustCalmness()
    {
        if (ShouldCalmDown?.Invoke() != true) return;

        // AI: the more calm they are the faster they get calmer
        float calmMultiplier = Mathf.InverseLerp(-1f, 1f, Calmness);

        // AI: Comfort increases recovery speed (up to double), discomfort slows it down (down to zero)
        // AI: map from [-1, 1] → [0, 2] → then clamp to [0,1]
        float comfortMultiplier = Mathf.Clamp01(Comfort + 1f); 

        float actualRecovery = BaseCalmRate * (1f + calmMultiplier) * comfortMultiplier;

        Calmness = Mathf.Clamp(Calmness + actualRecovery, -1f, 1f);
    }

    private void adjustComfort()
    {
        if (ShouldRegainComfort?.Invoke() != true) return;

        // The more comfortable they are, the faster they settle into that state
        float comfortMultiplier = Mathf.InverseLerp(-1f, 1f, Comfort);

        // The calmer they are, the easier it is to recognize and enjoy comfort
        float calmnessMultiplier = Mathf.Clamp01(Calmness + 1f); // [-1,1] → [0,1]

        float actualRecovery = BaseComfortRate * (1f + comfortMultiplier) * calmnessMultiplier;

        Comfort = Mathf.Clamp(Comfort + actualRecovery, -1f, 1f);
    }
}