using UnityEngine;

public interface IHearingSensor
{
    Vector3 Position { get; }
    void HandleSound(SoundStimulus stim);
}
