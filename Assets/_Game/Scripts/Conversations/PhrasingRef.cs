using UnityEngine;

[System.Serializable]
public class PhrasingRef
{
    public Phrasing InlineData = new();
    public PhrasingSO Asset;

    public string GetText()
    {
        if (Asset != null)
        {
            return Asset.Data.Text;
        }

        return InlineData.Text;
    }

    public AudioClip GetAudio()
    {
        if (Asset != null)
        {
            return Asset.Data.Audio;
        }

        return InlineData.Audio;
    }
}