#nullable enable

using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class is attached to root NPC objects
/// </summary>
public class InteractableEnemyNPC : EnemyNPC, IInteractable
{
    public string HoverText
    {
        get
        {
            if (!this.IsAlive) return $"{this.name} is dead";
            return $"Press [Q] to interact with the enemy named {this.name}";
        }
    }

    public List<InteractionData> Interactions { get; } = new List<InteractionData>();
    //[SerializeField, NPCStateDropdown]
    //private string _defaultState = "EnemyIdle";

    //public Transform[] PatrolPoints = new Transform[0];
    //public float ArrivalThreshold = 0.5f;

    //public float ViewAngle = 120f;
    //public Vector3 EyeOffset = new(0f, 1.6f, 0f);

    protected override void Start()
    {
        base.Start();
        Interactions.Add(new InteractionData { key = "converse", description = "Converse" });
    }

        // TODO: this is copied from ItemInteraction.cs, should be refactored to a common base class
    private void DoInteraction(string actionName)
    {
        Debug.Log($"Attempting to perform action '{actionName}' on {this.name}");
        Type type = this.GetType();
        while (type != null && type != typeof(MonoBehaviour))
        {
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                ItemAction attr = method.GetCustomAttribute<ItemAction>();
                if (attr != null && attr.ActionName == actionName)
                {
                    method.Invoke(this, null);
                    return;
                }
            }

            type = type.BaseType;
        }
        
        throw new Exception($"No action found for '{actionName}' in {this.GetType().Name}");
    }

    public override void Interact(GameEntity? interactor = null)
    {
        if (!this.IsAlive) return;
        InputManager.Instance.SetInputState(InputState.InteractionMenu);
        InteractionManager.Instance.OnInteractionAction += this.DoInteraction;
        InteractionManager.Instance.OpenMenu(Interactions);
    }

    [ItemAction("converse")]
    protected virtual void onConverse()
    {
        Debug.Log($"You converse with {this.name}. They don't have anything to say right now.");
    }
}