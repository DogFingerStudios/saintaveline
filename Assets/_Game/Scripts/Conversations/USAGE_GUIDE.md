# Dialogue Action System - Usage Guide

## Overview

This system provides **THREE** different approaches for executing gameplay actions from dialogue choices:

1. **Enum-based Actions** (Simple) - Best for beginners
2. **ScriptableObject Actions** (Modular) - Best for reusable action libraries
3. **Command String System** (Flexible) - Best for power users and rapid iteration

---

## Solution 1: Enum-Based Actions

### Setup
1. Add `DialogueActionExecutor` component to a scene GameObject
2. In `DialogNodeSO` Inspector, add actions to the `OnSelectedActions` list
3. Configure each action's type and parameters

### Example: Making NPC Aggressive After Rude Responses

```
DialogNodeSO: "PlayerResponse_Insult"
├── OnSelectedActions (List)
    ├── Action 0
    │   ├── ActionType: ChangeNPCDisposition
    │   ├── IntParam: -3  (disposition penalty)
    ├── Action 1
        ├── ActionType: SetStoryFlag
        ├── StringParam: "InsultedShopkeeper"
```

### Pros
- Fully visible in Unity Inspector
- Type-safe parameter validation
- No external asset dependencies
- Easy to understand for non-programmers

### Cons
- Limited to predefined actions
- Adding new action types requires code changes
- Parameters are generic (StringParam, IntParam)

---

## Solution 2: ScriptableObject Actions

### Setup
1. Create action assets: Right-click → Create → Game → Dialogue Actions
2. Configure action parameters in the asset Inspector
3. Reference actions in `DialogNodeSO.OnSelectedActionsSO` list

### Example: NPC Drops Item When Persuaded

**Step 1:** Create Actions
```
Assets/DialogueActions/
├── GiveGoldAction.asset
│   └── ItemId: "Gold"
│   └── Quantity: 50
│   └── DropAtNPCLocation: true
├── SetPersuadedFlag.asset
    └── FlagName: "ShopkeeperPersuaded"
    └── FlagValue: true
```

**Step 2:** Reference in Dialogue Node
```
DialogNodeSO: "PlayerResponse_Persuade"
├── OnSelectedActionsSO (List)
    ├── SetPersuadedFlag
    ├── GiveGoldAction
```

### Creating Custom Actions

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "MyCustomAction", menuName = "Game/Dialogue Actions/My Custom Action")]
public class MyCustomAction : DialogueActionSO
{
    [SerializeField] private int _myParameter;

    public override void Execute(DialogueActionContext context)
    {
        // AI: Your custom logic here
        Debug.Log($"Custom action executed with param: {_myParameter}");

        // AI: Access context
        GameObject npc = context.NPC;
        GameObject player = context.Player;
    }

    public override bool CanExecute(DialogueActionContext context)
    {
        // AI: Optional validation
        return context.NPC != null;
    }
}
```

### Pros
- Highly reusable (same action asset used in multiple dialogues)
- Clean separation of data and logic
- Supports complex nested actions (CompositeDialogueAction)
- Type-safe with custom parameters per action type
- Can use UnityEvents for scene-specific behaviors

### Cons
- Requires creating separate asset files
- More setup overhead initially
- Requires understanding of ScriptableObject workflows

---

## Solution 3: Command String System

### Setup
1. Add `DialogueCommandSystem` component to a scene GameObject
2. Write commands as text in `DialogNodeSO.CommandString` field
3. Commands are automatically discovered via `[DialogueCommand]` attribute

### Example: Multiple Actions in One String

```
DialogNodeSO: "PlayerResponse_Threaten"
CommandString: 
"ChangeDisposition -5; SetFlag NPCThreatened; PlaySound Angry_Grunt"
```

### Command Format
```
CommandName param1 param2 param3
```

Multiple commands separated by semicolons:
```
StartQuest quest_id; GiveItem sword 1; SetFlag quest_started
```

### Creating Custom Commands

```csharp
using UnityEngine;

public class MyGameSystem : MonoBehaviour
{
    private void Awake()
    {
        // AI: Register this class with the command system
        FindObjectOfType<DialogueCommandSystem>().RegisterHandler(this);
    }

    [DialogueCommand("TeleportPlayer")]
    private void TeleportPlayer(string locationName)
    {
        // AI: Your implementation
        Debug.Log($"Teleporting player to {locationName}");
    }

