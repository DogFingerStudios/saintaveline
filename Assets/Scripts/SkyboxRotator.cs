using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    public float rotationSpeed = 100.5f; // Play with this
    private Material runtimeSkybox;

    void Start()
    {
        // Clone the original so we don’t modify the asset
        runtimeSkybox = new Material(RenderSettings.skybox);
        RenderSettings.skybox = runtimeSkybox;
    }


    void Update()
    {
        runtimeSkybox.SetFloat("_Rotation", Time.time * rotationSpeed);
    }
}
