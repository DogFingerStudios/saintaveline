using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual string helpText
    {
        get { return "Press [E] to interact"; }
    }

    public virtual void OnFocus()
    {
        // Optional: highlight outline, play sound, etc.
    }

    public virtual void OnDefocus()
    {
        // Cleanup when not hovered
    }

    public abstract void Interact();
}
