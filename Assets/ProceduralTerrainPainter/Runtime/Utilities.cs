// Procedural Terrain Painter by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System;
using System.Collections.Generic;
using UnityEngine;

namespace sc.terrain.proceduralpainter
{
    public partial class Utilities
    {
        //Each splatmap has 4 channels, returns the number of splatmaps needed to fit all layers
        public static int GetSplatmapCount(int layerCount)
        {
            if (layerCount > 12) return 4;
            if (layerCount > 8) return 3;
            if (layerCount > 4) return 2;

            return 1;
        }
        
        public static int GetChannelIndex(int layerIndex)
        {
            return (layerIndex % 4);
        }
        
        //Create an RGBA component mask (eg. channelIndex=2 samples the Blue channel)
        public static Vector4 GetVectorMask(int channelIndex)
        {
            switch (channelIndex)
            {
                case 0: return new Vector4(1, 0, 0, 0);
                case 1: return new Vector4(0, 1, 0, 0);
                case 2: return new Vector4(0, 0, 1, 0);
                case 3: return new Vector4(0, 0, 0, 1);
                default: return Vector4.zero;
            }
        }
        
        public static int GetSplatmapIndex(int layerIndex)
        {
            if (layerIndex > 11) return 3;
            if (layerIndex > 7) return 2;
            if (layerIndex > 3) return 1;
            
            return 0;
        }
        
        public static Bounds RecalculateBounds(Terrain[] terrains)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.one);
            
			Vector3 minSum = Vector3.one * Mathf.Infinity;
            Vector3 maxSum = Vector3.one * Mathf.NegativeInfinity;
            Vector3 min = Vector3.zero;
            Vector3 max = Vector3.zero;
            
            foreach (Terrain terrain in terrains)
            {
                if (terrain == null || terrain.terrainData == null)
                {
                    throw new Exception("Failed to calculate total terrain bounds, one or more terrain objects are missing or broken");
                }

                //Min/max bounds corners in world-space
                min = terrain.GetPosition(); 
                max = terrain.GetPosition() + terrain.terrainData.size;

                minSum.x = Mathf.Min(minSum.x, min.x);
                minSum.y = Mathf.Min(minSum.y, min.y);
                minSum.z = Mathf.Min(minSum.z, min.z);
                
                //Must handle each axis separately, terrain may be further away, but not necessarily higher
                maxSum.x = Mathf.Max(maxSum.x, max.x);
                maxSum.y = Mathf.Max(maxSum.y, max.y);
                maxSum.z = Mathf.Max(maxSum.z, max.z);
            }

            bounds.SetMinMax(minSum, maxSum);

            //Increase bounds height for flat terrains
            if (bounds.size.y < 1f)
            {
                bounds.Encapsulate(new Vector3(bounds.center.x, bounds.center.y + 1f, bounds.center.z));
            }

            return bounds;
        }

        public static TerrainLayer[] SettingsToLayers(List<LayerSettings> layerSettings)
        {
            //Weirdness, using an array means the layers aren't actually assigned in reversed order
            List<TerrainLayer> layerList = new List<TerrainLayer>();
            
            //Convert LayerSettings to Layers
            for (int i = layerSettings.Count-1; i >= 0; i--)
            {
                layerList.Add(layerSettings[i].layer);
            }

            return layerList.ToArray();

        }

        public static bool HasMissingTerrain(Terrain[] terrains)
        {
            bool isMissing = false;

            for (int i = 0; i < terrains.Length; i++)
            {
                if (terrains[i] == null) isMissing = true;
            }

            return isMissing;
        }
    }
}