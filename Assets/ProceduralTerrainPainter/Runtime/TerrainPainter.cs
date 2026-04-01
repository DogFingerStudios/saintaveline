// Procedural Terrain Painter by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if __MICROSPLAT__
using JBooth.MicroSplat;
#endif

namespace sc.terrain.proceduralpainter
{
    [ExecuteInEditMode]
    public partial class TerrainPainter : MonoBehaviour
    {
        public const string Version = "1.0.5";
        public static TerrainPainter Current;

        public Terrain[] terrains = new Terrain[0];
        [Attributes.ResolutionDropdown(64, 1024)] 
        public int splatmapResolution = 256;
        [Attributes.ResolutionDropdown(16, 2048)]
        [Tooltip("The color/base map is a pre-rendered texture for the terrain color. This is shown on the terrain in the distance. High resolutions usually have little benefit")]
        public int colorMapResolution = 256;

        public List<LayerSettings> layerSettings = new List<LayerSettings>();

        /// <summary>
        /// Repaints a terrain if its heightmap is modified. The SetAutoRepaint function must be used to set this up for the terrains. In the editor, the inspector automatically handles this
        /// </summary>
        [Tooltip("Automatically repaint the terrains if their heightmap is modified. Repaints when the left-mouse button is released")]
        public bool autoRepaint;
        public List<TerrainChangeListener> terrainListeners = new List<TerrainChangeListener>();
        public Bounds bounds;
        
#if VEGETATION_STUDIO_PRO
        [Tooltip("Refreshes the vegetation systems after painting. If vegetation items use terrain layers masks, this is useful")]
        public bool refreshVegetationOnPaint;
#endif
        #if __MICROSPLAT__
        [Tooltip("Assign the TextureArrayConfig asset here. Adding, removing or re-ordering layers will be also be applied to the texture array")]
        public TextureArrayConfig msTexArray;
        #endif
        
        [SerializeField]
        //Reference it once here, so it gets included in a build
        private Shader filterShader;
        
        public delegate void TerrainRepaintEvent(Terrain terrain);
        /// <summary>
        /// Triggers whenever a terrain is repainted. Passes the context terrain as a parameter.
        /// </summary>
        public static event TerrainRepaintEvent OnTerrainRepaint;

        private void Reset()
        {
            filterShader = Shader.Find("Hidden/TerrainPainter/Modifier");
        }
        
        private void OnEnable()
        {
            Current = this;
        }

        private void OnDisable()
        {
            Dispose();
        }

        /// <summary>
        /// Applies the splatmapResolution value to all terrains. This must be called when changing the resolution before repainting a single terrain. Automatically done in the <see cref="RepaintAll"/> function
        /// </summary>
        public void ResizeSplatmaps()
        {
            //Needs to happen before repainting, all terrains must have the same splatmap resolution. PaintContext throws warnings otherwise
            
            foreach (Terrain terrain in terrains)
            {
                if(terrain) terrain.terrainData.alphamapResolution = splatmapResolution;
            }
        }
        
        public void RecalculateBounds()
        {
            bounds = Utilities.RecalculateBounds(terrains);
        }

        [ContextMenu("Assign active terrains")]
        public void AssignActiveTerrains()
        {
            SetTargetTerrains(Terrain.activeTerrains);
            RepaintAll();
        }

        public void SetTargetTerrains(Terrain[] terrains)
        {
            this.terrains = terrains;
            RecalculateBounds();
        }

        /// <summary>
        /// Repaints all the assigned terrains using the current configuration
        /// </summary>
        /// <param name="syncCPU">(Slow) Copy the internal splatmap render texture to the terrain's "Alphamaps" float arrays. Making it possible to use Terrain.TerrainData.GetAlphamaps()</param>
        public void RepaintAll(bool syncCPU = false)
        {
            if (layerSettings.Count == 0) return;

            ResizeSplatmaps();

            foreach (Terrain terrain in terrains)
            {
                if (!terrain)
                {
                    Debug.LogError("Missing terrain assigned to TerrainPainter", this);
                    continue;
                }
                
                RepaintTerrain(terrain, syncCPU);
            }
            
            //ApplyAllStamps();
        }

        public void Dispose()
        {
            ModifierStack.Dispose();
        }

