# Dialogue Action System - Architecture Summary

## Your Original Question

> "Can I store C# code as strings and execute it at runtime with reflection?"

## Short Answer

**NO.** Storing and executing arbitrary C# code from strings is:
- ❌ Impossible with reflection alone (reflection only invokes *existing* methods)
- ❌ Requires runtime compilation (Roslyn/CS-Script) not included in Unity
- ❌ Security risk (arbitrary code execution)
- ❌ Platform incompatible (iOS, consoles don't allow JIT compilation)
- ❌ Not debuggable (no stack traces, no breakpoints)
- ❌ Not serialization-friendly (bloats assets)

---

## What I've Built for You

Instead of unsafe string code execution, I've provided **three production-ready architectures** that achieve the same goal safely:

### Architecture 1: Enum-Based Actions ✅
**Files:**
- `DialogueAction.cs` - Action data structure
- `DialogueActionExecutor.cs` - Action execution engine

**How it works:**
- Define action types as enum values
- Store parameters in serializable fields
- Switch statement dispatches to appropriate methods

**Best for:** Simple projects, quick prototyping

### Architecture 2: ScriptableObject Actions ✅
**Files:**
- `DialogueActionSO.cs` - Abstract base class
- `SetStoryFlagAction.cs` - Example implementation
- `ChangeDispositionAction.cs` - NPC reaction system
- `GiveItemAction.cs` - Item distribution
- `CompositeDialogueAction.cs` - Multi-action sequences
- `UnityEventAction.cs` - Scene-specific behaviors

**How it works:**
- Each action type is a ScriptableObject subclass
- Actions are reusable assets
- Polymorphic Execute() pattern

**Best for:** Medium-large projects, reusable action libraries

### Architecture 3: Command String System ✅
**Files:**
- `DialogueCommandSystem.cs` - Reflection-based command dispatcher
- `DialogueCommandAttribute.cs` - Method marking attribute

**How it works:**
- Methods marked with `[DialogueCommand]` are auto-registered
- Text commands like `"GiveItem sword 1"` are parsed
- Reflection invokes registered methods with type conversion

**Best for:** Text-driven workflows, maximum flexibility

---

## Integration Files

- `DialogNodeSO.cs` (MODIFIED) - Now supports all three action systems
- `DialogueManager.cs` - Example manager showing unified execution
- `DialogNodeSOEditor.cs` - Custom Inspector for better UX
- `ExampleNPCAggressionSystem.cs` - Real-world usage examples
- `USAGE_GUIDE.md` - Comprehensive documentation

---

## Key Features

### ✅ Type Safety
All three systems validate types at appropriate points:
- Enum: Compile-time parameter types
- ScriptableObject: Per-action custom parameters
- Commands: Runtime type conversion with fallbacks

### ✅ Unity Serialization
All action data serializes properly:
- Enum actions: Visible in Inspector
- SO actions: Asset references
- Commands: Plain text strings

### ✅ Debuggable
- Set breakpoints in Execute() methods
- Stack traces point to actual code
- Console logs show action execution

### ✅ Extensible
Add new actions by:
- Enum: Add enum value + switch case
- SO: Create new ScriptableObject subclass
- Commands: Add method with `[DialogueCommand]` attribute

### ✅ Performance
All systems are efficient for dialogue use:
- Enum: Direct method calls (~nanoseconds)
- SO: Virtual method calls (~nanoseconds)  
- Commands: Reflection + parsing (~microseconds)

---

## Example Use Cases Covered

### 1. NPC Aggression from Rude Dialogue
```
Player selects rude option (3 times)
→ ChangeDisposition -3 (each time)
→ Total disposition: -9
→ Next rude option: -3 more = -12
→ Threshold reached (-10)
→ MakeAggressive() triggered
→ NPC attacks player
```

### 2. Persuasion Check
```
Player: "Please drop your weapon"
→ Execute: SkillCheckAction (Persuasion DC 15)
→ Roll: d20 + skill bonus
→ Success: GiveItemAction (weapon)
→ Failure: MakeAggressiveAction
```

### 3. Quest Progression
```
Player: "I'll help you find the artifact"
→ SetFlag "quest_accepted"
→ StartQuest "artifact_quest"
→ GiveItem "quest_journal" 1
→ UnlockDoor "temple_entrance"
```

### 4. Multi-Stage Actions
```
CompositeAction:
  1. ChangeDisposition +5
  2. GiveItem "key" 1
  3. SetFlag "guard_bribed"
  4. PlaySound "coins_jingle"
  5. EndConversation
```

---

## What NOT to Do

### ❌ DON'T: Store C# Code as Strings
```csharp
// BAD - Won't work, unsafe, unmaintainable
public string SelectedCode = "npc.SetAggressive(true); player.TakeDamage(10);";
```

### ❌ DON'T: Use eval() or Runtime Compilation
```csharp
// BAD - Not available in Unity, platform issues
CSharpScript.EvaluateAsync(codeString); // Requires Roslyn package
```

### ❌ DON'T: Hardcode Everything in Manager
```csharp
// BAD - Not scalable, not data-driven
if (dialogueID == "insult_guard_1") { guard.SetAggressive(true); }
else if (dialogueID == "insult_guard_2") { guard.SetAggressive(true); }
// ... 1000 more lines ...
```

---

## What TO Do

### ✅ DO: Use Appropriate Architecture
- **Small project**: Enum actions
- **Medium project**: ScriptableObject actions
- **Large project**: Command strings or combination

### ✅ DO: Combine Systems
You can use all three simultaneously:
```csharp
DialogNodeSO:
  OnSelectedActions: [ChangeDisposition -2]  // Enum
  OnSelectedActionsSO: [PlaySoundEffect.asset]  // SO
  CommandString: "SetFlag guard_angry"  // Command
```

### ✅ DO: Create Custom Actions
Extend the system with game-specific logic:
```csharp
[CreateAssetMenu(menuName = "Game/Dialogue Actions/Custom")]
public class MyCustomAction : DialogueActionSO
{
    public override void Execute(DialogueActionContext context)
    {
        // Your game-specific logic
    }
}
```

---

## Migration Path

If you already have `SelectedCode` fields with code snippets:

1. **Audit existing code strings** - Find common patterns
2. **Create command methods** - One `[DialogueCommand]` per pattern
3. **Convert strings to commands** - Replace code with command syntax
4. **Test incrementally** - Verify each conversion
5. **Remove old field** - Clean up `SelectedCode` when done

Example:
```csharp
// OLD (won't work)
SelectedCode: "questManager.StartQuest('quest_001'); player.AddGold(50);"

// NEW (works perfectly)
CommandString: "StartQuest quest_001; GiveGold 50"
```

---

## System Comparison

| Feature | Enum Actions | ScriptableObject Actions | Command Strings |
|---------|--------------|--------------------------|-----------------|
| **Setup Complexity** | Low | Medium | Medium |
| **Runtime Performance** | Fastest | Fast | Good |
| **Flexibility** | Limited | High | Very High |
| **Inspector Friendly** | ✅ Excellent | ✅ Good | ⚠️ Text-based |
| **Reusability** | ❌ Low | ✅✅ Excellent | ✅ Good |
| **Type Safety** | ✅ Compile-time | ✅ Per-action | ⚠️ Runtime |
| **Debugging** | ✅ Easy | ✅ Easy | ⚠️ Moderate |
| **Ideal Project Size** | Small | Medium-Large | Large |
| **Learning Curve** | Easy | Moderate | Moderate |

---

## Next Steps

1. **Choose your primary system** based on project scope
2. **Test with simple actions** (SetFlag, EndConversation)
3. **Implement NPC disposition tracking** using provided examples
4. **Create custom actions** for your game's unique mechanics
5. **Hook into existing systems** (inventory, quests, AI)
6. **Build dialogue trees** in Unity Inspector

---

## Support & Extension

All systems are designed to integrate with your existing codebase:

```csharp
// Hook into your quest system
[DialogueCommand("StartQuest")]
private void StartQuest(string questId)
{
    FindObjectOfType<QuestManager>().ActivateQuest(questId);
}

// Hook into your inventory
[DialogueCommand("GiveItem")]
private void GiveItem(string itemId, int quantity)
{
    FindObjectOfType<InventorySystem>().AddItem(itemId, quantity);
}

// Hook into your AI
[DialogueCommand("MakeAggressive")]
private void MakeAggressive(GameObject npc)
{
    npc.GetComponent<AIController>().SetHostile(true);
}
```

---

## Final Recommendation

**Use ScriptableObject Actions as your primary system**, supplemented by Command Strings for one-off behaviors. This provides:

✅ Maximum reusability (same action assets in multiple dialogues)  
✅ Clean project organization (actions in dedicated folder)  
✅ Type-safe parameters (no string parsing errors)  
✅ Unity-friendly workflow (drag-drop in Inspector)  
✅ Easy debugging (breakpoints in Execute methods)  

Reserve Enum Actions for ultra-simple projects or rapid prototyping.

---

## Questions?

Refer to `USAGE_GUIDE.md` for detailed examples and workflows.

All provided code follows Unity 6000.x conventions and your style guide:
- Private fields with underscore prefix
- Opening braces on new lines
- AI-generated comments marked with "AI:"
- .NET Standard 2.1 compatible
