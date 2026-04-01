using System;
using UnityEngine;

namespace sc.terrain.proceduralpainter
{
    public class Noise : Modifier
    {
        public enum NoiseType
        {
            Simplex,
            Gradient
        }
        public NoiseType noiseType;

        public float noiseScale = 50f;
        public Vector2 noiseOffset;
        [Attributes.MinMaxSlider(0f,1f)]
        public Vector2 levels = new Vector2(0.5f, 1f);

        public void OnEnable()
        {
            passIndex = FilterPass.Noise;
        }
        
        public override void Configure(Material material)
        {
            base.Configure(material);
            
            material.SetVector("_NoiseScaleOffset", new Vector4(noiseScale * 0.001f, noiseScale * 0.001f, noiseOffset.x, noiseOffset.y));
            material.SetVector("_Levels", new Vector4(levels.x, levels.y, 0,0));
            material.SetInt("_NoiseType", (int)noiseType);
        }
    }
}