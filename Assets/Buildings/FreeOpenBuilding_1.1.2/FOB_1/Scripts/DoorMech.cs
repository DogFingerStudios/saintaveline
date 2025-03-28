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

	void Update()
	{
		if (doorBool)
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(OpenRotation), rotSpeed * Time.deltaTime);
		}
		else
		{
			transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(CloseRotation), rotSpeed * Time.deltaTime);	
		}
	}
}

