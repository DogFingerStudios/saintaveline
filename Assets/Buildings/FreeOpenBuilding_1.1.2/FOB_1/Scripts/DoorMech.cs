using UnityEngine;

public class DoorMech : MonoBehaviour 
{
    // Cached the transform. Using transform directly always
    // uses GetComponent behind the scenes, so this script was
    // taking up about 50% of the CPU time (at time of change) with the GetComponent call alone.
    private Transform _transform; 

	public Vector3 OpenRotation, CloseRotation;

	public float rotSpeed = 1f;

	public bool doorBool;

	// private float doorTimer = 0f;

    public float interactionDistance = 3f;
    private Camera playerCamera;


    private void Awake() {
        _transform = transform;
        playerCamera = Camera.main;

        doorBool = false;        
    }

	void Update()
	{
        Vector3 targetRotation = doorBool ? OpenRotation : CloseRotation;
        if (_transform.rotation.eulerAngles != targetRotation) 
        {
            _transform.rotation = Quaternion.Lerp(_transform.rotation, Quaternion.Euler(targetRotation), rotSpeed * Time.deltaTime);
        }
	}
}