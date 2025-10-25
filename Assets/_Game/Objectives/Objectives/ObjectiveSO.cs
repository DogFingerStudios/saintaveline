using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewObjective", menuName = "Game/Objectives")]
public class  ObjectiveSO : ScriptableObject
{
    public string Name;

    [TextArea]
    public string Description;
}
