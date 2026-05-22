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

