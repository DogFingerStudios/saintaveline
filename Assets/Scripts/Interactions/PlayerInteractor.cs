using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 3f;
    public Image crosshairImage;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.green;

    private Interactable currentFocus;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactRange))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                if (interactable != currentFocus)
                {
                    ClearFocus();
                    currentFocus = interactable;
                    currentFocus.OnFocus();
                }

                crosshairImage.color = highlightColor;

                if (Input.GetMouseButtonDown(0))
                {
                    currentFocus.Interact();
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

    void ClearFocus()
    {
        if (currentFocus != null)
        {
            currentFocus.OnDefocus();
            currentFocus = null;
        }

        crosshairImage.color = defaultColor;
    }
}
