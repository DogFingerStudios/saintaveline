using System;
using UnityEngine;

[Serializable]
public class RelationshipTraits
{
    [Range(-1f, 1f)] public float TrustToward = 0f;
    [Range(0f, 1f)] public float Love = 0f;
    [Range(0f, 1f)] public float FearOf = 0f;
}
