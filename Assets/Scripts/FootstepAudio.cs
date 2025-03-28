using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    public AudioClip[] footstepClips;
    public float stepInterval = 0.5f;

    private CharacterController controller;
    private AudioSource audioSource;
    private float stepTimer;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        bool isMoving = controller.velocity.magnitude > 0.01f;// && controller.isGrounded;

        if (isMoving)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f; // Reset the timer when not moving
        }
    }

    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        audioSource.PlayOneShot(footstepClips[index]);
    }
}