    [DialogueCommand("SpawnBoss")]
    private void SpawnBoss(GameObject npc, string bossType, int level)
    {
        // AI: First param is automatically the NPC if GameObject type
        Debug.Log($"Spawning {bossType} level {level} near {npc.name}");
    }
}
```

### Built-in Commands

| Command | Parameters | Example |
|---------|------------|---------|
| `SetFlag` | flagName, value | `SetFlag quest_complete true` |
| `StartQuest` | questId | `StartQuest main_quest_01` |
| `MakeAggressive` | (uses NPC) | `MakeAggressive` |
| `ChangeDisposition` | delta | `ChangeDisposition -2` |
| `GiveItem` | itemId, quantity | `GiveItem sword 1` |
| `DropItem` | itemId | `DropItem health_potion` |
| `UnlockDoor` | doorId | `UnlockDoor castle_gate` |
| `SpawnEnemies` | enemyType, count | `SpawnEnemies goblin 3` |
| `EndConversation` | - | `EndConversation` |
| `PlaySound` | soundName | `PlaySound door_open` |

### Pros
- Fastest iteration (no asset creation)
- Most flexible (easy to add new commands)
- Can be data-driven (load from JSON/CSV)
- Supports parameters of any type
- Easy for writers/designers to use

### Cons
- No compile-time validation (typos cause runtime errors)
- No Inspector autocomplete
- Requires understanding of command syntax
- Debugging is harder (errors only at runtime)

---

## Combining All Three

You can use all three systems together! They execute in order:
1. Enum Actions (`OnSelectedActions`)
2. ScriptableObject Actions (`OnSelectedActionsSO`)
3. Command Strings (`CommandString`)

### Example: Complex Dialogue Option

```
DialogNodeSO: "PlayerResponse_ComplexPersuasion"

OnSelectedActions:
├── ChangeNPCDisposition: +5

OnSelectedActionsSO:
├── GiveGoldAction (50 gold)
├── UnlockQuestAction

CommandString:
"SetFlag merchant_persuaded; PlaySound coin_drop; StartQuest merchant_quest"
```

---

## Best Practices

### Use Enum Actions When:
- You're prototyping quickly
- Your team is new to Unity
- You need everything visible in one Inspector

### Use ScriptableObject Actions When:
- You reuse actions across many dialogues
- You want clean project organization
- You need complex multi-step behaviors
- You want UnityEvent integration for scene-specific triggers

### Use Command Strings When:
- You're implementing many unique one-off actions
- You want data-driven dialogue (external files)
- You need maximum flexibility
- Your designers are comfortable with text-based workflows

### Disposition-Based Aggression Example

Track NPC reactions across multiple dialogue choices:

```
Dialogue 1 (Rude): ChangeDisposition -2
Dialogue 2 (Rude): ChangeDisposition -3
Dialogue 3 (Insult): ChangeDisposition -6  → NPC becomes aggressive!
```

The `DialogueActionExecutor` automatically triggers aggression when disposition ≤ -10.

---

## Debugging Tips

1. **Enable Debug Logs**: All systems log to Console when actions execute
2. **Check NPC References**: Ensure DialogueManager has correct _currentNPC
3. **Verify Command Registration**: Check Console on play for "Registered dialogue command: X"
4. **Test CanExecute**: ScriptableObject actions can fail validation silently
5. **Use Breakpoints**: Set breakpoints in action Execute() methods

---

## Performance Notes

- **Enum Actions**: Fastest (no reflection, no asset lookups)
- **ScriptableObject Actions**: Fast (cached references)
- **Command Strings**: Slowest (reflection + string parsing), but still negligible for dialogue

All three are suitable for dialogue use cases (dozens of actions per frame is fine).

---

## Extending the System

### Add Global Story Flag System

Replace `PlayerPrefs` in examples with a proper persistent data system:

```csharp
public class StoryFlagManager : MonoBehaviour
{
    private Dictionary<string, bool> _flags = new();

    public void SetFlag(string name, bool value)
    {
        _flags[name] = value;
        // AI: Save to disk
    }

    public bool GetFlag(string name)
    {
        return _flags.ContainsKey(name) && _flags[name];
    }
}
```

### Add Conditional Dialogue Options

Check requirements before showing options:

```csharp
public class ConditionalDialogueOption
{
    [SerializeField] private DialogNodeSO _option;
    [SerializeField] private List<string> _requiredFlags;

    public bool IsAvailable(StoryFlagManager flagManager)
    {
        foreach (var flag in _requiredFlags)
        {
            if (!flagManager.GetFlag(flag))
            {
                return false;
            }
        }
        return true;
    }
}
```

---

## Migration from String Code

If you already have `SelectedCode` fields with C# code snippets, you can:

1. **Identify patterns**: Look for common actions in your code strings
2. **Create commands**: Add `[DialogueCommand]` methods for each pattern
3. **Replace incrementally**: Convert code strings to command strings
4. **Test thoroughly**: Verify behavior matches original code

Example migration:
```csharp
// OLD (unsafe, won't work)
SelectedCode: "npc.GetComponent<AIController>().SetAggressive(true);"

// NEW (command string)
CommandString: "MakeAggressive"
```

---

## Summary

**Don't store C# code as strings!** Instead:

✅ **Use Enum Actions** for simple, inspector-friendly behaviors  
✅ **Use ScriptableObject Actions** for reusable, modular actions  
✅ **Use Command Strings** for flexible, text-based workflows  

All three approaches are:
- Type-safe (or safely parsed)
- Debuggable
- Performant
- Unity-serialization-friendly
- Platform-independent

Choose based on your team's needs and project scale.
