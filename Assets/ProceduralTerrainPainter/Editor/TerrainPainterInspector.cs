// Procedural Terrain Painter by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

using System;
using System.Collections.Generic;
using System.Linq;
#if __MICROSPLAT__
using System.Reflection;
using JBooth.MicroSplat;
#endif
using UnityEditor;
using UnityEditor.AnimatedValues;
using UnityEditorInternal;
using UnityEngine;
#if UNITY_2021_2_OR_NEWER
using UnityEngine.TerrainTools;
#else
using UnityEngine.Experimental.TerrainAPI;
#endif

namespace sc.terrain.proceduralpainter
{
    [CustomEditor(typeof(TerrainPainter))]
    public class TerrainPainterInspector : Editor
    {
        TerrainPainter script;
        private SerializedProperty layerSettings;
        private SerializedProperty resolution;
        private SerializedProperty colorMapResolution;
        private SerializedProperty terrains;
        private SerializedProperty autoRepaint;
        private Dictionary<LayerSettings, ReorderableList> m_modifierList = new Dictionary<LayerSettings, ReorderableList>();
        private ReorderableList curModList;
        
        //Setup warnings
        private bool hasMissingTerrains;
        private bool instancedTerrains;

#if VEGETATION_STUDIO_PRO
        private SerializedProperty refreshVegetationOnPaint;
#endif
#if __MICROSPLAT__
        private SerializedProperty msTexArray;
        private Editor msTexArrayEditor;
        private bool requiresConfigRebuild;
#endif
        private bool requiresRepaint;
        private bool heatmapEnabled
        {
            get => SessionState.GetBool("PTP_HEATMAP", false);
            set => SessionState.SetBool("PTP_HEATMAP", value);
        }
        private bool visualizeContour;
        private bool visualizeTiling;

        private Editor layerEditor;
        private bool editLayerSettings;
        private AnimBool editLayerSettingsAnim;

        private RenderTexture[] heatmaps;
        
        private Vector2 scrollview;

        private int selectedLayerID
        {
            get { return SessionState.GetInt("PTP_SELECTED_LAYER", -1); }
            set { SessionState.SetInt("PTP_SELECTED_LAYER", value);}
        }
        private int selectedModifierIndex;
        ReorderableList m_LayerList;

        private enum Tab
        {
            Layers,
            Settings
        }
        
        private static Tab CurrentTab
        {
            get { return (Tab)SessionState.GetInt("PTP_TAB", 0); }
            set { SessionState.SetInt("PTP_TAB", (int)value); }
        }

        TerrainLayer m_PickedLayer;
        Texture2D m_PickedTexture;
        Texture2D m_layerTexture;
        
        int m_layerPickerWindowID = -1;
        int m_texturePickerWindowID = -1;

        // layer list view
        const int kElementHeight = 40;
        const int kElementObjectFieldHeight = 16;
        const int kElementPadding = 2;
        const int kElementObjectFieldWidth = 140;
        const int kElementToggleWidth = 20;
        const int kElementThumbSize = 40;

        private string iconPrefix => EditorGUIUtility.isProSkin ? "d_" : "";
        
        private void OnEnable()
        {
            script = (TerrainPainter) target;
            TerrainPainter.Current = script;
            
            if(script.terrains != null) script.RecalculateBounds();

            terrains = serializedObject.FindProperty("terrains");
            autoRepaint = serializedObject.FindProperty("autoRepaint");
            layerSettings = serializedObject.FindProperty("layerSettings");
            resolution = serializedObject.FindProperty("splatmapResolution");
            colorMapResolution = serializedObject.FindProperty("colorMapResolution");
#if VEGETATION_STUDIO_PRO
            refreshVegetationOnPaint = serializedObject.FindProperty("refreshVegetationOnPaint");
#endif
#if __MICROSPLAT__
            msTexArray = serializedObject.FindProperty("msTexArray");
#endif
            ModifierEditor.RefreshModifiers();

            RefreshLayerList();
            RefreshModifierLists();

            editLayerSettingsAnim = new AnimBool(editLayerSettings);
            editLayerSettingsAnim.valueChanged.AddListener(this.Repaint);
            editLayerSettingsAnim.speed = 4f;

            if(script.terrains != null) hasMissingTerrains = Utilities.HasMissingTerrain(script.terrains);

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneRepaint;
#else
            SceneView.onSceneGUIDelegate += OnSceneRepaint;
#endif
        }

        private void RefreshLayerList()
        {
            m_LayerList = null;
            
            if (m_LayerList == null)
            {
                m_LayerList = new ReorderableList(script.layerSettings, typeof(LayerSettings), true,
                    false, false, false);
                m_LayerList.elementHeight = kElementHeight;
                //m_LayerList.drawHeaderCallback = DrawHeader;
                m_LayerList.drawElementCallback = DrawLayerElement;
                m_LayerList.onSelectCallback = OnSelectLayerElement;
                m_LayerList.drawElementBackgroundCallback = DrawLayerBackground;
                m_LayerList.onReorderCallbackWithDetails = OnReorderLayerElement;
                //m_LayerList.onAddDropdownCallback = OnLayerAddButton;
                //m_LayerList.onRemoveCallback = OnLayerRemoveButton;
                    
                m_LayerList.headerHeight = 0f;
                m_LayerList.footerHeight = 0f;

                m_LayerList.index = selectedLayerID;
            }
                
            m_LayerList.showDefaultBackground = false;
        }

