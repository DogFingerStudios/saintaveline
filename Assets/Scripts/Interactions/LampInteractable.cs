using UnityEngine;

public class LampInteractable : Interactable
{
    // public AudioSource audioSource;
    public AudioClip lampSwitchSound;
    public override void Interact()
    {
        // Debug.Log("Lamp " + gameObject.name + " toggled!");

        Light bulb = GetComponentInChildren<Light>();
        if (bulb == null) return;
        bulb.intensity = bulb.intensity == 0 ? 1 : 0;

        AudioSource audioSource = GetComponent<AudioSource>();
        if (lampSwitchSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(lampSwitchSound);
        }
    }

    public override void OnFocus()
    {
        // Debug.Log("Looking at lamp " + gameObject.name);
    }
}
