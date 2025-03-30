using UnityEngine;

public class LampInteractable : Interactable
{
    public override void Interact()
    {
        Debug.Log("Lamp " + gameObject.name + " toggled!");
        // Trigger lamp animation or toggle state
    }

    public override void OnFocus()
    {
        Debug.Log("Looking at lamp " + gameObject.name);
    }
}
