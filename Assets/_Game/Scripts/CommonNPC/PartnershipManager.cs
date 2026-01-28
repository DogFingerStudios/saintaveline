using System.Collections.Generic;
using UnityEngine;


public class PartnershipManager : MonoBehaviour
{
    public Dictionary<BaseNPC, Partnership> PartnershipMap { get; private set; } = new();

    public static PartnershipManager Instance { get; private set; }

    PartnershipManager()
    {
        if (Instance != null)
        {
            throw new System.Exception("PartnershipManager: Multiple instances detected.");
        }

        Instance = this;
    }

    public void RegisterPartnership(BaseNPC npc, List<BaseNPC> partners)
    {
        if (PartnershipMap.ContainsKey(npc)) return;

        Partnership partnership = new();
        partnership.Partners.Add(npc);
        partnership.Partners.AddRange(partners);

        foreach (var partner in partnership.Partners)
        {
            PartnershipMap[partner] = partnership;
            partner.Partnership = partnership;
        }
    }
}
