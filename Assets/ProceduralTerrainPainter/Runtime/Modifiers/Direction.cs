using System;
using UnityEngine;

namespace sc.terrain.proceduralpainter
{
    [System.Serializable]
    public class Direction : Modifier
    {
        [Range(0f, 90f)]
        public float xAngle = 45f;
        
        [Range(0f, 360f)]
        public float yAngle = 0f;

        public bool addSunDirection;
        
        [Attributes.MinMaxSlider(0f, 1f)]
        [Min(0f)] public Vector2 levels = new Vector2(0f, 1f);
        
        public void OnEnable()
        {
            passIndex = FilterPass.Direction;
        }

        private readonly int _Direction = Shader.PropertyToID("_Direction");
        private readonly int _DirectionLevels = Shader.PropertyToID("_DirectionLevels");

        public override void Configure(Material material)
        {
            base.Configure(material);
            
            material.SetVector(_Direction, Quaternion.Euler(xAngle + (addSunDirection ? RenderSettings.sun.transform.eulerAngles.x : 0), yAngle + (addSunDirection ? RenderSettings.sun.transform.eulerAngles.y : 0), 0f) * Vector3.forward);
            material.SetVector(_DirectionLevels, new Vector2(levels.x, levels.y));
        }
    }
}