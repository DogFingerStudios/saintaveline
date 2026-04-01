using UnityEngine;

namespace sc.terrain.proceduralpainter
{
    [System.Serializable]
    public class Curvature : Modifier
    {
        public enum CurvatureSolver
        {
            Soft,
            Hard
        }
        public CurvatureSolver solver;
        
        [Attributes.MinMaxSlider(0f, 1f)]
        [Min(0f)] public Vector2 minMax = new Vector2(0f, 0.25f);
        [Range(1f, 16f)] public float radius = 1;
        [Range(0.001f, 1f)] public float minFalloff = 0.001f;
        [Range(0.001f, 1f)] public float maxFalloff = 0.001f;
        
        public void OnEnable()
        {
            passIndex = FilterPass.Curvature;
        }
        
        public override void Configure(Material material)
        {
            base.Configure(material);
            
            material.SetInt("_CurvatureSolver", (int)solver);
            material.SetFloat("_CurvatureRadius", radius);
            material.SetVector("_MinMaxCurvature", new Vector4(minMax.x, minMax.y, minFalloff,maxFalloff));
        }
    }
}