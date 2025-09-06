using UnityEngine;

public interface ItemInteractable
{
    string HelpText { get; }
    void OnFocus();
    void OnDefocus();

    void Interact(GameEntity? interactor = null);

    //void Equip();
    //void Store();
    //void Drop();
    //void Use();
    //void Throw();
    //void Wield();
}

// public interface NPCInteractable
// {
//     string HelpText { get; }
//     void OnFocus();
//     void OnDefocus();

//     void Interact(GameEntity? interactor = null);
// }
