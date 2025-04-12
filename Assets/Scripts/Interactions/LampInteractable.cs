using UnityEngine;

public class LampInteractable : MonoBehaviour, Interactable
{
    public string HelpText
    {
        get { return "Press [E] to toggle lamp"; }
    }

    // public AudioSource audioSource;
    public AudioClip lampSwitchSound;
    public void Interact()
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

    public void OnFocus()
    {
        // Debug.Log("Looking at lamp " + gameObject.name);
    }

    public void OnDefocus()
    {
        // Debug.Log("Defocused from lamp " + gameObject.name);
    }
}
