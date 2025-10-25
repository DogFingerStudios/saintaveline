using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public enum StimulusKind
{
    Gunshot,
    Explosion,
    Shout
}


public struct SoundStimulus
{
    public Vector3 Position;
    public StimulusKind Kind;
    public float HearingRange;

    // // AI: Base loudness in arbitrary units; map your weapons to this
    // public float BaseLoudness;
    // // AI: Faction or source id; optional
    // public int SourceTeam;
    // // AI: Frame count or time created
    // public float TimeCreated;

    // public SoundStimulus(Vector3 position, StimulusKind kind, float baseLoudness, int sourceTeam)
    // {
    //     Position = position;
    //     Kind = kind;
    //     BaseLoudness = baseLoudness;
    //     SourceTeam = sourceTeam;
    //     TimeCreated = Time.time;
    // }
}

public static class StimulusBus
{
    // AI: HearingSensors subscribe here; emitters publish here
    public static event Action<SoundStimulus> OnSoundEmitted;

    // AI: Keep weak registration of sensors if you later add spatial partitioning
    private static readonly List<IHearingSensor> _sensors = new List<IHearingSensor>();

    public static void Register(IHearingSensor sensor)
    {
        if (!_sensors.Contains(sensor))
        {
            _sensors.Add(sensor);
        }
    }

    public static void Unregister(IHearingSensor sensor)
    {
        if (_sensors.Contains(sensor))
        {
            _sensors.Remove(sensor);
        }
    }

    public static void Emit(SoundStimulus stim)
    {
        OnSoundEmitted?.Invoke(stim); 
    }

    public static void Emit2(SoundStimulus stim)
    {
        var position = stim.Position;
        var nearbySensors = _sensors.Where(sensor =>
            {
                var distance = Vector3.Distance(sensor.Position, position);
                return distance <= stim.HearingRange;
            });

        foreach (var sensor in nearbySensors)
        {
            sensor.HandleSound(stim);
        }
    }

    public static IReadOnlyList<IHearingSensor> Sensors => _sensors;
}
