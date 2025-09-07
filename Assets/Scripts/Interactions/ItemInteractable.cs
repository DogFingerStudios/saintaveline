using UnityEngine;

public interface IInteractable
{
    string HoverText { get; }
    void OnFocus();
    void OnDefocus();

    void Interact(GameEntity? interactor = null);
}

public interface ItemInteractable : IInteractable
{
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
    // Character-specific interaction methods
}
