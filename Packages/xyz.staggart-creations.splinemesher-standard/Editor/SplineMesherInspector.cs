// Staggart Creations (http://staggart.xyz)
// Copyright protected under Unity Asset Store EULA
// Copying or referencing source code for the production of new asset store, or public content, is strictly prohibited!

using System;
using System.Collections.Generic;
using System.Reflection;
using sc.modeling.splines.runtime;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine;

#if SPLINES
using UnityEditor.Splines;
using UnityEngine.Splines;
#endif

namespace sc.modeling.splines.editor
{
    [CustomEditor(typeof(SplineMesher))]
    [CanEditMultipleObjects]
    public class SplineMesherInspector : Editor
    {
        private SerializedProperty splineContainer;
        private SerializedProperty splineChangeMode;
        
        private SerializedProperty sourceMesh;
        private SerializedProperty rotation;

        private SerializedProperty outputObject;
        private SerializedProperty rebuildTriggers;

        //Collider
        private SerializedProperty enableCollider;
        private SerializedProperty colliderOnly;
        private SerializedProperty colliderType;
        private SerializedProperty colliderBoxSubdivisions;
        private SerializedProperty collisionMesh;
        
        //Distribution
        private SerializedProperty segments;
        private SerializedProperty autoSegmentCount;
        
        private SerializedProperty stretchToFit;
        private SerializedProperty evenOnly;
        private SerializedProperty trimStart;
        private SerializedProperty trimEnd;
        private SerializedProperty spacing;
        
        //Deforming
        private SerializedProperty ignoreKnotRotation;
        private SerializedProperty curveOffset;
        private SerializedProperty pivotOffset;
        private SerializedProperty scale;
        private SerializedProperty scaleDataPathIndexUnit;
        private SerializedProperty scaleInterpolation;
        
        private SerializedProperty rollDataPathIndexUnit;
        private SerializedProperty rollMode;
        private SerializedProperty rollFrequency;
        private SerializedProperty rollAngle;
        
        private SerializedProperty uvScale;
        private SerializedProperty uvOffset;
        private SerializedProperty uvStretchMode;
        
        private SerializedProperty colorPathIndexUnit;

        //Conforming
        private SerializedProperty enableConforming;
        private SerializedProperty conformingPathIndexUnit;
        private SerializedProperty conformingDirection;
        private SerializedProperty seekDistance;
        private SerializedProperty terrainOnly;
        private SerializedProperty layerMask;
        private SerializedProperty align;
        private SerializedProperty blendNormal;

        private SerializedProperty meshSettings;
        
        //Caps
        private SerializedProperty startCap;
        private SerializedProperty endCap;
        
        private SerializedProperty onPreRebuild, onPostRebuild;

        //Change tracking
        private bool requiresRebuild = false;
        private bool requiresCapUpdate = false;
        [NonSerialized]
        private bool requiresLightmapUV;
        private bool meshIsReadable;
        private bool colliderIsReadable;
        private bool isPrefab;
        
        //Validation
        private bool outputHasMeshFilter;
        private bool outputHasMeshRenderer;
        
        private static bool ExpandIndexUnits
        {
            get => SessionState.GetBool("SM_EXPAND_INDEXUNITS", false);
            set => SessionState.SetBool("SM_EXPAND_INDEXUNITS", value);
        } 
        
        private static bool ExpandSetup
        {
            get => SessionState.GetBool("SM_EXPAND_SETUP", true);
            set => SessionState.SetBool("SM_EXPAND_SETUP", value);
        }
        private static bool PreviewMesh
        {
            get => SessionState.GetBool("SM_PREVIEW_MESH", false);
            set => SessionState.SetBool("SM_PREVIEW_MESH", value);
        }
        private static bool LabelCaps
        {
            get => SessionState.GetBool("SM_LABEL_CAPS", true);
            set => SessionState.SetBool("SM_LABEL_CAPS", value);
        }
        private MeshPreview sourceMeshPreview;
        private PreviewRenderUtility meshPreviewUtility;
        
        private UI.Section setupSection;
        private UI.Section colliderSection;
        private UI.Section distributionSection;
        private UI.Section deformingSection;
        private UI.Section conformingSection;
        private UI.Section capsSection;
        private UI.Section eventsSection;
        private List<UI.Section> sections = new List<UI.Section>();
        
