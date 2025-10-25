#nullable enable
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
            DoorMech? targetDoor;

            // For reasons I don't remember, in the `FOB_LOD` building we have to get
            // the DoorMech component from the parent of the collider we hit. However,
            // in other buildings, we can get it directly from the collider we hit.
            if (((targetDoor = hit.transform.GetComponent<DoorMech>()) == null)
                && (hit.collider.transform.parent != null))
            {
                targetDoor = hit.collider.transform.parent.GetComponent<DoorMech>();
            }

            if (targetDoor == null) return;

            targetDoor.doorBool = !targetDoor.doorBool;
            if (targetDoor.adjacentDoor != null)
            {
                targetDoor.adjacentDoor.doorBool = targetDoor.doorBool;
            }
        }
    }
}
