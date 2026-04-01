using System;
using System.Linq;
using UnityEngine;

namespace sc.terrain.proceduralpainter
{
    [ExecuteInEditMode]
    [AddComponentMenu("")] //Hide
    public class TerrainChangeListener : MonoBehaviour
    {
        [HideInInspector]
        public Terrain terrain;

        private void Reset()
        {
            terrain = GetComponent<Terrain>();
        }

        void OnTerrainChanged(TerrainChangedFlags flags)
        {
            if (!terrain || !TerrainPainter.Current) return;

            if ((flags & TerrainChangedFlags.Heightmap) != 0)
            {
                if(TerrainPainter.Current.autoRepaint && TerrainPainter.Current.terrains.Contains(terrain)) TerrainPainter.Current.RepaintTerrain(terrain);
            }
        }
    }
}