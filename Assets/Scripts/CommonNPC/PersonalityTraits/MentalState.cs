using UnityEngine;

// dynamic modifiers of the entity's personality
[System.Serializable]
public class MentalState
{
    [Tooltip("The entity's comfort level, ranging from -1 (uncomfortable) to 1 (comfortable)")]
    [Range(-1f, 1f)] public float Comfort = 0.5f;

    // The entity's panic level, ranging from -1 (calm) to 1 (panicked).
    [Tooltip("The entity's panic level, ranging from 0 (calm) to 1 (panicked)")]
    [Range(-1f, 1f)] public float Panic = 0f;
}