        private void OnEnable()
        {
            sections = new List<UI.Section>();
            sections.Add(setupSection = new UI.Section(this, "SETUP", new GUIContent("Input/Output")));
            sections.Add(colliderSection = new UI.Section(this, "COLLIDER", new GUIContent("Collider")));
            sections.Add(distributionSection = new UI.Section(this, "DIST", new GUIContent("Distribution")));
            sections.Add(deformingSection = new UI.Section(this, "DEFORMING", new GUIContent("Deforming")));
            sections.Add(conformingSection = new UI.Section(this, "CONFORMING", new GUIContent("Conforming")));
            sections.Add(capsSection = new UI.Section(this, "CAPS", new GUIContent("Caps")));
            sections.Add(eventsSection = new UI.Section(this, "EVENTS", new GUIContent("Events")));
            
            #if SPLINES
            splineContainer = serializedObject.FindProperty("splineContainer");
            splineChangeMode = serializedObject.FindProperty("splineChangeMode");
            #endif
            
            sourceMesh = serializedObject.FindProperty("sourceMesh");
            rotation = serializedObject.FindProperty("rotation");
            
            outputObject = serializedObject.FindProperty("outputObject");
            rebuildTriggers = serializedObject.FindProperty("rebuildTriggers");

            SerializedProperty settings = serializedObject.FindProperty("settings");
            {
                SerializedProperty settingsCollision = settings.FindPropertyRelative("collision");
                enableCollider = settingsCollision.FindPropertyRelative("enable");
                colliderOnly = settingsCollision.FindPropertyRelative("colliderOnly");
                colliderType = settingsCollision.FindPropertyRelative("type");
                colliderBoxSubdivisions = settingsCollision.FindPropertyRelative("boxSubdivisions");
                collisionMesh = settingsCollision.FindPropertyRelative("collisionMesh");

                SerializedProperty settingsDistribution = settings.FindPropertyRelative("distribution");
                segments = settingsDistribution.FindPropertyRelative("segments");
                autoSegmentCount = settingsDistribution.FindPropertyRelative("autoSegmentCount");
                
                stretchToFit = settingsDistribution.FindPropertyRelative("stretchToFit");
                evenOnly = settingsDistribution.FindPropertyRelative("evenOnly");
                trimStart = settingsDistribution.FindPropertyRelative("trimStart");
                trimEnd = settingsDistribution.FindPropertyRelative("trimEnd");
                spacing = settingsDistribution.FindPropertyRelative("spacing");
                
                SerializedProperty settingsDeforming = settings.FindPropertyRelative("deforming");
                ignoreKnotRotation = settingsDeforming.FindPropertyRelative("ignoreKnotRotation");
                curveOffset = settingsDeforming.FindPropertyRelative("curveOffset");
                pivotOffset = settingsDeforming.FindPropertyRelative("pivotOffset");
                scale = settingsDeforming.FindPropertyRelative("scale");
                #if SPLINES
                scaleDataPathIndexUnit = settingsDeforming.FindPropertyRelative("scalePathIndexUnit");
                scaleInterpolation = settingsDeforming.FindPropertyRelative("scaleInterpolation");
                #endif
                
                rollMode = settingsDeforming.FindPropertyRelative("rollMode");
                rollFrequency = settingsDeforming.FindPropertyRelative("rollFrequency");
                rollAngle = settingsDeforming.FindPropertyRelative("rollAngle");
                #if SPLINES
                rollDataPathIndexUnit = settingsDeforming.FindPropertyRelative("pathIndexUnit");
                
                SerializedProperty settingColor = settings.FindPropertyRelative("color");
                colorPathIndexUnit = settingColor.FindPropertyRelative("pathIndexUnit");
                #endif
                
                SerializedProperty settingsUV = settings.FindPropertyRelative("uv");
                uvScale = settingsUV.FindPropertyRelative("scale");
                uvOffset = settingsUV.FindPropertyRelative("offset");
                uvStretchMode = settingsUV.FindPropertyRelative("stretchMode");

                SerializedProperty settingsConforming = settings.FindPropertyRelative("conforming");
                enableConforming = settingsConforming.FindPropertyRelative("enable");
                conformingPathIndexUnit = settingsConforming.FindPropertyRelative("pathIndexUnit");
                seekDistance = settingsConforming.FindPropertyRelative("seekDistance");
                conformingDirection = settingsConforming.FindPropertyRelative("direction");
                terrainOnly = settingsConforming.FindPropertyRelative("terrainOnly");
                layerMask = settingsConforming.FindPropertyRelative("layerMask");
                align = settingsConforming.FindPropertyRelative("align");
                blendNormal = settingsConforming.FindPropertyRelative("blendNormal");
                
                meshSettings = settings.FindPropertyRelative("mesh");
                meshSettings.isExpanded = false;
            }
            
            startCap = serializedObject.FindProperty("startCap");
            endCap = serializedObject.FindProperty("endCap");
            
            onPreRebuild = serializedObject.FindProperty("onPreRebuild");
            onPostRebuild = serializedObject.FindProperty("onPostRebuild");

            Undo.undoRedoPerformed += OnUndoRedo;
            
            sourceMeshPreview = new MeshPreview(new Mesh());

            isPrefab = PrefabUtility.IsPartOfPrefabInstance((SplineMesher)target) || PrefabStageUtility.GetCurrentPrefabStage();
            //may be too annoying, since a Spline Mesher prefab may serve as a settings container
            //isPrefab |= PrefabUtility.GetPrefabAssetType(target) != PrefabAssetType.NotAPrefab;

            CheckInputMeshReadability();
            CheckInputColliderReadability();
            
            //Override zoom level
            meshPreviewUtility = (PreviewRenderUtility)typeof(MeshPreview).GetField("m_PreviewUtility", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(sourceMeshPreview);
            meshPreviewUtility.camera.fieldOfView = 17;
            meshPreviewUtility.camera.backgroundColor = Color.white * 0.09f;

            SceneView.duringSceneGui += DuringSceneGUI;
        }

        private void CheckInputMeshReadability()
        {
            if (sourceMesh.objectReferenceValue) meshIsReadable = SplineMeshEditor.CheckInputMeshReadability(sourceMesh.objectReferenceValue as Mesh);
        }
        
        private void CheckInputColliderReadability()
        {
            if (colliderType.intValue == (int)Settings.ColliderType.Mesh)
            {
                if (collisionMesh.objectReferenceValue) colliderIsReadable = SplineMeshEditor.CheckInputMeshReadability(collisionMesh.objectReferenceValue as Mesh);
            }
        }
        
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= OnUndoRedo;
            
            if (sourceMeshPreview != null)
            {
                sourceMeshPreview.Dispose();
                sourceMeshPreview = null;
            }
            
            SceneView.duringSceneGui -= DuringSceneGUI;
        }

        private void OnUndoRedo()
        {
            Rebuild();
        }

