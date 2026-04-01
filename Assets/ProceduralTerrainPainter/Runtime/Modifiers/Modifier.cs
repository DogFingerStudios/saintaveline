using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace sc.terrain.proceduralpainter
{
    [Serializable]
    public class Modifier : ScriptableObject
    {
        [HideInInspector]
        public bool enabled = true;
        [HideInInspector]
        public string label;
        [HideInInspector]
        public BlendMode blendMode;
        [HideInInspector]
        [Range(0f, 100f)]
        public float opacity = 100;
        
		public enum FilterPass
        {
            Height,
            Slope,
            Curvature,
            TextureMask,
            Noise,
            Direction
        }
        [HideInInspector]
        public FilterPass passIndex;

        public enum BlendMode
        {
            Multiply,
            Add,
            Subtract,
            Min,
            Max
        }

        private static int SrcFactorID = Shader.PropertyToID("_SrcFactor");
        private static int DstFactorID = Shader.PropertyToID("_DstFactor");
        private static int BlendOpID = Shader.PropertyToID("_BlendOp");
        private static int OpacityID = Shader.PropertyToID("_Opacity");

        public static void SetBlendMode(Material mat, BlendMode mode)
        {
            //Note: The source is actually the current filter, destination the result from previous filters
            //Meaning operations are reversed

            switch (mode)
            {
                case BlendMode.Multiply:
                {
                    mat.SetInt(SrcFactorID, (int)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetInt(DstFactorID, (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    mat.SetInt(BlendOpID, (int)BlendOp.Multiply);
                } break;
                case BlendMode.Add:
                {
                    mat.SetInt(SrcFactorID, (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    mat.SetInt(DstFactorID, (int)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetInt(BlendOpID, (int)BlendOp.Add);
                } break;
                case BlendMode.Subtract:
                {
                    mat.SetInt(SrcFactorID, (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    mat.SetInt(DstFactorID, (int)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetInt(BlendOpID, (int)BlendOp.ReverseSubtract);
                } break;
                case BlendMode.Min:
                {
                    mat.SetInt(SrcFactorID, (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    mat.SetInt(DstFactorID, (int)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetInt(BlendOpID, (int)BlendOp.Min);
                } break;
                case BlendMode.Max:
                {
                    mat.SetInt(SrcFactorID, (int)UnityEngine.Rendering.BlendMode.SrcColor);
                    mat.SetInt(DstFactorID, (int)UnityEngine.Rendering.BlendMode.DstColor);
                    mat.SetInt(BlendOpID, (int)BlendOp.Max);
                } break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
            
        }  

        /// <summary>
        /// Set any material properties here. Base implementation must be called, it sets the blend mode and opacity
        /// </summary>
        public virtual void Configure(Material material)
        {
            SetBlendMode(material, blendMode);
            material.SetFloat(OpacityID, opacity * 0.01f);
        }
        
        public virtual void Execute(RenderTexture target)
        {
            if (!enabled || opacity == 0) return;
            
            //Debug.Log("<b>Executing:</b> " + this.name + ". SRC: <i>" + source.name + "</i> DEST: <i>" + destination.name + "</i>. Pass: " + passIndex);
            
            Graphics.Blit(null, target, ModifierStack.filterMat, (int)passIndex);
        }
    }
}