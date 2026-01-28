using System.Collections.Generic;
using UnityEngine;

public class PartnershipManager : MonoBehaviour
{
    private Dictionary<int, Partnership> _partnerships = new();
    public Dictionary<int, Partnership> Partnerships => _partnerships;

    public static PartnershipManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            throw new System.Exception("PartnershipManager: Multiple instances detected.");
        }

        Instance = this;
    }
}
