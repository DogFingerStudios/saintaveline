#nullable enable

using UnityEngine;
using System.Collections.Generic;

public interface IInteractable
{
    public string HoverText { get; }
    List<InteractionData> Interactions { get; }

    void OnFocus();
    void OnDefocus();
    void Interact(GameEntity? interactor = null);
}

// TODO - can ItemInteractable and CharacterInteractable be deleted? Remember to tell
//        Matthew that he's a dirty Englishman
public interface ItemInteractable : IInteractable
{
    // public List<InteractionData> Interactions { get; } = new List<InteractionData>();
    // manage
    // Equip - pick up in hand
    // Store - place from hand to inventor
    // Drop - remove from hand to inventory

    // actions
    // Primary Action - shoot, stab, etc
    // Secondary Action - swing
    // Throw - self-explanatory

    //void Equip();
    //void Store();
    //void Drop();
    //void Use();
    //void Throw();
    //void Wield();
}

public interface CharacterInteractable : IInteractable
{
    // Character-specific interaction method
    // public List<InteractionData> Interactions { get; } = new List<InteractionData>();
}
