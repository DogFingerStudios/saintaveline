using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorMech : MonoBehaviour 
{

	public Vector3 OpenRotation, CloseRotation;

	public float rotSpeed = 1f;

	public bool doorBool;

	private float doorTimer = 0f;


	void Start()
	{
		doorBool = false;
	}
		
	void OnTriggerStay(Collider col)
	{
		if(col.gameObject.tag == ("Player") && Input.GetKeyDown(KeyCode.E))
		{
			if (!doorBool)
				doorBool = true;
			else
				doorBool = false;
		}
	}

	void Update()
	{
		doorTimer += Time.deltaTime;
		if (doorTimer >= 5f)
		{
			doorBool = !doorBool;
			doorTimer = 0f;
		}

		if (doorBool)
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (OpenRotation), rotSpeed * Time.deltaTime);
		else
			transform.rotation = Quaternion.Lerp (transform.rotation, Quaternion.Euler (CloseRotation), rotSpeed * Time.deltaTime);	
	}

}

