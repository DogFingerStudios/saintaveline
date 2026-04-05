#nullable enable

using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField] public ConversationSO Conversation = null!;
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

    public override void Interact(GameEntity? interactor = null)
    {
        if (!this.IsAlive) return;
        InputManager.Instance.SetInputState(InputState.InteractionMenu);
        InteractionManager.Instance.OnInteractionAction += this.DoInteraction;
        InteractionManager.Instance.OpenMenu(Interactions);
    }

    [ItemAction("converse", ItemAction.Flags.SkipStateChange | ItemAction.Flags.SkipCrossHairReset)]
    protected virtual void OnConverse()
    {
        if (this.Conversation == null) return;
        ConversationManager.Instance.StartConversation(this.Conversation);
    }
}