        private void RefreshModifierLists()
        {
            m_modifierList.Clear();
            
            foreach (LayerSettings s in script.layerSettings)
            {
                ReorderableList layerModifiers = new ReorderableList(s.modifierStack, typeof(Modifier));
                layerModifiers.draggable = true;
                layerModifiers.elementHeight = 25;
                layerModifiers.drawHeaderCallback = DrawModifierHeader;
                layerModifiers.displayAdd = true;
                layerModifiers.displayRemove = true;
                layerModifiers.drawElementCallback = DrawModifierElement;
                layerModifiers.onSelectCallback = OnSelectModifier;
                layerModifiers.drawElementBackgroundCallback = DrawModifierBackground;
                layerModifiers.onRemoveCallback = OnRemoveModifier;
                layerModifiers.onReorderCallbackWithDetails = OnReorderModifier;
                layerModifiers.onAddDropdownCallback = OnAddModifierDropDown;
                m_modifierList.Add(s, layerModifiers);
            }
        }

        private void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneRepaint;
#else
            SceneView.onSceneGUIDelegate -= OnSceneRepaint;
#endif
            
            //This function copies the GPU splatmaps to the terrain data's internal float arrays.
            //It is normally called when saving the project. But there's the risk of the terrains in the queue no longer being present in the scene, thus throwing null-refs.
            PaintContext.ApplyDelayedActions();
            
            script.Dispose();
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("Version " + TerrainPainter.Version, EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5f);
            
            //Terrains not yet assigned? Force settings tab
            if (terrains.arraySize > 0 && !hasMissingTerrains)
            {
                CurrentTab = (Tab)GUILayout.Toolbar((int)CurrentTab, new GUIContent[]
                {
#if UNITY_2019_3_OR_NEWER
                    new GUIContent("Layers", EditorGUIUtility.IconContent(iconPrefix + "Terrain Icon").image),
#else
                    //Old UI does not have a dark skin version for the terrain icon
                    new GUIContent("Layers", EditorGUIUtility.IconContent("Terrain Icon").image),
#endif
                    new GUIContent("Settings", EditorGUIUtility.IconContent(iconPrefix + "SettingsIcon").image)
                }, GUILayout.Height(30f));

                instancedTerrains = script.terrains[0] && script.terrains[0].drawInstanced;
            }
            else
            {
                CurrentTab = Tab.Settings;
            }

            //Default
            requiresRepaint = false;
            #if __MICROSPLAT__
            requiresConfigRebuild = false;
            #endif
            
            if(hasMissingTerrains) EditorGUILayout.HelpBox("One or more terrains are missing", MessageType.Error);
            
            serializedObject.Update();
            
            EditorGUI.BeginChangeCheck();

            switch (CurrentTab)
            {
                case Tab.Layers:
                    DrawLayers();
                    break;
                case Tab.Settings:
                    DrawSettings();
                    break;
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
            
            #if __MICROSPLAT__
            if(requiresConfigRebuild && script.msTexArray) TextureArrayConfigEditor.CompileConfig(script.msTexArray);
            #endif

            EditorGUILayout.Space();
            
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();   
                if(GUILayout.Button(new GUIContent(" Force Repaint ", "Trigger a complete repaint operation." +
                                                                      "\n\nTypically needed if the terrain was modified externally in some way, yet no changes were made in the Terrain Painter component")))
                {
                    requiresRepaint = true;
                    EditorApplication.Beep();
                }
                GUILayout.FlexibleSpace();
            }
            
            if (requiresRepaint)
            {
                script.RepaintAll();
                UpdateHeatmap();
            }
            
