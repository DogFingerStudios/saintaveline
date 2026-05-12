using UnityEngine;

// AI: Sets a boolean story flag - useful for quest progression and branching
[CreateAssetMenu(fileName = "SetFlagAction", menuName = "Game/Dialogue Actions/Set Story Flag")]
public class SetStoryFlagAction : DialogueActionSO
{
    [SerializeField] private string _flagName;
    [SerializeField] private bool _flagValue = true;

    public override void Execute(DialogueActionContext context)
    {
        if (string.IsNullOrEmpty(_flagName))
        {
            Debug.LogWarning("SetStoryFlagAction: Flag name is empty!");
            return;
        }

        // AI: You would hook this into your game's persistent data system
        PlayerPrefs.SetInt(_flagName, _flagValue ? 1 : 0);
        Debug.Log($"Story flag '{_flagName}' set to {_flagValue}");
    }
}
