using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class floorlight_control : MonoBehaviour
{
    Material _lightMaterial = null!;
    Light _floorLight;

    void Start()
    {
        _floorLight = GetComponent<Light>();
        
        _lightMaterial = GetComponent<Renderer>().material;
        // _lightMaterial.SetColor("_Emission", Color.green);
        // _lightMaterial.SetColor("_BaseColor", Color.green);
        // .SetColor("_Emission", colorVariable);

        // _floorLight.color = Color.green;
    }

    void Update()
    {
        
    }
}