            EditorGUILayout.LabelField("- Staggart Creations -", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawSettings()
        {
            if (terrains.arraySize == 0 || hasMissingTerrains)
            {
                terrains.isExpanded = true;
            }

            using (new EditorGUILayout.VerticalScope("Box"))
            {
                serializedObject.Update();
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(terrains, new GUIContent("Terrains (" + terrains.arraySize + ")"));
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
                    
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Assign active terrains"))
                    {
                        //Important! Don't repaint if the terrains are new, current splatmaps would be wiped without user warning!
                        if(terrains.arraySize > 0) requiresRepaint = true;
                        
                        script.SetTargetTerrains(Terrain.activeTerrains);

                        hasMissingTerrains = false;
                        
                        EditorUtility.SetDirty(target);
                    }
                    EditorGUI.BeginDisabledGroup(script.transform.childCount == 0);
                    if (GUILayout.Button("Assign child terrains"))
                    {
                        //Important! Don't repaint if the terrains are new, current splatmaps would be wiped without user warning!
                        if(terrains.arraySize > 0) requiresRepaint = true;
                        
                        script.SetTargetTerrains(script.GetComponentsInChildren<Terrain>());

                        hasMissingTerrains = false;
                        
                        EditorUtility.SetDirty(target);
                        
                    }
                    EditorGUI.EndDisabledGroup();

                    if (terrains.arraySize > 0)
                    {
                        if (GUILayout.Button("Clear"))
                        {
                            script.terrains = new Terrain[0];

                            hasMissingTerrains = false;
                            
                            EditorUtility.SetDirty(target);
                        }
                    }
                }
            }
            
            if (terrains.arraySize == 0)
            {
                EditorGUILayout.HelpBox("Assign terrains to paint on first", MessageType.Info);
            }
            else
            {
                EditorGUILayout.Space();
                
                serializedObject.Update();

                EditorGUI.BeginChangeCheck();
                
                EditorGUILayout.PropertyField(autoRepaint);
                if (EditorGUI.EndChangeCheck())
                {
                    serializedObject.ApplyModifiedProperties();
                    script.SetAutoRepaint(autoRepaint.boolValue);
                }
                
                EditorGUI.BeginChangeCheck();
                
                EditorGUILayout.PropertyField(resolution);
                EditorGUILayout.PropertyField(colorMapResolution);
                if (EditorGUI.EndChangeCheck())
                {
                    requiresRepaint = true;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PrefixLabel("Bounds");

                    if (GUILayout.Button(new GUIContent("Recalculate bounds",
                            "If the terrain size has changed, the bounds must be recalculated. The white box must encapsulate all terrains"),
                        GUILayout.MaxWidth(150f)))
                    {
                        script.RecalculateBounds();
                        requiresRepaint = true;
                        EditorUtility.SetDirty(target);
                    }
                }
                
                EditorGUILayout.Space();
                
#if VEGETATION_STUDIO_PRO
                EditorGUILayout.LabelField("Vegetation Studio Pro", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(refreshVegetationOnPaint);
                EditorGUILayout.Space();
#endif
            }
        }

        private void OnSceneRepaint(SceneView sceneView)
        {
            if (layerSettings.arraySize != 0 && terrains.arraySize != 0) //Not yet initialized OnEnable
            {
                if (heatmapEnabled && selectedLayerID >= 0)
                {
                    if (heatmaps == null || script.terrains.Length != heatmaps.Length) UpdateHeatmap();

                    for (int i = 0; i < script.terrains.Length; i++)
                    {
                        HeatmapPreview.Draw(script.terrains[i], script.layerSettings[m_LayerList.index].layer, heatmaps[i], visualizeContour, visualizeTiling);
                    }
                }
            }

            if (!heatmapEnabled && heatmaps != null)
            {
                for (int i = 0; i < heatmaps.Length; i++)
                {
                    DestroyImmediate(heatmaps[i]);
                }

                heatmaps = null;
            }

            if (CurrentTab == Tab.Settings)
            {
                Handles.DrawWireCube(script.bounds.center, script.bounds.size);
            }
        }

        private void UpdateHeatmap()
        {
            if(heatmapEnabled) HeatmapPreview.CreateHeatmaps(script.terrains, (m_LayerList.count-1) - selectedLayerID, ref heatmaps);
        }
        
        #region Layers
        private float previewScaleMultiplier 
        {
            get => EditorPrefs.GetFloat("PTP_UI_LAYER_COUNT", 4f);
            set => EditorPrefs.SetFloat("PTP_UI_LAYER_COUNT", value);
        }
        private Rect sliderRect;
        
        private void DrawLayers()
        {
#if __MICROSPLAT__
            DrawMicroSplatField();
#else
            EditorGUILayout.Space();
#endif

            sliderRect = EditorGUILayout.GetControlRect();
            sliderRect.x += (EditorGUIUtility.currentViewWidth * 0.725f);
            sliderRect.width *= 0.2f;
            
            previewScaleMultiplier = GUI.HorizontalSlider(sliderRect, previewScaleMultiplier, 4f, 16);
            
            scrollview = EditorGUILayout.BeginScrollView(scrollview, EditorStyles.textArea, GUILayout.Height((kElementHeight * previewScaleMultiplier) + 20f));

            m_LayerList.DoLayoutList();
            
            EditorGUILayout.EndScrollView();

            // Control buttons
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                EditorGUI.BeginDisabledGroup(m_LayerList.index < 0 || m_LayerList.count == 0);
                {
                    heatmapEnabled = GUILayout.Toggle(heatmapEnabled, new GUIContent("  Heatmap", EditorGUIUtility.IconContent(iconPrefix + "P4_Local").image), EditorStyles.toolbarButton, GUILayout.MaxWidth(110f));
                    if (heatmapEnabled)
                    {
                        EditorGUILayout.LabelField("Countour", GUILayout.MaxWidth(60f));
                        visualizeContour = EditorGUILayout.Toggle(visualizeContour, GUILayout.MaxWidth(30f));
                        EditorGUILayout.LabelField("Tiling", GUILayout.MaxWidth(35f));
                        visualizeTiling = EditorGUILayout.Toggle(visualizeTiling, GUILayout.MaxWidth(30f));
                    }
                        
                    editLayerSettings = GUILayout.Toggle(editLayerSettings, new GUIContent("  Edit layer", EditorGUIUtility.IconContent(iconPrefix + "editicon.sml").image), EditorStyles.toolbarButton, GUILayout.MaxWidth(90f));
                }
                EditorGUI.EndDisabledGroup();

                
                GUILayout.FlexibleSpace();

                EditorGUI.BeginDisabledGroup(layerSettings.arraySize >= 32); //Maximum realistic number of terrain layers
#if UNITY_2019_3_OR_NEWER
                var newIcon = EditorGUIUtility.IconContent(iconPrefix + "DefaultAsset Icon").image;
#else
                var newIcon = EditorGUIUtility.IconContent("DefaultAsset Icon").image;
#endif
                if (GUILayout.Button(new GUIContent("", newIcon, "New terrain layer from texture"), EditorStyles.toolbarButton, GUILayout.MaxWidth(32f)))
                {
                    m_texturePickerWindowID = EditorGUIUtility.GetControlID(FocusType.Passive) + 201; 
                    EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, "", m_texturePickerWindowID);
                }
                
                if (GUILayout.Button(new GUIContent("",
                    EditorGUIUtility.IconContent(iconPrefix + "Toolbar Plus More")
                        .image, "Add terrain layer from project"), EditorStyles.toolbarButton))
                {
                    m_layerPickerWindowID = EditorGUIUtility.GetControlID(FocusType.Passive) + 200; 
                    EditorGUIUtility.ShowObjectPicker<TerrainLayer>(null, false, "", m_layerPickerWindowID);
                }

                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginDisabledGroup(m_LayerList.index < 0 || m_LayerList.count == 0);
                if (GUILayout.Button(new GUIContent("", EditorGUIUtility.IconContent(iconPrefix + "TreeEditor.Trash").image,
                    "Remove selected layer"), EditorStyles.toolbarButton))
                {
                    if (!EditorUtility.DisplayDialog("Terrain Painter",
                        "Removing a layer cannot be undone, settings will be lost",
                        "Ok","Cancel")) return;
                    
                    RemoveLayerElement(m_LayerList.index);
                }
                EditorGUI.EndDisabledGroup();
            }
            
