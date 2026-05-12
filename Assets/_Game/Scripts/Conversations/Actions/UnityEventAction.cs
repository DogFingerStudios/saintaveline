using UnityEngine;
using UnityEngine.Events;

// AI: Invokes UnityEvents - bridges dialogue system to scene-specific behaviors
[CreateAssetMenu(fileName = "UnityEventAction", menuName = "Game/Dialogue Actions/Unity Event")]
public class UnityEventAction : DialogueActionSO
{
    [SerializeField] private UnityEvent _onExecute;

    public override void Execute(DialogueActionContext context)
    {
        _onExecute?.Invoke();
    }
}
