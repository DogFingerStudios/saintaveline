using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogLine", menuName = "Game/Dialogs/DialogLine")]
public class DialogLineSO : ScriptableObject
{
    public GameObject speaker;
    public string Title;

    // these are the possible lines that can be spoken at the current point in the conversation, the 
    // idea here is to randomly pick one of multiple options that more or less say the same thing, this
    // will add some variety to the conversations without needing to create a ton of different dialog lines
    public List<DialogLineDataRef> Line = new();

    // if true, the conversation will automatically advance to the next line after this one is finished,
    // if false, the player will need to select an option to advance the conversation
    public bool AutoAdvance = false;

    // these are the options the player can select from when responding to the Line
    public List<DialogLineSO> Options = new();

    [TextArea]
    public string Note;
}

