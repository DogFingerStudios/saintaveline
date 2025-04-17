using System.ComponentModel;
using UnityEngine;

[System.Serializable]
public class EntityTraits
{
    [Description("How trustiong this entity is overall.")]
    [Range(-1f, 1f)] public float Trusting = 0f;

    [Description("How loving this entity is overall.")]
    [Range(0f, 1f)] public float Loving = 0.5f;

    [Description("How courageous this entity is overall.")]
    [Range(0f, 1f)] public float Courage = 0.5f;
}
