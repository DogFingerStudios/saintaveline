using System.Collections.Generic;
using UnityEngine;

public class ZoneManager : MonoBehaviour
{
    private readonly HashSet<Zone> _activeZones = new HashSet<Zone>();
    private Zone _currentZone;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent(out Zone zone))
        {
            return;
        }

        if (_activeZones.Add(zone))
        {
            EvaluateZones();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent(out Zone zone))
        {
            return;
        }

        if (_activeZones.Remove(zone))
        {
            EvaluateZones();
        }
    }

    private void EvaluateZones()
    {
        Zone bestZone = null;

        foreach (var zone in _activeZones)
        {
            if (bestZone == null || zone.Priority > bestZone.Priority)
            {
                bestZone = zone;
            }
        }

        if (bestZone == _currentZone)
        {
            return;
        }

        _currentZone = bestZone;

        if (_currentZone != null)
        {
            BottomTypewriter.Instance.Enqueue(_currentZone.Data.Name);
        }
    }
}
