using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    public AudioClip[] concreteSteps;
    public AudioClip[] woodSteps;
    public AudioClip[] dirtSteps;
    public float stepInterval = 0.5f;
    // public LayerMask groundLayers;

    private CharacterController controller;
    private AudioSource audioSource;
    private float stepTimer;
    private Vector3 lastPosition;

    public bool IsEnabled { get; set; } = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        lastPosition = transform.position;
    }

    void Update()
    {
        if (!IsEnabled) return;

        Vector3 move = transform.position - lastPosition;
        move.y = 0f;

        if (move.magnitude > 0.01f && controller.isGrounded)
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
        SurfaceType surface = DetectSurface();

        AudioClip clip = GetRandomClipForSurface(surface);
        if (clip != null)
        {
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(clip);
        }
    }

    SurfaceType DetectSurface()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2f))
        {
            SurfaceIdentifier identifier = hit.collider.GetComponent<SurfaceIdentifier>();
            if (identifier != null)
            {
                return identifier.surfaceType;
            }
        }

        return SurfaceType.Concrete; // Default
    }

    AudioClip GetRandomClipForSurface(SurfaceType surface)
    {
        switch (surface)
        {
            case SurfaceType.Concrete:
                return concreteSteps[Random.Range(0, concreteSteps.Length)];
            case SurfaceType.Wood:
                return woodSteps[Random.Range(0, woodSteps.Length)];
            case SurfaceType.Dirt:
                return dirtSteps[Random.Range(0, dirtSteps.Length)];
            default:
                return null;
        }
    }
}