            if (script.layerSettings.ElementAtOrDefault(selectedLayerID) != null)
            {
                editLayerSettingsAnim.target = editLayerSettings;
                //TODO: Fix error about mismatching layout
                if (EditorGUILayout.BeginFadeGroup(editLayerSettingsAnim.faded))
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Layer settings", EditorStyles.boldLabel);

#if UNITY_2019_2_OR_NEWER
                    Editor.CreateCachedEditor(script.layerSettings.ElementAtOrDefault(selectedLayerID).layer, typeof(TerrainLayerInspector), ref layerEditor);
#else
                    //TerrainLayerInspector is internal
                    layerEditor = Editor.CreateEditor(script.layerSettings.ElementAtOrDefault(selectedLayerID).layer);
#endif
                    layerEditor.OnInspectorGUI();
                }
                EditorGUILayout.EndFadeGroup();
            }

            if (m_LayerList.count == 0)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("All existing terrain layers and painting will be cleared when adding the first layer!", MessageType.Warning);
            }

            ObjectPickerActions();
            
            GUILayout.Space(17f);
            
            DrawLayerModifierStack();
        }

        private void DrawMicroSplatField()
        {
#if __MICROSPLAT__
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(msTexArray, new GUIContent("MicroSplat texture array", msTexArray.tooltip));
                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();

                EditorGUI.BeginDisabledGroup(msTexArray.objectReferenceValue == null);
                if (GUILayout.Button(new GUIContent("Copy to", "Copy all the terrain layers to the MicroSplat array." +
                                                               "\n\nThis also removes any textures from the array that aren't configured here" +
                                                               "\n\nNote that additional maps (AO, Height, smoothness) aren't taken into account because regular terrain layers don't have them"), EditorStyles.miniButton, GUILayout.MaxWidth(70f)))
                {
                    MicroSplatCopyLayersToArray();
                }
                EditorGUI.EndDisabledGroup();
            }

            if (msTexArray.objectReferenceValue && script.msTexArray.sourceTextures.Count != script.layerSettings.Count)
            {
                EditorGUILayout.HelpBox("The amount of textures in the array, and layers configured here doesn't match up.\n\nYou can choose to copy all the current layers into the array", MessageType.Error);
                if (GUILayout.Button("Apply layers to array"))
                {
                    MicroSplatCopyLayersToArray();
                }
            }
