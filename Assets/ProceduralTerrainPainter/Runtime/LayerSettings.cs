// Procedural Terrain Painter by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System.Collections.Generic;
using UnityEngine;

namespace sc.terrain.proceduralpainter
{
    [System.Serializable]
    public class LayerSettings
    {
        public bool enabled = true;
        public TerrainLayer layer;

        public List<Modifier> modifierStack = new List<Modifier>();
    }

}