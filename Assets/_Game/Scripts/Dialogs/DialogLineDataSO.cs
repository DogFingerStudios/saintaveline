using UnityEngine;

[CreateAssetMenu(fileName = "DialogLineData", menuName = "Game/Dialogs/Dialog Line Data")]
public class DialogLineDataSO : ScriptableObject
{
    [TextArea]
    public string Text;
    public AudioClip Audio;
}
