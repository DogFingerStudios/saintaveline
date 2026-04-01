using UnityEngine;
using UnityEngine.Serialization;

namespace sc.terrain.proceduralpainter
{
    [System.Serializable]
    public class Slope : Modifier
    {
        [Attributes.MinMaxSlider(0f, 90f)]
        public Vector2 minMax = new Vector2(0, 90f);
        [Range(0.001f, 90f)] public float minFalloff = 10;
        [Range(0.001f, 90f)] public float maxFalloff = 10;
        
        public void OnEnable()
        {
            passIndex = FilterPass.Slope;
        }
        
        public override void Configure(Material material)
        {
            base.Configure(material);

            material.SetVector("_MinMaxSlope", new Vector4(minMax.x, minMax.y, minFalloff, maxFalloff));
        }
    }
}