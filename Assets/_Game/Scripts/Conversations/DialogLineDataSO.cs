using UnityEngine;

[CreateAssetMenu(fileName = "DialogLineData", menuName = "Game/Dialogs/Dialog Line Data")]
public class DialogLineDataSO : ScriptableObject
{
    [SerializeField] 
    public DialogLineData Data;
}
