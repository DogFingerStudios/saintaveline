using UnityEngine;

public class Bouncer : MonoBehaviour
{
    public float moveDistance = 2f; // how far up/down
    public float moveDuration = 2f; // total time for up + down

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        float t = Mathf.PingPong(Time.time, moveDuration) / moveDuration;
        float yOffset = Mathf.Lerp(-moveDistance, moveDistance, t);
        transform.position = startPos + new Vector3(0, yOffset, 0);
    }
}
