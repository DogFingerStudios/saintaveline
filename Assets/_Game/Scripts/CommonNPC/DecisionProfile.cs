using UnityEngine;

public enum DecisionResult
{
    Obey,
    Refuse,
    Hesitate
}

public static class DecisionProfile
{
    /// <summary>
    /// Evaluates a decision threshold based on order context and entity/relationship traits.
    /// </summary>
    /// <param name="safety">How dangerous the command is (-1 to 1)</param>
    /// <param name="utility">How useful the command is (-1 to 1)</param>
    /// <param name="traits">Entity-level traits (e.g., courage, trusting)</param>
    /// <param name="relationship">Relationship-specific traits toward the order-giver</param>
    public static DecisionResult Evaluate(
        float safety,
        float utility,
        PersonalityProfile traits,
        RelationshipTraits relationship)
    {
        // Adjust Safety: Less courage = more penalty from danger
        float adjustedSafety = Mathf.Lerp(safety, 0f, 1f - traits.Courage);

        // Calculate effective trust
        float totalTrust = traits.Trusting + relationship.TrustToward;

        // Emotional influence from love and fear
        float emotionalBias = relationship.Love - relationship.FearOf;

        // Weighted sum of all factors
        float sum =
            adjustedSafety +   // danger consideration
            utility +          // usefulness of the order
            totalTrust +       // global + relational trust
            (traits.Loving * relationship.Love) -   // overall warmth magnifies love's impact
            relationship.FearOf * 0.5f;             // fear reduces obedience

        // Normalize and shift into [0, 1] range
        float normalized = sum / 5f; // Denominator = number of blended terms
        float threshold = (normalized + 1f) / 2f;

        // Interpret threshold
        if (threshold > 0.6f)
        {
            return DecisionResult.Obey;
        }
        else if (threshold < 0.3f)
        {
            return DecisionResult.Refuse;
        }
        else
        {
            return DecisionResult.Hesitate;
        }
    }
}
