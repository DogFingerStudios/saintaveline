#nullable enable
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewZone", menuName = "Game/Zone")]
public class ZoneData : ScriptableObject
{
    public string Name = null!;
    [TextArea] public string Description = null!;
}
