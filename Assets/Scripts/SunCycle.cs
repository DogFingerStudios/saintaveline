using UnityEngine;

public class SunCycle : MonoBehaviour
{
    public float dayDuration = 60f; // Duration of a full day in seconds
    public Light directionalLight; // Reference to the Directional Light
    public Gradient sunColor; // Color gradient for the sun (optional)

    private float rotationSpeed;

    void Start()
    {
        if (directionalLight == null)
        {
            Debug.LogError("Directional Light not assigned!");
            return;
        }

        // Calculate the rotation speed (360 degrees in dayDuration seconds)
        rotationSpeed = 360f / dayDuration;
    }

    void Update()
    {
        if (directionalLight != null)
        {
            // Rotate the light around the X-axis
            directionalLight.transform.Rotate(Vector3.right * rotationSpeed * Time.deltaTime);

            float intensity = Mathf.Clamp01(Vector3.Dot(directionalLight.transform.forward, Vector3.down));
            directionalLight.intensity = intensity;

            // Optionally update the sun's color based on its rotation angle
            if (sunColor != null)
            {
                float timeOfDay = Mathf.PingPong(Time.time / dayDuration, 1);
                directionalLight.color = sunColor.Evaluate(timeOfDay);
            }
        }
    }
}
