using UnityEngine;

public class DoorInteraction : MonoBehaviour
{
    public float interactionDistance = 3f;
    private Camera playerCamera;
    public HealthBar healthBar;
    
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

        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            healthBar.SetHealth(healthBar.CurrentHealth + 5f);
        }
        
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            healthBar.SetHealth(healthBar.CurrentHealth - 5f);
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
                targetDoor.doorBool = !targetDoor.doorBool;
                if (targetDoor.name == "DoorEntry_R_LOD")
                {
                    DoorMech leftDoor = GameObject.Find("DoorEntry_L_LOD").GetComponent<DoorMech>();
                    if (leftDoor != null)
                    {
                        leftDoor.doorBool = targetDoor.doorBool;
                    }
                }
                else if (targetDoor.name == "DoorEntry_L_LOD")
                {
                    DoorMech leftDoor = GameObject.Find("DoorEntry_R_LOD").GetComponent<DoorMech>();
                    if (leftDoor != null)
                    {
                        leftDoor.doorBool = targetDoor.doorBool;
                    }
                }
            }
        }
    }
}
