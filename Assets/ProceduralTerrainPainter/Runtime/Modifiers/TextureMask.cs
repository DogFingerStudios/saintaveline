using UnityEngine;

namespace sc.terrain.proceduralpainter
{
    [System.Serializable]
    public class TextureMask : Modifier
    {
        public Texture2D texture;
        [Attributes.ChannelPicker]
        public int channel;

        [Tooltip("Spans the texture across all terrains")]
        public bool spanTerrains;
        public float tiling = 1f;
        
        public void OnEnable()
        {
            passIndex = FilterPass.TextureMask;
        }
        
        public override void Configure(Material material)
        {
            base.Configure(material);

            material.SetTexture("_MaskTexture", texture);
            material.SetVector("_TilingParams", new Vector4(tiling, spanTerrains ? 1 : 0, 0f, 0f));
            material.SetInt("_Channel", channel);
        }
    }
}