using UnityEngine;

[System.Serializable]
public class PersonalityProfile
{   
    [Range(-1f,1f)] public float Trusting = 0f;
    [Range(-1f,1f)] public float Loving = 0.5f;
    [Range(-1f,1f)] public float Courage = 0.5f;
    [Range(-1f,1f)] public float Decisiveness = 0.5f;
    [Range(-1f,1f)] public float Composure = 0.5f;
}