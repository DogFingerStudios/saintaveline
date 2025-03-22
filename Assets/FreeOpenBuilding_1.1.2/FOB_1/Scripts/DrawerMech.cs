using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawerMech : MonoBehaviour 
{

	public Vector3 OpenPosition, ClosePosition;

	float moveSpeed;

    float lerpTimer;

    public bool drawerBool;

    

	void Start()
	{
        drawerBool = false;
	}
		
	void OnTriggerStay(Collider col)
	{
		if(col.gameObject.tag == ("Player") && Input.GetKeyDown(KeyCode.E))
		{
			if (!drawerBool)
                drawerBool = true;
			else
                drawerBool = false;
		}
	}

	void Update()
	{

        if (drawerBool)
        {
            moveSpeed = +1f;

            lerpTimer = Mathf.Clamp(lerpTimer + Time.deltaTime * moveSpeed, 0f, 1f);

            transform.localPosition = Vector3.Lerp(ClosePosition, OpenPosition, lerpTimer);
        }
            
        else
        {
            moveSpeed = -1f;

            lerpTimer = Mathf.Clamp(lerpTimer + Time.deltaTime * moveSpeed, 0f, 1f);

            transform.localPosition = Vector3.Lerp(ClosePosition, OpenPosition, lerpTimer);
        }

    }

}

