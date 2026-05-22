using UnityEngine;

[System.Serializable]
public class RelationshipTraits
{
    [Tooltip("The entity's trust level towards the target, ranging from -1 (distrust) to 1 (trust)")]
    [Range(-1f, 1f)] public float TrustToward = 0f;

    [Tooltip("How much warmth or attachment this entity feels toward the target, ranging from -1 (hostility) to 1 (affection)")]
    [Range(-1f, 1f)] public float Affection = 0f;

    [Tooltip("The entity's fear level towards the target, ranging from 0 (no fear) to 1 (maximum fear)")]
    [Range(-1f, 1f)] public float FearOf = 0f;

    [Tooltip("How suspicious this entity is of the target, ranging from -1 (gives benefit of the doubt) to 1 (expects deception or danger)")]
    [Range(-1f, 1f)] public float SuspicionOf = 0f;
}
