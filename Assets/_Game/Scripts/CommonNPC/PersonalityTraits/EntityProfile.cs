using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class EntityProfile
{
    [SerializeField]
    PersonalityProfile _personality;
    public PersonalityProfile Personality
    {
        get => _personality;
        set => _personality = value;
    }
    
    [SerializeField]
    MentalState _mentalState;
    public MentalState MentalState
    {
        get => _mentalState;
        set => _mentalState = value;
    }

    private Dictionary<GameObject, RelationshipTraits> _relationships = new();
    public Dictionary<GameObject, RelationshipTraits> Relationships 
    {
        get => _relationships;
        set => _relationships = value;
    }
}