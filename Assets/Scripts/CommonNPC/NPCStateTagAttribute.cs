#nullable enable

// The classes in this file allow new states to be tagged with a string and have that state
// created via a string value in the NPC (think of a `_defaultState` string in the NPC class).
// This method allows new classes to be created just by tagging them. There is also code in 
// this file to allow Unity to create a dropdown list of NPC states in the inspector, and

using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;


// let's Unity know that this attribute is used for dropdowns in the inspector
public class NPCStateDropdownAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(NPCStateDropdownAttribute))]
public class NPCStateDropdownDrawer : PropertyDrawer
{
    private static string[] _tags = null!;

    private void EnsureTagsLoaded()
    {
        if (_tags != null)
        {
            return;
        }

        _tags = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(NPCState).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => t.GetCustomAttribute<NPCStateTagAttribute>())
            .Where(attr => attr != null)
            .Select(attr => attr!.Tag)
            .Distinct()
            .OrderBy(t => t)
            .ToArray();
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EnsureTagsLoaded();

        if (_tags == null || _tags.Length == 0)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        int selectedIndex = Mathf.Max(0, Array.IndexOf(_tags, property.stringValue));
        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, _tags);

        property.stringValue = _tags[selectedIndex];
    }
}
#else
// AI: Dummy attribute so builds outside the editor don't fail
public class NPCStateDropdownAttribute : PropertyAttribute
{
    // AI: No code needed here — just prevents missing symbol errors
}
#endif

[AttributeUsage(AttributeTargets.Class)]
public class NPCStateTagAttribute : Attribute
{
    public string Tag { get; }
    public NPCStateTagAttribute(string tag)
    {
        Tag = tag;
    }
}

public static class NPCStateFactory
{
    private static readonly Dictionary<string, Type> _taggedTypes;

    static NPCStateFactory()
    {
        _taggedTypes = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(NPCState).IsAssignableFrom(t) && !t.IsAbstract)
            .Select(t => new
            {
                Type = t,
                Tag = t.GetCustomAttribute<NPCStateTagAttribute>()?.Tag
            })
            .Where(x => !string.IsNullOrEmpty(x.Tag))
            .ToDictionary(x => x.Tag!, x => x.Type);
    }

    public static NPCState? CreateState(string tag, params object[] args)
    {
        if (!_taggedTypes.TryGetValue(tag, out var stateType))
        {
            UnityEngine.Debug.LogError($"No NPCState found for tag '{tag}'");
            return null;
        }

        try
        {
            return (NPCState?)Activator.CreateInstance(stateType, args);
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError($"Failed to create NPCState '{tag}': {ex}");
            return null;
        }
    }
}
