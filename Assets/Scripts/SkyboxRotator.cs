using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    public float rotationSpeed = 100.5f; // Play with this

    void Update()
    {
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
    }
}
