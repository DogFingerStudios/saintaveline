using UnityEngine;

public class SirenTrigger : MonoBehaviour
{
    public float delay = 10f;

    private AudioSource siren;

    void Start()
    {
        siren = GetComponent<AudioSource>();
        Invoke(nameof(PlaySiren), delay);
    }

    void PlaySiren()
    {
        if (siren != null && !siren.isPlaying)
        {
            siren.Play();
        }
    }
}
