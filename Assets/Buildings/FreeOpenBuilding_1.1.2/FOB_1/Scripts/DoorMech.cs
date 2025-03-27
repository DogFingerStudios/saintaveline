using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMech : MonoBehaviour 
{

	public Vector3 OpenRotation, CloseRotation;

	public float rotSpeed = 1f;

	public bool doorBool;

	private float doorTimer = 0f;


    public float interactionDistance = 3f;
    private Camera playerCamera;

	void Start()
	{
		doorBool = false;
		playerCamera = Camera.main;
	}
		
	// void OnTriggerStay(Collider col)
	// {
	// 	if(col.gameObject.tag == ("Player") && Input.GetKeyDown(KeyCode.E))
	// 	{
	// 		if (!doorBool)
	// 			doorBool = true;
	// 		else
	// 			doorBool = false;
	// 	}
	// }

	void Update()
	{
		// if (Input.GetKeyDown(KeyCode.V))
		// {
		// 	var name = this.name;
		// 	Debug.Log(name + " doorBool: " + doorBool);		
		// }
		// // doorTimer += Time.deltaTime;
		// // if (doorTimer >= 5f)
		// // {
		// // 	doorBool = !doorBool;
		// // 	doorTimer = 0f;
		// // }

        // if (Input.GetKeyDown(KeyCode.E))
        // {
        //     TryInteractWithDoor();
        // }


		if (doorBool)
		{
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (OpenRotation), rotSpeed * Time.deltaTime);
		}
		else
		{
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (CloseRotation), rotSpeed * Time.deltaTime);	
		}
	}

	// void TryInteractWithDoor()
	// {
	// 	Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
	// 	RaycastHit hit;

	// 	if (Physics.Raycast(ray, out hit, interactionDistance))
	// 	{
	// 		// // Check if the object hit by the ray has the door script
	// 		DoorMech targetDoor = hit.collider.transform.parent != null ? hit.collider.transform.parent.GetComponent<DoorMech>() : null;
	// 		if (targetDoor != null)
	// 		{
	// 			// targetDoor.ToggleDoor();  // Open or close the door
	// 			// doorBool = !doorBool;
	// 			targetDoor.doorBool = !targetDoor.doorBool;
	// 		}
	// 	}
	// }

}

