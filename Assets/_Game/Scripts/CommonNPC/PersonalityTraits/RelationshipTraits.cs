using UnityEngine;

[System.Serializable]
public class RelationshipTraits
{
    [Tooltip("The entity's trust level towards the target, ranging from -1 (distrust) to 1 (trust)")]
    [Range(-1f, 1f)] public float TrustToward = 0f;

    [Tooltip("The entity's love level towards the target, ranging from -1 (hate) to 1 (love)")]
    [Range(-1f, 1f)] public float Love = 0f;

    [Tooltip("The entity's fear level towards the target, ranging from 0 (no fear) to 1 (maximum fear)")]
    [Range(-1f, 1f)] public float FearOf = 0f;
}
