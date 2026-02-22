using UnityEngine;
using System;
using System.Collections.Generic;

public class GroupManager : MonoBehaviour
{
    public Dictionary<BaseNPC, Group> GroupMap { get; private set; } = new();

    private static readonly Lazy<GroupManager> _instance =
        new(() => new GroupManager());

    public static GroupManager Instance => _instance.Value;

    public void RegisterGroup(BaseNPC npc, List<BaseNPC> members)
    {
        if (GroupMap.ContainsKey(npc)) return;

        Group group = new();
        group.Members.Add(npc);
        group.Members.AddRange(members);

        foreach (var member in group.Members)
        {
            GroupMap[member] = group;
            member.Group = group;
        }
    }
}