#endif
        }
        
        private void ObjectPickerActions()
        {
            // Add existing layer
            if (Event.current.commandName == "ObjectSelectorClosed" &&
                EditorGUIUtility.GetObjectPickerControlID() == m_layerPickerWindowID)
            {
                m_PickedLayer = (TerrainLayer) EditorGUIUtility.GetObjectPickerObject();
                m_layerPickerWindowID = -1;

                if (m_PickedLayer)
                {

                    var exists = false;

                    foreach (LayerSettings s in script.layerSettings)
                    {
                        if (s.layer == m_PickedLayer) exists = true;
                    }

                    if (exists)
                    {
                        EditorUtility.DisplayDialog("Terrain Painter", "Terrain layer already exists", "Ok");
                        return;
                    }
                }
                
                script.CreateSettingsForLayer(m_PickedLayer);
                
                MicroSplatAdd(m_PickedLayer);
                
                RefreshLayerList();
                RefreshModifierLists();
                
                //Auto-select new layer
                m_LayerList.index = 0;
                m_LayerList.onSelectCallback.Invoke(m_LayerList);
                scrollview.y = 0;
                
                EditorUtility.SetDirty(target);

                requiresRepaint = true;
            }
            
            // New layer creation
            if (Event.current.commandName == "ObjectSelectorClosed" &&
                EditorGUIUtility.GetObjectPickerControlID() == m_texturePickerWindowID)
            {
                m_PickedTexture = (Texture2D) EditorGUIUtility.GetObjectPickerObject();
                m_texturePickerWindowID = -1;

                if (m_PickedTexture == null) return;

                TerrainLayer newLayer = CreateLayerFromTexture(m_PickedTexture);

                if (newLayer == null) return;
                
                script.CreateSettingsForLayer(newLayer);

                MicroSplatAdd(newLayer);
                
                RefreshLayerList();
                RefreshModifierLists();

                //Auto-select new layer
                m_LayerList.index = 0;
                m_LayerList.onSelectCallback.Invoke(m_LayerList);
                scrollview.y = 0;
                
                EditorUtility.SetDirty(target);

                requiresRepaint = true;
            }
        }

        private void MicroSplatCopyLayersToArray()
        {
#if __MICROSPLAT__
            if (script.msTexArray)
            {
                script.msTexArray.sourceTextures.Clear();
                
                //For safety, may have switched scene with an entirely different TerrainPainter component
                script.SetTerrainLayers();

                TextureArrayConfigEditor.GetFromTerrain(script.msTexArray, script.terrains[0]);

                requiresConfigRebuild = true;
            }
#endif
        }
        
        private void MicroSplatAdd(TerrainLayer newLayer)
        {
#if __MICROSPLAT__
            if (script.msTexArray)
            {
                var entry = new TextureArrayConfig.TextureEntry(); 
                if (script.msTexArray.sourceTextures.Count > 0)
                {
                    entry.aoChannel = script.msTexArray.sourceTextures[0].aoChannel;
                    entry.heightChannel = script.msTexArray.sourceTextures[0].heightChannel;
                    entry.smoothnessChannel = script.msTexArray.sourceTextures[0].smoothnessChannel;
                }
                else
                {
                    entry.aoChannel = TextureArrayConfig.TextureChannel.G;
                    entry.heightChannel = TextureArrayConfig.TextureChannel.G;
                    entry.smoothnessChannel = TextureArrayConfig.TextureChannel.G;
                }

                entry.diffuse = newLayer.diffuseTexture;
                //Layer will be inserted at index 0, so for the "real" terrain layers, append to end
                script.msTexArray.sourceTextures.Add(entry);

                requiresConfigRebuild = true;
            }
#endif
        }

        private TerrainLayer CreateLayerFromTexture(Texture2D tex)
        {
            TerrainLayer newLayer = new TerrainLayer();
            newLayer.diffuseTexture = m_PickedTexture;
            newLayer.name = newLayer.diffuseTexture.name;
                
            string assetPath =string.Empty;
            assetPath = EditorUtility.SaveFilePanel("Asset destination folder", "Assets/", "New Layer", "asset");
            if (assetPath.Length == 0) return null;

            //Relative path in project
            assetPath = assetPath.Substring(assetPath.IndexOf("Assets/"));
                
            AssetDatabase.CreateAsset(newLayer, assetPath);
                
            newLayer = (TerrainLayer)AssetDatabase.LoadAssetAtPath(assetPath, typeof(TerrainLayer));

            return newLayer;
        }
        
        private void DrawLayerBackground(Rect rect, int index, bool isactive, bool selected)
        {
            var prevColor = GUI.color;
            var prevBgColor = GUI.backgroundColor;

            GUI.color = index % 2 == 0
                ? Color.grey * (EditorGUIUtility.isProSkin ? 1f : 1.7f)
                : Color.grey * (EditorGUIUtility.isProSkin ? 1.05f : 1.66f);

            if (m_LayerList.index == index) GUI.color = EditorGUIUtility.isProSkin ? Color.grey * 1.1f : Color.grey * 1.5f;

            //Selection outline
            if (m_LayerList.index == index)
            {
                Rect outline = rect;
                EditorGUI.DrawRect(outline, EditorGUIUtility.isProSkin ? Color.gray * 1.5f : Color.gray);

                rect.x += 1;
                rect.y += 1;
                rect.width -= 2;
                rect.height -= 2;
            }

            EditorGUI.DrawRect(rect, GUI.color);

            GUI.color = prevColor;
            GUI.backgroundColor = prevBgColor;
        }

        void DrawLayerElement(Rect rect, int index, bool selected, bool focused)
        {
            rect.y = rect.y + kElementPadding;
            var rectButton = new Rect((rect.x + kElementPadding), rect.y + (kElementHeight / 4), kElementToggleWidth,
                kElementToggleWidth);
            var rectImage = new Rect((rectButton.x + kElementToggleWidth) + 5f, rect.y, kElementThumbSize, kElementThumbSize);
            var rectObject = new Rect((rectImage.x + kElementThumbSize + 10), rect.y + (kElementHeight / 4),
                kElementObjectFieldWidth, kElementObjectFieldHeight);
            
            if (script.layerSettings.Count > 0 && script.layerSettings.ElementAtOrDefault(index) != null)
            {
                if (index < layerSettings.arraySize-1)
                {
#if UNITY_2019_1_OR_NEWER
                    EditorGUI.Toggle(rectButton, new GUIContent(EditorGUIUtility.IconContent(script.layerSettings[index].enabled ? iconPrefix + "scenevis_visible_hover" : iconPrefix + "scenevis_hidden_hover").image), script.layerSettings[index].enabled, GUIStyle.none);
                    if (rectButton.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown &&
                        Event.current.button == 0)
                    {
                        script.layerSettings[index].enabled = !script.layerSettings[index].enabled;

                        requiresRepaint = true;
                    }

#else
                    EditorGUI.BeginChangeCheck();
                    script.layerSettings[index].enabled = EditorGUI.Toggle(rectButton, script.layerSettings[index].enabled);
                    if (EditorGUI.EndChangeCheck()) requiresRepaint = true;
#endif
                }
                else
                {
                    //Base layer is always enabled
                    script.layerSettings[index].enabled = true;
                }
                
                Texture2D icon = null;
                if (script.layerSettings[index].layer != null)
                {
                    icon = AssetPreview.GetAssetPreview(script.layerSettings[index].layer.diffuseTexture);
                }
                GUI.Box(rectImage, icon);
                
                EditorGUI.BeginChangeCheck();
                script.layerSettings[index].layer = EditorGUI.ObjectField(rectObject, script.layerSettings[index].layer, typeof(TerrainLayer), false) as TerrainLayer;
                if (EditorGUI.EndChangeCheck())
                {
                    OnChangeLayer(script.layerSettings[index].layer, index);
                }
            }
        }

        void OnSelectLayerElement(ReorderableList list)
        {
            selectedLayerID = list.index;
            
            LayerSettings settings = script.layerSettings.ElementAtOrDefault(selectedLayerID);
            m_modifierList.TryGetValue(settings, out curModList);

            SelectModifier(curModList, 0);
            
            //Refresh for current layer
            UpdateHeatmap();
        }

        void OnChangeLayer(TerrainLayer terrainLayer, int index)
        {
            requiresRepaint = true;

            script.SetTerrainLayers();
            RefreshLayerList();
            
#if __MICROSPLAT__
            if (script.msTexArray)
            {
                script.msTexArray.sourceTextures[index].diffuse = terrainLayer.diffuseTexture;
                script.msTexArray.sourceTextures[index].normal = terrainLayer.normalMapTexture;
                
                script.msTexArray.sourceTextures2[index].diffuse = terrainLayer.diffuseTexture;
                script.msTexArray.sourceTextures2[index].normal = terrainLayer.normalMapTexture;
                
                script.msTexArray.sourceTextures3[index].diffuse = terrainLayer.diffuseTexture;
                script.msTexArray.sourceTextures3[index].normal = terrainLayer.normalMapTexture;

                requiresConfigRebuild = true;
                //other maps (AO, height, smoothness) aren't changed, touch luck
            }
#endif
        }

        void DrawLayerModifierStack()
        {
            if (layerSettings.arraySize == 0) return;
            
            if (selectedLayerID == layerSettings.arraySize -1)
            {
                EditorGUILayout.HelpBox("Base layer has no configurable options, it fills the entire terrain." + (layerSettings.arraySize == 1 ? " \n\nAdd an additional terrain layer" : ""), MessageType.Info);
                return;
            }

            if (script.layerSettings.ElementAtOrDefault(selectedLayerID) == null)
            {
                EditorGUILayout.HelpBox("Select a layer to modify its spawn rules", MessageType.Info);
                return;
            }
            
            LayerSettings settings = script.layerSettings.ElementAtOrDefault(selectedLayerID);
            m_modifierList.TryGetValue(settings, out curModList);
            
            //Draw all modifierStack for the current layer
            using (new EditorGUI.DisabledGroupScope(settings.enabled == false))
            {
                if (curModList != null)
                {
                    curModList.DoLayoutList();
                    
                    if(curModList.index < 0 && curModList.count > 0) EditorGUILayout.HelpBox("Select a modifier from the stack to edit its settings", MessageType.Info);
                    if(curModList.count == 0) EditorGUILayout.HelpBox("Add a modifier to create painting rules", MessageType.Info);
                    
                    DrawModifierSettings(curModList.index);
                }
            }
            
        }

        void OnReorderLayerElement(ReorderableList list, int oldIndex, int newIndex)
        {
            script.SetTerrainLayers();
            RefreshLayerList();
            
            requiresRepaint = true;

            UpdateHeatmap();
            
            #if __MICROSPLAT__
            if (script.msTexArray)
            {
                MethodInfo SwapEntryInfo = typeof(TextureArrayConfigEditor).GetMethod("SwapEntry", BindingFlags.Instance | BindingFlags.NonPublic);
                Editor.CreateCachedEditor(script.msTexArray, typeof(TextureArrayConfigEditor), ref msTexArrayEditor);
                //Because layers are from bottom to top, reverse indices
                SwapEntryInfo.Invoke(msTexArrayEditor, new object[] { script.msTexArray, (list.count-1) - oldIndex, (list.count-1) - newIndex });

                requiresConfigRebuild = true;
            }
            #endif
        }
        
        void RemoveLayerElement(int index)
        {
            if (script.layerSettings.ElementAtOrDefault(index) == null)
            {
                return;
            }

            script.layerSettings.RemoveAt(index);

            script.SetTerrainLayers();
            RefreshLayerList();
            
            EditorUtility.SetDirty(target);

            requiresRepaint = true;
            
            UpdateHeatmap();
            
#if __MICROSPLAT__
            if (script.msTexArray)
            {
                MethodInfo RemoveInfo = typeof(TextureArrayConfigEditor).GetMethod("Remove", BindingFlags.Instance | BindingFlags.NonPublic);
                Editor.CreateCachedEditor(script.msTexArray, typeof(TextureArrayConfigEditor), ref msTexArrayEditor);
                RemoveInfo.Invoke(msTexArrayEditor, new object[] { script.msTexArray, (script.layerSettings.Count) - index});

                requiresConfigRebuild = true;
            }
#endif
        }
        #endregion
        
        #region Modifiers

        private void SelectModifier(ReorderableList list, int index)
        {
            selectedModifierIndex = index;
            list.index = index;
            list.onSelectCallback.Invoke(curModList);
            //list.GrabKeyboardFocus();
        }
        private void OnRemoveModifier(ReorderableList list)
        {
            if (!EditorUtility.DisplayDialog("Remove modifier", "This operation cannot be undone, settings will be lost",
                "Ok", "Cancel")) return;
            
            //get the related layer
            LayerSettings layer = script.layerSettings.ElementAtOrDefault(selectedLayerID);
            
            layer.modifierStack.RemoveAt(list.index);
            RefreshModifierLists();
            
            EditorUtility.SetDirty(target);

            requiresRepaint = true;
        }

        private void DrawModifierHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Modifier stack");
        }

        private void OnAddModifierDropDown(Rect buttonrect, ReorderableList list)
        {
            List<Modifier> currentModifierList = script.layerSettings.ElementAtOrDefault(selectedLayerID).modifierStack;
            
            GenericMenu menu = new GenericMenu();

            foreach (string item in ModifierEditor.ModifierNames)
            {
                menu.AddItem(new GUIContent(item), false, () => AddModifier(currentModifierList, list, item));
            }
                        
            menu.ShowAsContext();
        }
        
        private void AddModifier(List<Modifier> currentModifierList, ReorderableList list, string typeName)
        {
            Type type = ModifierEditor.GetType(typeName);
            
            Modifier m = (Modifier)CreateInstance(type);
            m.label = typeName;
            currentModifierList.Insert(0, m);
            
            RefreshModifierLists();
            
            LayerSettings settings = script.layerSettings.ElementAtOrDefault(selectedLayerID);
            m_modifierList.TryGetValue(settings, out curModList);
            //Auto select new
            SelectModifier(curModList, 0);

            requiresRepaint = true;

            EditorUtility.SetDirty(target);
        }
        

        void OnSelectModifier(ReorderableList list)
        {
            selectedModifierIndex = list.index;
        }

        private void DrawModifierBackground(Rect rect, int index, bool isactive, bool isfocused)
        {
            var prevColor = GUI.color;
            var prevBgColor = GUI.backgroundColor;

            GUI.color = index % 2 == 0
                ? Color.grey * (EditorGUIUtility.isProSkin ? 1f : 1.7f)
                : Color.grey * (EditorGUIUtility.isProSkin ? 1.05f : 1.66f);

            
            //Selection outline (note: can't rely on isfocused. Focus and selection aren't the same thing)
            if (index == selectedModifierIndex)
            {
                GUI.color = EditorGUIUtility.isProSkin ? Color.grey * 1.1f : Color.grey * 1.5f;
                Rect outline = rect;
                EditorGUI.DrawRect(outline, EditorGUIUtility.isProSkin ? Color.gray * 1.5f : Color.gray);

                rect.x += 1;
                rect.y += 1;
                rect.width -= 2;
                rect.height -= 2;
            }
            

            EditorGUI.DrawRect(rect, GUI.color);

            GUI.color = prevColor;
            GUI.backgroundColor = prevBgColor;
        }

        private void DrawModifierElement(Rect rect, int index, bool isactive, bool isfocused)
        {
            //Get modifierStack for current layer
            List<Modifier> currentModifierList = script.layerSettings.ElementAtOrDefault(m_LayerList.index).modifierStack;
            
            if (currentModifierList.ElementAtOrDefault(index) == null)
            {
                EditorGUILayout.LabelField("NULL!");
                return;
            }
            
            Modifier m = currentModifierList[index];
            
            rect.y = rect.y;
            var rectButton = new Rect(10 + (rect.x + kElementPadding), rect.y + kElementPadding, kElementToggleWidth,
                kElementToggleWidth);
            var labelRect = new Rect(rect.x + rectButton.x - 10, rect.y+kElementPadding, 120, 17);
            var blendModeRect = new Rect((labelRect.x + 120 + 10), rect.y+ kElementPadding, 80, 27);
            var opacityRect = new Rect(blendModeRect.x + blendModeRect.width + kElementPadding + 10, rect.y+ kElementPadding, 0f, 17);
            opacityRect.width = EditorGUIUtility.currentViewWidth - opacityRect.x - 30f;
            
            m.label = EditorGUI.TextField(labelRect, m.label);
            
#if UNITY_2019_1_OR_NEWER
            EditorGUI.Toggle(rectButton, new GUIContent(EditorGUIUtility.IconContent(m.enabled ? iconPrefix +  "scenevis_visible_hover" : iconPrefix + "scenevis_hidden_hover").image, "Toggle visibility"), m.enabled, GUIStyle.none);
            if (rectButton.Contains(Event.current.mousePosition) && Event.current.type == EventType.MouseDown &&
                Event.current.button == 0)
            {
                m.enabled = !m.enabled;

                requiresRepaint = true;
            }
#else
            EditorGUI.BeginChangeCheck();
            m.enabled = EditorGUI.Toggle(rectButton, m.enabled);
            if (EditorGUI.EndChangeCheck())
            {
                requiresRepaint = true;
            }
#endif
            
            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            
            m.blendMode =  (Modifier.BlendMode) EditorGUI.Popup(blendModeRect, (int)m.blendMode, Enum.GetNames((typeof(Modifier.BlendMode))));
            m.opacity = EditorGUI.Slider(opacityRect, m.opacity, 0f, 100f);

            if (EditorGUI.EndChangeCheck())
            {
                requiresRepaint = true;
            }
        }

        private void DrawModifierSettings(int index)
        {
            //None selected
            if (index < 0) return;
            
            serializedObject.Update();
            
            SerializedProperty settingsElement = serializedObject.FindProperty("layerSettings").GetArrayElementAtIndex(m_LayerList.index);
            SerializedProperty modifiersElement = settingsElement.FindPropertyRelative("modifierStack");
            if (index >= modifiersElement.arraySize) return;
            SerializedProperty modifierProp = modifiersElement.GetArrayElementAtIndex(index);
            
            //Can't draw the properties of the serializedproperty itself
            var editor = Editor.CreateEditor(modifierProp.objectReferenceValue);
            
            if (instancedTerrains == false && (modifierProp.objectReferenceValue.GetType() == typeof(Curvature) || modifierProp.objectReferenceValue.GetType() == typeof(Direction)))
            {
                EditorGUILayout.HelpBox("\nInstanced rendering is disabled on the terrain(s)." +
                                        "\n\nThis modifier will have no effect\n", MessageType.Error);
                
                GUILayout.Space(-48);
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button(new GUIContent("Enable"), GUILayout.Width(60)))
                    {
                        foreach (Terrain terrain in script.terrains)
                        {
                            terrain.drawInstanced = true;
                            EditorUtility.SetDirty(terrain);
                        }

                        requiresRepaint = true;
                    }
                    GUILayout.Space(8);
                }
                GUILayout.Space(32);
            }
            
            EditorGUI.BeginChangeCheck();
            
            #if ODIN_INSPECTOR
            editor.DrawDefaultInspector();
            #else
            //If Odin is installed, this doesn't draw!
            editor.OnInspectorGUI();
            #endif

            if (EditorGUI.EndChangeCheck())
            {
                requiresRepaint = true;
            }
        }
        
        void OnReorderModifier(ReorderableList list, int oldIndex, int newIndex)
        {
            RefreshModifierLists();
            
            UpdateHeatmap();
            
            requiresRepaint = true;
        }
#endregion
    }
}