using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;

// AI: Command pattern implementation - maps string commands to methods via reflection
public class DialogueCommandSystem : MonoBehaviour
{
    private Dictionary<string, MethodInfo> _commandRegistry = new Dictionary<string, MethodInfo>();
    private Dictionary<string, object> _commandHandlers = new Dictionary<string, object>();

    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _questSystem;

    private void Awake()
    {
        RegisterCommandHandlers();
    }

    // AI: Register all classes that contain dialogue commands
    private void RegisterCommandHandlers()
    {
        // AI: Register this class as a handler
        RegisterHandler(this);

        // AI: You can register other systems here
        // RegisterHandler(FindObjectOfType<QuestManager>());
        // RegisterHandler(FindObjectOfType<InventorySystem>());
    }

    // AI: Finds all methods marked with [DialogueCommand] attribute
    public void RegisterHandler(object handler)
    {
        if (handler == null)
        {
            return;
        }

        Type handlerType = handler.GetType();
        MethodInfo[] methods = handlerType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        foreach (var method in methods)
        {
            var attribute = method.GetCustomAttribute<DialogueCommandAttribute>();
            if (attribute != null)
            {
                string commandName = string.IsNullOrEmpty(attribute.CommandName) 
                    ? method.Name 
                    : attribute.CommandName;

                _commandRegistry[commandName.ToLower()] = method;
                _commandHandlers[commandName.ToLower()] = handler;

                Debug.Log($"Registered dialogue command: {commandName}");
            }
        }
    }

    // AI: Execute a command with parameters
    public bool ExecuteCommand(string commandString, GameObject npc)
    {
        if (string.IsNullOrWhiteSpace(commandString))
        {
            return false;
        }

        // AI: Parse command format: "CommandName param1 param2 param3"
        string[] parts = commandString.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return false;
        }

        string commandName = parts[0].ToLower();

        if (!_commandRegistry.ContainsKey(commandName))
        {
            Debug.LogWarning($"Dialogue command '{commandName}' not found!");
            return false;
        }

        MethodInfo method = _commandRegistry[commandName];
        object handler = _commandHandlers[commandName];

        try
        {
            ParameterInfo[] parameters = method.GetParameters();
            object[] args = new object[parameters.Length];

            // AI: Build arguments array with type conversion
            for (int i = 0; i < parameters.Length; i++)
            {
                if (i == 0 && parameters[i].ParameterType == typeof(GameObject))
                {
                    args[i] = npc; // AI: First param is always NPC if GameObject
                }
                else
                {
                    int paramIndex = i + 1 - (parameters[0].ParameterType == typeof(GameObject) ? 1 : 0);
                    if (paramIndex < parts.Length - 1)
                    {
                        args[i] = ConvertParameter(parts[paramIndex + 1], parameters[i].ParameterType);
                    }
                    else if (parameters[i].HasDefaultValue)
                    {
                        args[i] = parameters[i].DefaultValue;
                    }
                    else
                    {
                        args[i] = GetDefaultValue(parameters[i].ParameterType);
                    }
                }
            }

            method.Invoke(handler, args);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error executing dialogue command '{commandName}': {ex.Message}");
            return false;
        }
    }

    // AI: Convert string parameter to target type
    private object ConvertParameter(string value, Type targetType)
    {
        try
        {
            if (targetType == typeof(string))
            {
                return value;
            }
            else if (targetType == typeof(int))
            {
                return int.Parse(value);
            }
            else if (targetType == typeof(float))
            {
                return float.Parse(value);
            }
            else if (targetType == typeof(bool))
            {
                return bool.Parse(value);
            }
            else
            {
                return Convert.ChangeType(value, targetType);
            }
        }
        catch
        {
            return GetDefaultValue(targetType);
        }
    }

    private object GetDefaultValue(Type type)
    {
        return type.IsValueType ? Activator.CreateInstance(type) : null;
    }

    // ==================== EXAMPLE COMMANDS ====================

    [DialogueCommand("SetFlag")]
    private void SetStoryFlag(string flagName, bool value = true)
    {
        PlayerPrefs.SetInt(flagName, value ? 1 : 0);
        Debug.Log($"Story flag '{flagName}' set to {value}");
    }

    [DialogueCommand("StartQuest")]
    private void StartQuest(string questId)
    {
        Debug.Log($"Starting quest: {questId}");
        // AI: Hook into your quest system
    }

    [DialogueCommand("MakeAggressive")]
    private void MakeNPCAggressive(GameObject npc)
    {
        Debug.Log($"NPC {npc.name} is now aggressive!");
        var controller = npc.GetComponent<NPCController>();
        if (controller != null)
        {
            controller.SetAggressive(true);
        }
    }

    [DialogueCommand("ChangeDisposition")]
    private void ChangeDisposition(GameObject npc, int delta)
    {
        Debug.Log($"Changing {npc.name} disposition by {delta}");
        // AI: Implement disposition tracking
    }

    [DialogueCommand("GiveItem")]
    private void GiveItem(string itemId, int quantity = 1)
    {
        Debug.Log($"Giving player {quantity}x {itemId}");
        // AI: Add to inventory
    }

    [DialogueCommand("DropItem")]
    private void DropItem(GameObject npc, string itemId)
    {
        Debug.Log($"NPC {npc.name} drops {itemId}");
        // AI: Spawn item at NPC location
    }

    [DialogueCommand("UnlockDoor")]
    private void UnlockDoor(string doorId)
    {
        Debug.Log($"Unlocking door: {doorId}");
        GameObject door = GameObject.Find(doorId);
        if (door != null)
        {
            // AI: Unlock door logic
        }
    }

    [DialogueCommand("SpawnEnemies")]
    private void SpawnEnemies(string enemyType, int count)
    {
        Debug.Log($"Spawning {count} enemies of type {enemyType}");
        // AI: Use spawning system
    }

    [DialogueCommand("EndConversation")]
    private void EndConversation(GameObject npc)
    {
        Debug.Log("Ending conversation");
        // AI: Close dialogue UI
    }

    [DialogueCommand("PlaySound")]
    private void PlaySound(string soundName)
    {
        Debug.Log($"Playing sound: {soundName}");
        // AI: Trigger audio
    }
}

// AI: Attribute to mark methods as dialogue commands
[AttributeUsage(AttributeTargets.Method)]
public class DialogueCommandAttribute : Attribute
{
    public string CommandName { get; private set; }

    public DialogueCommandAttribute(string commandName = null)
    {
        CommandName = commandName;
    }
}
