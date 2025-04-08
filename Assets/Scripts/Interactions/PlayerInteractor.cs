using NUnit.Framework.Internal.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 3f; // The maximum distance at which the player can interact with objects
    public Image crosshairImage; // The UI element representing the crosshair
    public Color defaultColor = Color.white;  // The default color of the crosshair
    public Color highlightColor = Color.green; // The color of the crosshair when hovering over an interactable object
    public TextMeshProUGUI helpTextUI; // The UI element that displays help text

    private Interactable _currentFocus; // The currently focused interactable object
    private void Start() // Initialize the player interactor
    {
        helpTextUI.gameObject.SetActive(false); // Hide the help text UI at the start
    }
    private void checkInteractions() // Check for interactions with objects
    {
        RaycastHit hit; // Structure to store raycast hit information
        Ray ray = new Ray(transform.position, transform.forward); // Create a ray from the player's position in the forward direction
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green); // Visualize the ray in the editor

        if (Physics.Raycast(ray, out hit, interactRange, ~0)) // Checks if the ray hits an object on all layers within the interact range
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>(); // Get the Interactable component from the hit object
            if (interactable != null) // Check if the hit object has an Interactable component
            {
                if (interactable != _currentFocus) // Checks if the interactable is a new target
                {
                    ClearFocus(); // Clear the previous focus
                    _currentFocus = interactable; // Set the new focus to the interactable object
                    _currentFocus.OnFocus(); // Call the OnFocus method of the interactable object
                    crosshairImage.color = highlightColor; // Change the crosshair color to highlight color

                    helpTextUI.text = interactable.helpText; // Set the help text to the interactable's help text
                    helpTextUI.gameObject.SetActive(true); // Show the help text UI
                }

                if (Input.GetKeyDown(KeyCode.E)) // Check if the player presses the interact key (E)
                {
                    _currentFocus.Interact(); // Call the Interact method of the interactable object
                }
            }
            else
            {
                ClearFocus();
            }
        }
        else
        {
            ClearFocus(); // Clear the focus if no interactable object is hit
        }
    }

    void Update()
    {
        checkInteractions();
    }

    void ClearFocus()
    {
        if (_currentFocus != null)
        {
            _currentFocus.OnDefocus();
            _currentFocus = null;
        }

        helpTextUI.text = "";
        helpTextUI.gameObject.SetActive(false);
        crosshairImage.color = defaultColor;
    }
}
