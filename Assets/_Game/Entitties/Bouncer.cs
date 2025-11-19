using UnityEngine;

using UnityEngine;

public class Bouncer : MonoBehaviour
{
    [SerializeField]
    private Vector3 _destinationPosition;

    [SerializeField]
    private float _duration = 2f;

    // AI: Internal timer
    private float _elapsed = 0f;

    // AI: Whether the Item is currently lerping
    private bool _isLerping = false;

    // AI: Cached starting position for this lerp
    private Vector3 _startPosition;

    private void Start()
    {
        // AI: Optional – if you want to default the destination to some value at runtime:
        // _destinationPosition = new Vector3(transform.position.x, 890f, transform.position.z);
    }

    private void Update()
    {
        if (_isLerping)
        {
            _elapsed += Time.deltaTime;

            float t = _elapsed / _duration;

            if (t > 1f)
            {
                t = 1f;
            }

            // AI: Lerp entire position from start to destination
            Vector3 newPosition = Vector3.Lerp(_startPosition, _destinationPosition, t);

            transform.position = newPosition;

            if (t >= 1f)
            {
                _isLerping = false;
            }
        }
    }

    // AI: Call this when you want to begin moving toward _destinationPosition
    public void BeginLerp()
    {
        _startPosition = transform.position;
        _elapsed = 0f;
        _isLerping = true;
    }

}




// 884

// public class Bouncer : MonoBehaviour
// {
//     public float moveDistance = 2f; // how far up/down
//     public float moveDuration = 2f; // total time for up + down
//     public float bounceStartPosY = 0f;

//     private Vector3 startPos;
//     private SpriteRenderer spriteRenderer;
//     private bool isBouncing = false;

//     void Start()
//     {
//         spriteRenderer = GetComponent<SpriteRenderer>();
//         spriteRenderer.enabled = false;


//         startPos = transform.position;
//     }

//     void BounceUpdate()
//     {
//         float t = Mathf.PingPong(Time.time, moveDuration) / moveDuration;
//         float yOffset = Mathf.Lerp(-moveDistance, moveDistance, t);
//         transform.position = startPos + new Vector3(0, yOffset, 0);
//     }

//     void Update()
//     {
//         if (isBouncing)
//         {
//             BounceUpdate();
//             return;
//         }

//     }

//     public void SetVisible(bool isVisible)
//     {
//         if (spriteRenderer != null)
//         {
//             spriteRenderer.enabled = isVisible;
//         }
//     }
// }
