using UnityEngine;
using System.Collections.Generic;



[CreateAssetMenu(fileName = "DialogLine", menuName = "Game/DialogLine")]
public class DialogLineSO : ScriptableObject
{
    [System.Serializable]
    public class DialogLineData
    {
        public string Text;
        public AudioClip Audio;
    }

    public GameObject speaker;
    public string Title;

    // these are the possible lines that can be spoken at the current point in the conversation, the 
    // idea here is to randomly pick one of multiple options that more or less say the same thing, this
    // will add some variety to the conversations without needing to create a ton of different dialog lines
    public List<DialogLineData> Line = new();

    // these are the options the player can select from when responding to the Line
    public List<DialogLineSO> Options = new();
}

