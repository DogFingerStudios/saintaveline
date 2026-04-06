using UnityEngine;

[CreateAssetMenu(fileName = "DialogLineData", menuName = "Game/Dialogs/Dialog Line Data")]
public class PhrasingSO : ScriptableObject
{
    [SerializeField] 
    public Phrasing Data;
}