        private static string iconPrefix => EditorGUIUtility.isProSkin ? "d_" : string.Empty;
        
        public override void OnInspectorGUI()
        {
            #if !SPLINES || !MATHEMATICS
            #if !SPLINES
            EditorGUILayout.HelpBox("The Spline package isn't installed, please install this through the Package Manager", MessageType.Error);
            #endif
            #if !MATHEMATICS
            EditorGUILayout.HelpBox("The Mathematics package isn't installed or outdated, please install this through the Package Manager", MessageType.Error);
            #endif
            
            return;
            #else
            //Reset
            requiresRebuild = false;
            requiresCapUpdate = false;
            requiresLightmapUV = SplineMeshEditor.RequiresLightmapUV(((SplineMesher)target));

            DrawHeaderSection();

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            DrawInputOutput();
            DrawCollider();
            DrawDistribution();
            DrawDeforming();
            DrawConforming();
            DrawCaps();
            DrawEvents();

            /*
            EditorGUILayout.Separator();
            
            EditorGUILayout.LabelField("FBX Export", EditorStyles.boldLabel);
            #if !FBX_EXPORTER
            EditorGUILayout.HelpBox("This functionality requires the FBX Exporter package to be installed", MessageType.Info);
            #else
            
            #endif
            */

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();

                SplineMesher component = (SplineMesher)target;
                if (requiresCapUpdate)
                {
                    foreach (var m_target in targets)
                    {
                        ((SplineMesher)m_target).UpdateCaps();
                    }
                }
                if (requiresRebuild)
                {
                    if(component.rebuildTriggers.HasFlag(SplineMesher.RebuildTriggers.OnUIChange))
                    {
                        Rebuild();
                    }
                }
            }

            EditorGUILayout.Space();

            if (((SplineMesher)target).rebuildTriggers.HasFlag(SplineMesher.RebuildTriggers.OnUIChange) == false)
            {
                EditorGUILayout.HelpBox("Auto-rebuilding on UI change is disabled (see Rebuilder Triggers)", MessageType.None);
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Rebuild now"))
                    {
                        Rebuild();
                    }
                    GUILayout.FlexibleSpace();
                }
            }
            
