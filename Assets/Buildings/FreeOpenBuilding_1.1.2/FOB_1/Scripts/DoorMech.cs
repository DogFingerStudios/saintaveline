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
    public float interactionDistance = 3f;


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
}