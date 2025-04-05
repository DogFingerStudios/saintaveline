using UnityEngine;
using UnityEngine.UI;

public class PlayerInteractor : MonoBehaviour
{
    public float interactRange = 3f;
    public Image crosshairImage;
    public Color defaultColor = Color.white;
    public Color highlightColor = Color.green;
    public CommandMenu commandMenu;

    private Interactable _currentFocus;

    private void checkInteractions()
    {
        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, interactRange))
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

            if (hit.collider.CompareTag("FriendlyNPC"))
            {
                SonNPC son = hit.collider.GetComponent<SonNPC>();
                if (son != null)
                {
                    if (Input.GetKeyDown(KeyCode.E))
                    {
                        commandMenu.Open(son);
                    }
                }
            }
        

        }
    }

    void Update()
    {
        checkInteractions();

        // Ray ray = new Ray(transform.position, transform.forward);
        // RaycastHit hit;

        // if (Physics.Raycast(ray, out hit, interactRange))
        // {
        //     Interactable interactable = hit.collider.GetComponent<Interactable>();
        //     if (interactable != null)
        //     {
        //         if (interactable != currentFocus)
        //         {
        //             ClearFocus();
        //             currentFocus = interactable;
        //             currentFocus.OnFocus();
        //         }

        //         crosshairImage.color = highlightColor;

        //         // if (Input.GetMouseButtonDown(0))
        //         if (Input.GetKeyDown(KeyCode.E))
        //         {
        //             currentFocus.Interact();
        //         }
        //     }
        //     else
        //     {
        //         ClearFocus();
        //     }
        
        //     if (Input.GetKeyDown(KeyCode.E))
        //     {
        //         if (hit.collider.CompareTag("FriendlyNPC"))
        //         {
        //             SonNPC son = hit.collider.GetComponent<SonNPC>();
        //             if (son != null)
        //             {
        //                 commandMenu.Open(son);
        //             }
        //         }
        //     }

        
        // }
        // else
        // {
        //     ClearFocus();
        // }
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
