using UnityEngine;

[System.Serializable]
public class PersonalityProfile
{
    // for believing excuses, trusting others, and thinking “maybe this
    // man really is just trying to get medicine for his daughter”
    [Range(-1f,1f)] public float Trusting = 0f;

    // this will make a guard hesitate when the player pleads, or make
    // an enemy soften when hearing about family, or a collaborator feel
    // bad enough to reveal an escape route
    [Range(-1f, 1f)] public float Empathy = 0f;

    // for matters of intimidation, whether fear makes them fold, and 
    // whether they stick to their orders when things get tense
    [Range(-1f,1f)] public float Courage = 0.5f;

    // this defines how tightly they follow orders, how much they care
    // about regulations, whether they prioritize “the rules” over pity
    [Range(-1f, 1f)] public float Dutifulness = 0.5f;
}