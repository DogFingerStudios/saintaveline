using UnityEngine;

public class LampInteractable : Interactable
{
    public override void Interact()
    {
        Debug.Log("Lamp " + gameObject.name + " toggled!");
        // get child component named "Bulb"
        Light bulb = GetComponentInChildren<Light>();
        if (bulb == null) return;
        bulb.intensity = bulb.intensity == 0 ? 1 : 0; 
    }

    public override void OnFocus()
    {
        Debug.Log("Looking at lamp " + gameObject.name);
    }
}
