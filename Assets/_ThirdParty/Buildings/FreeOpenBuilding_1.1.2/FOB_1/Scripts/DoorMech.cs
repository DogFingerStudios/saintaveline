#nullable enable
using System.Collections.Generic;
using UnityEngine;

// This is attached to doors, drawers and other things that open and close.
public class DoorMech : MonoBehaviour, IInteractable
{
    // Cached the transform. Using transform directly always
    // uses GetComponent behind the scenes, so this script was
    // taking up about 50% of the CPU time (at time of change) with the GetComponent call alone.
    private Transform _transform;
    public Vector3 OpenRotation, CloseRotation;
    public float rotSpeed = 1f;
    public bool doorBool;
    public float interactionDistance = 3f;
    public DoorMech? adjacentDoor;

    private void Awake()
    {
        _transform = transform;
        doorBool = false;
    }

    void Update()
    {
        Vector3 targetRotation = doorBool ? OpenRotation : CloseRotation;
        if (_transform.localRotation.eulerAngles != targetRotation)
        {
            _transform.localRotation = Quaternion.Lerp(_transform.localRotation, Quaternion.Euler(targetRotation), rotSpeed * Time.deltaTime);
        }
    }

    public string HoverText
    {
        get
        {
            if (doorBool)
            {
                return "Press [Q] to close";
            }

            return "Press [Q] to open";
        }
    }

    public List<InteractionData> Interactions => new List<InteractionData>();

    public void OnFocus()
    {
        Debug.Log("Door focused");
    }

    public void OnDefocus()
    {
        Debug.Log("Door defocused");
    }

    public void Interact(GameEntity? interactor = null)
    {
        doorBool = !doorBool;
        if (adjacentDoor != null) adjacentDoor.doorBool = doorBool;
    }
}