            #if SM_DEV
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{((SplineMesher)target).GetLastRebuildTime()}ms", EditorStyles.centeredGreyMiniLabel);
            }
            #endif
            #endif
            
            EditorGUILayout.Space();
            
            EditorGUILayout.LabelField("- Staggart Creations -", EditorStyles.centeredGreyMiniLabel);
        }

        private void DrawHeaderSection()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                UI.DrawHeader();
                
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "Help").image, "Help window"), EditorStyles.miniButtonMid, GUILayout.Width(30f)))
                {
                    HelpWindow.ShowWindow();
                }
                
                if (GUILayout.Button(new GUIContent(EditorGUIUtility.IconContent(iconPrefix + "Settings").image, "Utility functions"), EditorStyles.miniButtonMid, GUILayout.Width(30f)))
                {
                    #if SPLINES
                    GenericMenu menu = new GenericMenu();

                    SplineMesher component = (SplineMesher)target;
                    
                    menu.AddItem(new GUIContent("Open preferences"), false, () =>
                    {
                        SettingsService.OpenUserPreferences(SplineMeshEditor.Preferences.PreferencesPath);
                    });
                    
                    menu.AddSeparator(string.Empty);
                    
                    menu.AddItem(new GUIContent("Clear Scale data"), false, () =>
                    {
                        component.ResetScaleData();
                        EditorUtility.SetDirty(component);
                    });
                    menu.AddItem(new GUIContent("Clear Roll data"), false, () =>
                    {
                        component.ResetRollData();
                        EditorUtility.SetDirty(component);
                    });
                    menu.AddItem(new GUIContent("Clear Vertex Color data"), false, () =>
                    {
                        component.ResetVertexColorData();
                        EditorUtility.SetDirty(component);
                    });
                    menu.AddItem(new GUIContent("Clear Conforming data"), false, () =>
                    {
                        component.ResetConformingData();
                        EditorUtility.SetDirty(component);
                    });
                    
                    menu.AddSeparator(string.Empty);

                    menu.AddItem(new GUIContent("Detach caps", "tooltip"), false, () =>
                    {
                        component.DetachCaps();
                        EditorUtility.SetDirty(component);
                    });
                    menu.AddItem(new GUIContent("Rebuild all instances using same mesh", "tooltip"), false, () =>
                    {
                        if (sourceMesh.objectReferenceValue)
                        {
                            SplineMeshEditor.RebuildAllInstancesUsingMesh(sourceMesh.objectReferenceValue as Mesh);
                        }
                    });
                    menu.AddItem(new GUIContent("Reverse spline"), false, () => component.ReverseSpline());
                    menu.AddItem(new GUIContent("Generate lightmap UVs"), false, () => SplineMeshEditor.GenerateLightmapUV(component));
                    
                    menu.ShowAsContext();
                    #endif
                }
            }
            
            #if SM2
            EditorGUILayout.HelpBox("Spline Mesher Pro is installed, please use the migration tool to convert this instance", MessageType.Info);
            #endif
        }
        
        private void DrawInputOutput()
        {
            //setupSection.DrawHeader(() => SwitchSection(setupSection));
            //EditorGUILayout.BeginFadeGroup(setupSection.anim.faded);
            {
                //EditorGUILayout.Space();

                //if (setupSection.Expanded)
                {

                    EditorGUILayout.LabelField("Input", EditorStyles.boldLabel);
                    using (new EditorGUILayout.HorizontalScope())
                    {
                    #if SPLINES
                        EditorGUI.BeginChangeCheck();
                        {
                            EditorGUILayout.PropertyField(splineContainer);
                        }
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (splineContainer.objectReferenceValue)
                            {
                                foreach (var target in targets)
                                {
                                    ((SplineMesher)target).ValidateData();
                                }

                                requiresRebuild = true;
                            }
                        }

                        if (splineContainer.objectReferenceValue)
                        {
                            if (GUILayout.Button("Edit", EditorStyles.miniButton, GUILayout.Width(50f)))
                            {
                                Selection.activeGameObject = ((SplineMesher)target).splineContainer.gameObject;
                                EditorApplication.delayCall += ToolManager.SetActiveContext<SplineToolContext>;
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Add", EditorStyles.miniButton, GUILayout.Width(50f)))
                            {
                                splineContainer.objectReferenceValue = SplineMeshEditor.AddSplineContainer(((SplineMesher)target).gameObject);
                            }
                        }
                    #endif
                    }
                    if (splineContainer.objectReferenceValue == false)
                    {
                        EditorGUILayout.HelpBox("A source Spline Container must be assigned", MessageType.Error);
                    }
                    else
                    {
                        EditorGUI.indentLevel++;
                        ExpandIndexUnits = EditorGUILayout.Foldout(ExpandIndexUnits, "Configuration");

                        if (ExpandIndexUnits)
                        {
                            EditorGUILayout.LabelField("Data index units", EditorStyles.boldLabel);
                            EditorGUILayout.HelpBox("Specifies in what format the data points positions are stored." +
                                                    "\n" +
                                                    "\n• Distance: Points stays consistently in place, even when the spline gets longer." +
                                                    "\n• Normalized: Points stays fixed relative to the spline, and move with if it stretches" +
                                                    "\n• Knot: Points are attached to the spline knots",
                                MessageType.Info);
                            
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(scaleDataPathIndexUnit, new GUIContent("Scale"), GUILayout.Width(EditorGUIUtility.labelWidth + 180f));
                            EditorGUILayout.PropertyField(rollDataPathIndexUnit, new GUIContent("Rotation Roll"), GUILayout.Width(EditorGUIUtility.labelWidth + 180f));
                            EditorGUILayout.PropertyField(colorPathIndexUnit, new GUIContent("Vertex Color"), GUILayout.Width(EditorGUIUtility.labelWidth + 180f));
                            EditorGUILayout.PropertyField(conformingPathIndexUnit, new GUIContent("Conforming"), GUILayout.Width(EditorGUIUtility.labelWidth + 180f));
                            if (EditorGUI.EndChangeCheck())
                            {
                                requiresRebuild = true;
                            }
                            EditorGUILayout.Separator();
                        }
                        EditorGUI.indentLevel--;
                    }

                    if (splineContainer.objectReferenceValue)
                    {
                        EditorGUILayout.Space();

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUI.BeginChangeCheck();
                            EditorGUILayout.PropertyField(sourceMesh);
                            if (EditorGUI.EndChangeCheck())
                            {
                                CheckInputMeshReadability();
                                requiresRebuild = true;
                            }

                            if (sourceMesh.objectReferenceValue)
                            {
                                PreviewMesh = GUILayout.Toggle(PreviewMesh,
                                    new GUIContent(EditorGUIUtility.IconContent(iconPrefix + (PreviewMesh ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff")).image,
                                        "Toggle mesh inspector"), "Button", GUILayout.MaxWidth(40f));
                            }
                        }

                        if (sourceMesh.objectReferenceValue == false)
                        {
                            EditorGUILayout.HelpBox("An input mesh must be assigned", MessageType.Error);
                        }
                        else
                        {
                            if (PreviewMesh)
                            {
                                Mesh mesh = (Mesh)sourceMesh.objectReferenceValue;

                                if (sourceMeshPreview.mesh != mesh) sourceMeshPreview.mesh = mesh;
                                Rect previewRect = EditorGUILayout.GetControlRect(false, 150f);

                                var previewMouseOver = previewRect.Contains(Event.current.mousePosition);
                                var meshPreviewFocus = previewMouseOver && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag);

                                //EditorGUILayout.LabelField(meshPreviewFocus.ToString());

                                if (meshPreviewFocus)
                                {
                                    sourceMeshPreview.OnPreviewGUI(previewRect, GUIStyle.none);
                                }
                                else
                                {
                                    if (Event.current.type == EventType.Repaint)
                                    {
                                        GUI.DrawTexture(previewRect, sourceMeshPreview.RenderStaticPreview((int)previewRect.width, (int)previewRect.height));
                                    }
                                }
                                previewRect.y += previewRect.height - 22f;
                                previewRect.x += 5f;
                                previewRect.height = 22f;

                                GUI.Label(previewRect, MeshPreview.GetInfoString(mesh), EditorStyles.miniLabel);

                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    sourceMeshPreview.OnPreviewSettings();
                                }

                                EditorGUILayout.Space();
                            }

                            if (meshIsReadable == false && Application.isPlaying)
                            {
                                EditorGUILayout.HelpBox("This mesh is not marked as readable. In a build, it would not be usable.", MessageType.Warning);

                                if (GUILayout.Button("Enable Read/Write option on mesh"))
                                {
                                    if(SplineMeshEditor.SetMeshReadWriteFlag(sourceMesh.objectReferenceValue as Mesh))
                                        meshIsReadable = true;
                                }
                            }
                            
                            EditorGUI.BeginChangeCheck();
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(rotation);
                            EditorGUI.indentLevel--;
                            if (EditorGUI.EndChangeCheck()) requiresRebuild = true;

                            
                        }

                        EditorGUILayout.Space();

                        EditorGUILayout.LabelField("Output", EditorStyles.boldLabel);
                        {
                            using (new EditorGUILayout.HorizontalScope())
                            {
                                EditorGUI.BeginChangeCheck();
                                EditorGUILayout.PropertyField(outputObject);
                                if (EditorGUI.EndChangeCheck())
                                {
                                    requiresLightmapUV = SplineMeshEditor.RequiresLightmapUV(((SplineMesher)target));
                                    requiresRebuild = true;
                                }

                                outputHasMeshFilter = outputObject.objectReferenceValue ? ((GameObject)outputObject.objectReferenceValue).GetComponent<MeshFilter>() : false;
                                outputHasMeshRenderer = outputObject.objectReferenceValue ? ((GameObject)outputObject.objectReferenceValue).GetComponent<MeshRenderer>() : false;

                                if (GUILayout.Button("This", EditorStyles.miniButton, GUILayout.Width(50f)))
                                {
                                    SplineMesher component = ((SplineMesher)target);

                                    outputObject.objectReferenceValue = component.gameObject;

                                    if (outputObject.objectReferenceValue.GetHashCode() != component.gameObject.GetHashCode()) requiresRebuild = true;

                                    SplineMeshEditor.SetupSplineRenderer(component.gameObject);

                                    requiresRebuild = true;
                                }

                            }
                            if (outputObject.objectReferenceValue == false)
                            {
                                EditorGUILayout.HelpBox("An output GameObject must be assigned", MessageType.Warning);
                            }
                            else
                            {
                                if (requiresLightmapUV)
                                {
                                    string lightmapNotification = "Output has no or outdated lightmap UVs, they will be generated, once light baking starts.";
                                    
                                    #if BAKERY_INCLUDED
                                    lightmapNotification = "[Bakery detected] " + lightmapNotification;
                                    #endif
                                    
                                    EditorGUILayout.HelpBox(lightmapNotification, MessageType.None);

                                    if (SplineMeshEditor.Preferences.AutoGeneratedLightmapUV == false)
                                    {
                                        EditorGUILayout.HelpBox("Lightmap generation has been disabled in Preferences", MessageType.Warning);
                                    }
                                }

                                if (outputHasMeshFilter == false)
                                {
                                    EditorGUILayout.HelpBox("Output object is missing a Mesh Filter component, no mesh can be created for it", MessageType.Error);
                                }
                                if (outputHasMeshRenderer == false)
                                {
                                    EditorGUILayout.HelpBox("Output object is missing a Mesh Renderer component, no geometry will be visible", MessageType.Error);
                                }

                                EditorGUI.indentLevel++;
                                EditorGUILayout.PropertyField(meshSettings);
                                EditorGUI.indentLevel--;
                            }
                        }

                        int rebuildTrigger = rebuildTriggers.intValue;
                        
                        if ((rebuildTrigger & (int)SplineMesher.RebuildTriggers.OnStart) != (int)SplineMesher.RebuildTriggers.OnStart && isPrefab)
                        {
                            EditorGUILayout.HelpBox("Procedurally created geometry cannot be used in a prefab." +
                                                    "\n\nMesh data will be lost when the prefab is used outside of the scene it was created in." +
                                                    "\n\nExport the created mesh to an FBX file, and use that instead. Or enable the \"On Start()\" option under Rebuild Triggers.", MessageType.Warning);
                        }

                        EditorGUILayout.Space();

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.PropertyField(rebuildTriggers, new GUIContent("Rebuild triggers", rebuildTriggers.tooltip), GUILayout.Width(EditorGUIUtility.labelWidth + 140f));
                            if (GUILayout.Button(new GUIContent(" Rebuild", EditorGUIUtility.IconContent("d_Refresh").image)))
                            {
                                Rebuild();
                                return;
                            }
                            GUILayout.FlexibleSpace();
                        }
                        
                        if ((rebuildTrigger & (int)SplineMesher.RebuildTriggers.OnSplineChanged) == (int)SplineMesher.RebuildTriggers.OnSplineChanged)
                        {
                            EditorGUI.indentLevel++;
                            EditorGUILayout.PropertyField(splineChangeMode, new GUIContent("Spline Change Mode", splineChangeMode.tooltip), GUILayout.MaxWidth(EditorGUIUtility.labelWidth + 140f));
                            EditorGUI.indentLevel--;
                        }

                        if ((rebuildTrigger & (int)SplineMesher.RebuildTriggers.OnTransformChange) == (int)SplineMesher.RebuildTriggers.OnTransformChange)
                        {
                            //Check if Gizmos are disabled in the scene-view
                            if (SceneView.lastActiveSceneView && SceneView.lastActiveSceneView.drawGizmos == false)
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    foreach (var m_target in targets)
                                    {
                                        ((SplineMesher)m_target).ListenForTransformChanges();
                                    }
                                };
                            } }

                    }

                    EditorGUILayout.Space(10f);
                }
            }
            //EditorGUILayout.EndFadeGroup();
        }

        private void DrawCollider()
        {
            colliderSection.DrawHeader(() => SwitchSection(colliderSection));
            EditorGUILayout.BeginFadeGroup(colliderSection.anim.faded);
            {
                if (colliderSection.Expanded)
                {
                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();
                    {
                        EditorGUILayout.PropertyField(enableCollider, new GUIContent("Enable", enableCollider.tooltip));

                        EditorGUILayout.Space();

                        if (enableCollider.boolValue)
                        {
                            EditorGUILayout.PropertyField(colliderOnly);
                            EditorGUILayout.PropertyField(colliderType, new GUIContent("Type", colliderType.tooltip), GUILayout.MaxWidth(EditorGUIUtility.labelWidth + 80f));
                            EditorGUI.indentLevel++;
                            if (colliderType.intValue == (int)Settings.ColliderType.Mesh)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUI.BeginChangeCheck();
                                    EditorGUILayout.PropertyField(collisionMesh);
                                    
                                    if (GUILayout.Button(new GUIContent("Same", "Use the same mesh for collision as the source mesh"), EditorStyles.miniButton, GUILayout.Width(50f)))
                                    {
                                        collisionMesh.objectReferenceValue = sourceMesh.objectReferenceValue;
                                    }

                                    if (EditorGUI.EndChangeCheck())
                                    {
                                        CheckInputColliderReadability();
                                    }
                                }

                                if (collisionMesh.objectReferenceValue)
                                {
                                    if (colliderIsReadable == false)
                                    {
                                        EditorGUILayout.HelpBox("The input mesh is not marked as readable. In a build, it would not be usable.", MessageType.Warning);

                                        if (GUILayout.Button("Enable Read/Write option on mesh"))
                                        {
                                            if (SplineMeshEditor.SetMeshReadWriteFlag(collisionMesh.objectReferenceValue as Mesh))
                                                colliderIsReadable = true;
                                        }
                                    }
                                }
                            }
                            else if (colliderType.intValue == (int)Settings.ColliderType.Box)
                            {
                                using (new EditorGUILayout.HorizontalScope())
                                {
                                    EditorGUILayout.PrefixLabel(new GUIContent("Subdivisions", colliderBoxSubdivisions.tooltip));
                                    using (new EditorGUI.DisabledScope(colliderBoxSubdivisions.intValue <= 0))
                                    {
                                        if (GUILayout.Button("-", EditorStyles.miniButtonLeft, GUILayout.Width(25f)))
                                        {
                                            colliderBoxSubdivisions.intValue--;
                                        }
                                    }
                                    GUILayout.Space(-15f);
                                    EditorGUILayout.PropertyField(colliderBoxSubdivisions, GUIContent.none, GUILayout.MaxWidth(40f));
                                    if (GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(25f)))
                                    {
                                        colliderBoxSubdivisions.intValue++;
                                    }
                                }
                            }
                            EditorGUI.indentLevel--;
                        }
                    }
                    if (EditorGUI.EndChangeCheck()) requiresRebuild = true;

                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndFadeGroup();
        }
        
        private void DrawDistribution()
        {
            distributionSection.DrawHeader(() => SwitchSection(distributionSection));
            EditorGUILayout.BeginFadeGroup(distributionSection.anim.faded);
            {
                if (distributionSection.Expanded)
                {
                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUI.BeginDisabledGroup(autoSegmentCount.boolValue);
                        {
                            EditorGUILayout.PropertyField(segments, GUILayout.Width(EditorGUIUtility.labelWidth + 60f));
                        }
                        EditorGUI.EndDisabledGroup();
                        
                        autoSegmentCount.boolValue = GUILayout.Toggle(autoSegmentCount.boolValue, new GUIContent(" Auto", autoSegmentCount.tooltip), "Button", GUILayout.MaxWidth(60f), GUILayout.MaxHeight(19f));
                    }
                    
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(stretchToFit);

                    if (stretchToFit.boolValue == false)
                    {
                        EditorGUILayout.PropertyField(evenOnly);
                    }
                    
                    EditorGUILayout.Space();

                    EditorGUILayout.LabelField("Trimming", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(trimStart, new GUIContent("Start", trimStart.tooltip), GUILayout.Width(EditorGUIUtility.labelWidth + 60f));
                    EditorGUILayout.PropertyField(trimEnd, new GUIContent("End", trimEnd.tooltip), GUILayout.Width(EditorGUIUtility.labelWidth + 60f));
                    
                    EditorGUILayout.Separator();

                    EditorGUILayout.PropertyField(spacing, GUILayout.Width(EditorGUIUtility.labelWidth + 60f));

                    if (EditorGUI.EndChangeCheck())
                    {
                        requiresCapUpdate = true;
                        requiresRebuild = true;
                    }

                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndFadeGroup();
        }
        
        private void DrawDeforming()
        {
            deformingSection.DrawHeader(() => SwitchSection(deformingSection));
            EditorGUILayout.BeginFadeGroup(deformingSection.anim.faded);
            {
                if (deformingSection.Expanded)
                {
                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();
                    
                    EditorGUI.BeginChangeCheck();
                    
                    EditorGUILayout.PropertyField(curveOffset);
                    EditorGUILayout.PropertyField(pivotOffset);
                    
                    EditorGUILayout.Separator();
                    EditorGUILayout.PropertyField(scale);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(scaleInterpolation, new GUIContent("Interpolation mode"), GUILayout.Width(EditorGUIUtility.labelWidth + 180f));
                    EditorGUI.indentLevel--;

                    if (EditorGUI.EndChangeCheck())
                    {
                        requiresCapUpdate = true;
                    }
                    
                    #if SPLINES
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(new GUIContent("  Open Editor", ScaleTool.LoadIcon()), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight + 5f)))
                        {
                            ToolManager.SetActiveTool<ScaleTool>();
                        }
                        if (GUILayout.Button(new GUIContent("▼"), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight + 5f)))
                        {
                            GenericMenu menu = new GenericMenu();

                            SplineMesher component = (SplineMesher)target;
                    
                            menu.AddItem(new GUIContent("Clear Scale data"), false, () =>
                            {
                                component.ResetScaleData();
                                EditorUtility.SetDirty(component);
                            });
                            
                            menu.ShowAsContext();
                        }
                    }
                    #endif
                    
                    EditorGUILayout.LabelField("Roll", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(ignoreKnotRotation);
                    
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(rollMode, GUILayout.Width(EditorGUIUtility.labelWidth + 180f));
                    EditorGUILayout.PropertyField(rollFrequency, new GUIContent("Frequency", rollFrequency.tooltip));
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.PropertyField(rollAngle, new GUIContent("Angle °", rollAngle.tooltip));
                        if (GUILayout.Button(new GUIContent("R", "Reset to 0"), EditorStyles.miniButton, GUILayout.MaxWidth(25f)))
                        {
                            rollAngle.floatValue = 0f;
                        }
                    }
                    if (EditorGUI.EndChangeCheck())
                    {
                        requiresCapUpdate = true;
                    }
                    if (rollAngle.floatValue != 0 && enableConforming.boolValue && align.boolValue)
                    {
                        EditorGUILayout.HelpBox("The Conforming feature is enabled, which overrides this rotation completely", MessageType.Warning);
                    }
                    
                    #if SPLINES
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();

                        if (GUILayout.Button(new GUIContent("  Open Editor", RollTool.LoadIcon()), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight + 5f)))
                        {
                            ToolManager.SetActiveTool<RollTool>();
                        }
                        if (GUILayout.Button(new GUIContent("▼"), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight + 5f)))
                        {
                            GenericMenu menu = new GenericMenu();

                            SplineMesher component = (SplineMesher)target;
                    
                            menu.AddItem(new GUIContent("Clear Roll data"), false, () =>
                            {
                                component.ResetRollData();
                                EditorUtility.SetDirty(component);
                            });
                            
                            menu.ShowAsContext();
                        }
                    }
                    #endif
                    
                    EditorGUILayout.Space();
                    
                    EditorGUILayout.LabelField("UV", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(uvStretchMode, GUILayout.Width(EditorGUIUtility.labelWidth + 180f));
                    EditorGUILayout.PropertyField(uvScale);
                    EditorGUILayout.PropertyField(uvOffset);

                    if (EditorGUI.EndChangeCheck())
                    {
                        requiresRebuild = true;
                    }

                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndFadeGroup();
        }
        
        private void DrawConforming()
        {
            conformingSection.DrawHeader(() => SwitchSection(conformingSection));
            EditorGUILayout.BeginFadeGroup(conformingSection.anim.faded);
            {
                if (conformingSection.Expanded)
                {
                    EditorGUILayout.Space();

                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.PropertyField(enableConforming);

                    if (enableConforming.boolValue)
                    {
                        #if SPLINES
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();

                            if (GUILayout.Button(new GUIContent("  Open Editor", ConformingTool.LoadIcon()), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight + 5f)))
                            {
                                ToolManager.SetActiveTool<ConformingTool>();
                            }
                            if (GUILayout.Button(new GUIContent("▼"), GUILayout.MaxHeight(EditorGUIUtility.singleLineHeight + 5f)))
                            {
                                GenericMenu menu = new GenericMenu();

                                SplineMesher component = (SplineMesher)target;
                    
                                menu.AddItem(new GUIContent("Clear Conforming Strengths"), false, () =>
                                {
                                    component.ResetConformingData();
                                    EditorUtility.SetDirty(component);
                                });
                            
                                menu.ShowAsContext();
                            }
                        }
                        #endif
                        
                        EditorGUILayout.Separator();
                        
                        EditorGUILayout.PropertyField(conformingDirection);
                        EditorGUILayout.PropertyField(seekDistance);
                        
                        EditorGUILayout.Separator();
                        
                        EditorGUILayout.LabelField("Filtering", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(terrainOnly);
                        EditorGUILayout.PropertyField(layerMask);
                        
                        EditorGUILayout.Separator();
                        
                        EditorGUILayout.LabelField("Operations", EditorStyles.boldLabel);
                        EditorGUILayout.PropertyField(align);
                        EditorGUILayout.PropertyField(blendNormal);
                    }

                    if (EditorGUI.EndChangeCheck()) requiresRebuild = true;

                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndFadeGroup();

        }
        
        private void DrawCaps()
        {
            void DrawCap(SerializedProperty cap)
            {
                SerializedProperty prefab = cap.FindPropertyRelative("prefab");
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.PropertyField(prefab);
                    if (prefab.objectReferenceValue)
                    {
                        if (GUILayout.Button("X", GUILayout.MaxWidth(30f)))
                        {
                            prefab.objectReferenceValue = null;
                        }
                    }
                }

                if (prefab.objectReferenceValue)
                {
                    EditorGUILayout.Separator();

                    EditorGUILayout.LabelField("Position", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(cap.FindPropertyRelative("offset"));
                    EditorGUILayout.PropertyField(cap.FindPropertyRelative("shift"));
                            
                    EditorGUILayout.Separator();

                    EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(cap.FindPropertyRelative("align"));
                    
                    EditorGUILayout.PropertyField(cap.FindPropertyRelative("rotation"));
                            
                    EditorGUILayout.Separator();

                    EditorGUILayout.LabelField("Scale", EditorStyles.boldLabel);
                    EditorGUILayout.PropertyField(cap.FindPropertyRelative("matchScale"));
                    EditorGUILayout.PropertyField(cap.FindPropertyRelative("scale"));
                            
                    SerializedProperty instances = cap.FindPropertyRelative("instances");
                    EditorGUILayout.LabelField($"Instances: {instances.arraySize}", EditorStyles.miniLabel);
                }
            }
            
            capsSection.DrawHeader(() => SwitchSection(capsSection));
            EditorGUILayout.BeginFadeGroup(capsSection.anim.faded);
            {
                if (capsSection.Expanded)
                {
                    EditorGUILayout.Space();
                    
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        EditorGUILayout.LabelField("Label in scene view", EditorStyles.miniLabel, GUILayout.MaxWidth(100f));
                        LabelCaps = GUILayout.Toggle(LabelCaps,
                            new GUIContent(EditorGUIUtility.IconContent(iconPrefix + (LabelCaps ? "animationvisibilitytoggleon" : "animationvisibilitytoggleoff")).image,
                                "Identify the caps in the scene view"), "Button", GUILayout.MaxWidth(40f));
                    }

                    if (ignoreKnotRotation.boolValue)
                    {
                        EditorGUILayout.HelpBox("Caps cannot be properly positioned if the \"Ignore Knot Rotation\" option is enabled for Deforming." +
                                                "\n\n" +
                                                "Instead, ensure knots are correctly rotated as desired.", MessageType.Warning, true);
                
                        EditorGUILayout.Separator();
                    }
                    
                    EditorGUI.BeginChangeCheck();

                    EditorGUILayout.LabelField("Start", EditorStyles.boldLabel);
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        DrawCap(startCap);
                    }
                    
                    EditorGUILayout.Separator();

                    EditorGUILayout.LabelField("End", EditorStyles.boldLabel);
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        DrawCap(endCap);
                    }
                    
                    EditorGUILayout.Separator();
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        requiresCapUpdate = true;
                    }
                    
                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawEvents()
        {
            eventsSection.DrawHeader(() => SwitchSection(eventsSection));
            EditorGUILayout.BeginFadeGroup(eventsSection.anim.faded);
            {
                if (eventsSection.Expanded)
                {
                    EditorGUILayout.Space();

                    EditorGUILayout.PropertyField(onPreRebuild);
                    EditorGUILayout.PropertyField(onPostRebuild);

                    EditorGUILayout.Space();
                }
            }
            EditorGUILayout.EndFadeGroup();
        }
        
        private void Rebuild()
        {
            requiresRebuild = false;

            if (SplineMeshEditor.Preferences.RebuildEveryFrame)
            {
                RebuildTargets();
            }
            else
            {
                EditorApplication.delayCall += RebuildTargets;
            }
        }

        private void RebuildTargets()
        {
            foreach (var m_target in targets)
            {
                SplineMesher mesher = (SplineMesher)m_target;
                
                mesher.Rebuild();
                EditorUtility.SetDirty(mesher.meshFilter);
            }
        }
        
        private void SwitchSection(UI.Section targetSection)
        {
            if (SplineMeshEditor.Preferences.SectionStyleMode == SplineMeshEditor.Preferences.SectionStyle.Foldouts)
            {
                //Classic foldout behaviour
                targetSection.Expanded = !targetSection.Expanded;
            }
            else
            {
                //Accordion behaviour
                foreach (var section in sections)
                {
                    section.Expanded = (targetSection == section) && !section.Expanded;
                    //section.Expanded = true;
                }
            }
        }
        
        private void DuringSceneGUI(SceneView sceneView)
        {
            if (LabelCaps == false || capsSection.Expanded == false) return;
            
            Handles.BeginGUI();

            Handles.color = Color.black;
            foreach (var m_target in targets)
            {
                SplineMesher splineMesher = (SplineMesher)m_target;

                if (splineMesher.startCap.prefab)
                {
                    for (int i = 0; i < splineMesher.startCap.instances.Length; i++)
                    {
                        if(splineMesher.startCap.instances[i] == null) continue;
                        
                        Vector3 position = splineMesher.startCap.instances[i].transform.position;

                        DrawLabel(position, "Start");
                    }
                }
                
                if (splineMesher.endCap.prefab)
                {
                    for (int i = 0; i < splineMesher.endCap.instances.Length; i++)
                    {
                        if(splineMesher.endCap.instances[i] == null) continue;
                        
                        Vector3 position = splineMesher.endCap.instances[i].transform.position;

                        DrawLabel(position, "End");
                    }
                }
            }
            Handles.EndGUI();
        }

        private void DrawLabel(Vector3 position, string text)
        {
            var labelOffset = HandleUtility.GetHandleSize(position) * 0.25f;
            position += new Vector3(0, -labelOffset, 0);

            Vector2 screenPos = HandleUtility.WorldToGUIPoint(position);
            Rect r = new Rect(screenPos.x, screenPos.y, Label.CalcSize(new GUIContent(text)).x, 22f);
            
            //Center
            r.x -= r.width * 0.5f;
            
            GUI.color = EditorGUIUtility.isProSkin ? Color.white * 0.5f : Color.gray;
            if (EditorGUIUtility.isProSkin)
            {
                GUI.Box(r, "", EditorStyles.textArea);
            }

            GUI.color = Color.white * 2f;
            GUI.Label(r, text, Label);
            //Handles.Label(position , "End");
        }
        
        private static GUIStyle _Label;
        public static GUIStyle Label
        {
            get
            {
                if (_Label == null)
                {
                    _Label = new GUIStyle(EditorStyles.largeLabel)
                    {
                        alignment = TextAnchor.MiddleLeft,
                        fontSize = 12,
                        fontStyle = FontStyle.Bold,
                        padding = new RectOffset()
                        {
                            left = 10,
                            right = 10,
                            top = 0,
                            bottom = 0
                        }
                    };
                }

                return _Label;
            }
        }
    }
}