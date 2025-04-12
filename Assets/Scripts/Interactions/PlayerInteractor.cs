using NUnit.Framework.Internal.Commands;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 3f;
    public Image crosshairImage;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.green;
    public TextMeshProUGUI helpTextUI; 

    private Interactable _currentFocus;

    private void Start()
    {
        helpTextUI.gameObject.SetActive(false);
    }

    private void checkInteractions()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);
        Debug.DrawRay(ray.origin, ray.direction * interactRange, Color.green);

        if (Physics.Raycast(ray, out hit, interactRange, ~0))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != null)
            {
                if (interactable != _currentFocus)
                {
                    ClearFocus();
                    _currentFocus = interactable;
                    _currentFocus.OnFocus();
                    crosshairImage.color = highlightColor;

                    helpTextUI.text = interactable.HelpText; 
                    helpTextUI.gameObject.SetActive(true);
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
        else
        {
            ClearFocus();
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
