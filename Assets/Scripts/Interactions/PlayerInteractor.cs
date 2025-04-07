using NUnit.Framework.Internal.Commands;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 3f;
    public Image crosshairImage;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.green;
    // public CommandMenu commandMenu;

    private Interactable _currentFocus;

    private void checkInteractions()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green);
        if (Physics.Raycast(ray, out hit, interactRange, ~0))
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("Raycast hit: " + hit.collider.name);
            }
            
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                if (interactable != _currentFocus)
                {
                    ClearFocus();
                    _currentFocus = interactable;
                    _currentFocus.OnFocus();
                    crosshairImage.color = highlightColor;
                }

                if (Input.GetKeyDown(KeyCode.E))
                {
                    _currentFocus.Interact();
                }
            }
            else
            {
                ClearFocus();
            }
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

        crosshairImage.color = defaultColor;
    }
}
