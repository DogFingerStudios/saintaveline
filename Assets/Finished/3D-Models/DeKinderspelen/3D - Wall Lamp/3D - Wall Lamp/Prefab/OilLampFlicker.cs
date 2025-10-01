using UnityEngine;

public class OilLampFlicker : MonoBehaviour
{
    public Light oilLampLight;
    public float minIntensity = 0.8f;
    public float maxIntensity = 1.2f;
    public float flickerSpeed = 0.1f;
    public float secondaryFlickerSpeed = 0.3f; // Slower secondary flicker for variation

    void Update()
    {
        // Primary flicker (fast)
        float noise1 = Mathf.PerlinNoise(Time.time * flickerSpeed, 0);
        // Secondary flicker (slower, for subtle variation)
        float noise2 = Mathf.PerlinNoise(Time.time * secondaryFlickerSpeed, 1); // Different seed
        // Combine noises (e.g., average or weighted sum)
        float combinedNoise = (noise1 + noise2 * 0.5f) / 1.5f; // Normalize to ~0-1
        oilLampLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, combinedNoise);
    }
}
