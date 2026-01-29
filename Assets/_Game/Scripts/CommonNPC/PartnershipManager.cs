using UnityEngine;
using System;
using System.Collections.Generic;

public class PartnershipManager : MonoBehaviour
{
    public Dictionary<BaseNPC, Partnership> PartnershipMap { get; private set; } = new();

    private static readonly Lazy<PartnershipManager> _instance =
        new(() => new PartnershipManager());

    public static PartnershipManager Instance => _instance.Value;

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
