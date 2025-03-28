using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f;

    private AudioSource audioSource;
    private CharacterController controller;
    private Vector3 lastPosition;
    private float stepTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        lastPosition = transform.position;
    }

    void Update()
    {
        Vector3 horizontalMove = transform.position - lastPosition;
        horizontalMove.y = 0f; // Ignore vertical movement

        bool isMovingHorizontally = horizontalMove.magnitude > 0.01f;
        bool isGrounded = controller.isGrounded;

        if (isMovingHorizontally && isGrounded)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }

        lastPosition = transform.position;
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        audioSource.PlayOneShot(footstepClips[index]);
    }
}
