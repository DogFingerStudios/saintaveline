using UnityEngine;

[System.Serializable]
public class DialogLineDataRef
{
    //public DialogLineSO.DialogLineData InlineData = new();
    public string Text;
    public AudioClip AudioClip;
    public DialogLineDataSO Asset;

    public string GetText()
    {
        if (Asset != null)
        {
            return Asset.Text;
        }

        return Text;
    }

    public AudioClip GetAudio()
    {
        if (Asset != null)
        {
            return Asset.Audio;
        }

        return AudioClip;
    }
}