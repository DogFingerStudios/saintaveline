using Unity.VisualScripting;
using UnityEngine;

public abstract class FriendlyNPC : BaseNPC, Interactable
{
    public CommandMenu commandMenu;

#region Interactable Interface Implementation
    public string HelpText => "Press [E] to interact";

    public void OnFocus()
    { 
        // Optional: highlight outline, play sound, etc.
    }

    public void OnDefocus()
    {
        // Cleanup when not hovered
    }

    public void Interact()
    {
        commandMenu.Open(this);
    }
#endregion

}