        /// <summary>
        /// Repaints an individual terrain
        /// </summary>
        /// <param name="terrain"></param>
        /// <param name="syncCPU">(Slow) Copy the internal splatmap render texture to the terrain's "Alphamaps" float arrays. Making it possible to use Terrain.TerrainData.GetAlphamaps()</param>
        public void RepaintTerrain(Terrain terrain, bool syncCPU = false)
        {
            if (layerSettings.Count == 0 || terrain == null) return;
            
            ModifierStack.Configure(terrain, bounds, splatmapResolution);
            
            ModifierStack.ProcessLayers(terrain, layerSettings);

            ApplyStampsToTerrain(terrain);
            
            //Regenerate basemap
            terrain.terrainData.baseMapResolution = colorMapResolution;
            terrain.terrainData.SetBaseMapDirty();

            RefreshVegetation(terrain);

            OnTerrainRepaint?.Invoke(terrain);

            if (syncCPU) FinalizeTerrain(terrain);

#if UNITY_EDITOR
            EditorUtility.SetDirty(terrain.terrainData);
#endif
        }

        private void RefreshVegetation(Terrain terrain)
        {
#if VEGETATION_STUDIO_PRO
            AwesomeTechnologies.VegetationStudio.VegetationStudioManager manager = AwesomeTechnologies.VegetationStudio.VegetationStudioManager.Instance;

            if (refreshVegetationOnPaint && manager)
            {
#if UNITY_2019_1_OR_NEWER
                //VS requires the CPU data
                terrain.terrainData.SyncTexture(TerrainData.AlphamapTextureName);
#endif
                foreach (AwesomeTechnologies.VegetationSystem.VegetationSystemPro sys in manager.VegetationSystemList)
                {
                    if (sys == null) continue;
                    
                    sys.ClearCache();
                    sys.RefreshTerrainArea(terrain.terrainData.bounds);
                }
#if UNITY_EDITOR
                SceneView.RepaintAll();
#endif
            }
#endif
        }

        public void CreateSettingsForLayer(TerrainLayer layer)
        {
            LayerSettings s = new LayerSettings();
            s.layer = layer;
            s.modifierStack = new List<Modifier>();
            
            layerSettings.Insert(0, s);

            SetTerrainLayers();
        }

        /// <summary>
        /// Adds or removes the TerrainChangeListener component from all assigned terrains. If enabled, terrains will be repainted when their height is modified
        /// </summary>
        /// <param name="value"></param>
        public void SetAutoRepaint(bool value)
        {
            autoRepaint = value;
            
            if (value)
            {
                RemoveTerrainListeners();

                foreach (Terrain terrain in terrains)
                {
                    TerrainChangeListener listener = terrain.GetComponent<TerrainChangeListener>();
                    if (!listener) listener = terrain.gameObject.AddComponent<TerrainChangeListener>();

                    listener.terrain = terrain;
                    this.terrainListeners.Add(listener);
                }
            }
            else
            {
                RemoveTerrainListeners();
            }
        }

        private void RemoveTerrainListeners()
        {
            for (int i = 0; i < terrainListeners.Count; i++)
            {
                DestroyImmediate(terrainListeners[i]);
            }
            
            terrainListeners.Clear();
        }
        
        /// <summary>
        /// Ensures that all configured layers are in fact assigned to the terrains. Also removed if they were.
        /// </summary>
        [ContextMenu("Set terrain layers")]
        public void SetTerrainLayers()
        {
            TerrainLayer[] layers = Utilities.SettingsToLayers(layerSettings);
            
            foreach (Terrain terrain in terrains)
            {
                terrain.terrainData.terrainLayers = layers;
                terrain.terrainData.SetBaseMapDirty();
                
#if UNITY_EDITOR
                EditorUtility.SetDirty(terrain.terrainData);
#endif
            }
        }

        /// <summary>
        /// Copy the internal splatmap render texture to the terrain's "Alphamaps" float arrays. Making it possible to use (the slower) scipting operations such as Terrain.TerrainData.GetAlphamaps()
        /// This also saves the data to disk
        /// </summary>
        public void FinalizeChanges()
        {
            foreach (Terrain terrain in terrains)
            {
                FinalizeTerrain(terrain);
            }
        }

        private void FinalizeTerrain(Terrain terrain)
        {
            terrain.terrainData.SetBaseMapDirty();
            terrain.terrainData.SyncTexture(TerrainData.AlphamapTextureName);
                
#if UNITY_EDITOR
            EditorUtility.SetDirty(terrain.terrainData);
#endif
        }
        
        #region Virtual
        //Only should be called after the modifiers have been applied, otherwise it acts as a persistent brush
        private void ApplyStampsToTerrain(Terrain terrain)
        {
            ApplyStamps(terrain);
        }

        partial void ApplyAllStamps();

        partial void ApplyStamps(Terrain terrain);
        #endregion
    }
}