using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    private Camera playerCamera;
    
    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        // Cast a ray from the player toward the center of the screen
        if (Input.GetKeyDown(KeyCode.E))
        {
            TryInteractWithDoor();
        }
    }

    void TryInteractWithDoor()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDistance))
        {
            // Check if the object hit by the ray has the door script
            // DoorMech targetDoor = hit.collider.GetComponent<DoorMech>();
            DoorMech targetDoor = hit.collider.transform.parent != null ? hit.collider.transform.parent.GetComponent<DoorMech>() : null;
            if (targetDoor != null)
            {
                // targetDoor.ToggleDoor();  // Open or close the door
                Debug.Log("Door named: " + targetDoor.name + " was interacted with.");
                targetDoor.doorBool = !targetDoor.doorBool;
            }
        }
    }
}
