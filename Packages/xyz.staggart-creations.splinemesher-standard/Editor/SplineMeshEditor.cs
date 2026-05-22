// Staggart Creations (http://staggart.xyz)
// Copyright protected under Unity Asset Store EULA
// Copying or referencing source code for the production of new asset store, or public content, is strictly prohibited!

using System;
using System.Collections.Generic;
using sc.modeling.splines.runtime;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#if SPLINES
using UnityEngine.Splines;
using UnityEditor.Splines;
#endif

#if FBX_EXPORTER
using UnityEditor.Formats.Fbx.Exporter;
#endif

namespace sc.modeling.splines.editor
{
    public static class SplineMeshEditor
    {
        private static bool STARTUP_PERFORMED
        {
            get => SessionState.GetBool("SPLINE_MESHER1_EDITOR_STARTED", false);
            set => SessionState.SetBool("SPLINE_MESHER1_EDITOR_STARTED", value);
        }
        
        [InitializeOnLoadMethod]
        static void Initialize()
        {
            #if !SPLINES
            if (EditorUtility.DisplayDialog("Spline Mesher", $"This asset requires the \"Splines\" (v{MIN_SPLINES_VERSION}) package dependency, which is not installed.", "Install now", "Later"))
            {
                //Note: Mathematics package is a dependency, so will also be installed
                UnityEditor.PackageManager.Client.Add($"com.unity.splines@{MIN_SPLINES_VERSION}");
            }
            #endif
            
            #if !SM_DEV
            if (STARTUP_PERFORMED == false)
            #endif
            {
                AssetInfo.VersionChecking.CheckForUpdate();
                STARTUP_PERFORMED = true;
            }

            Lightmapping.bakeStarted += OnLightBakeStart;
            
            #if BAKERY_INCLUDED
            ftRenderLightmap.OnPreFullRender += OnBakeryStart;
            #endif
        }
        
        private class ModelImportCallback : AssetPostprocessor
        {
            //Note, need to work with paths because object references won't be valid during the importing stage
            private static readonly List<string> importedMeshPaths = new();
            
            void OnPostprocessModel(GameObject gameObject)
            {
                MeshFilter[] meshFilters = gameObject.GetComponentsInChildren<MeshFilter>();
                if (meshFilters.Length == 0) return;
                
                importedMeshPaths.Clear();
                importedMeshPaths.Add(assetPath);

                //Defer handling until after import is fully done
                EditorApplication.delayCall += ProcessImportedMeshes;
            }

            private static void ProcessImportedMeshes()
            {
                if (importedMeshPaths.Count == 0)
                    return;
                
                //Debug.Log($"Processing {importedMeshes.Count} imported meshes");

                int instanceCount = SplineMesher.Instances.Count;
                if (instanceCount == 0)
                {
                    //Debug.LogWarning("[SplineMesher.ModelImportCallback] Skipped, no mesher instances found");
                    importedMeshPaths.Clear();
                    return;
                }

                List<SplineMesher> toRebuild = new();

                foreach (var importedMeshPath in importedMeshPaths)
                {
                    if (string.IsNullOrEmpty(importedMeshPath))
                    {
                        Debug.LogError($"[SplineMesher.ModelImportCallback] Attempting to process an empty mesh file path");
                    }
                    Mesh mesh = AssetDatabase.LoadAssetAtPath<Mesh>(importedMeshPath);

                    //Debug.Log($"[SplineMesher.ModelImportCallback] Processing {importedMeshPath}");
                    
                    for (int j = 0; j < instanceCount; j++)
                    {
                        var mesher = SplineMesher.Instances[j];

                        if (!mesher.rebuildTriggers.HasFlag(SplineMesher.RebuildTriggers.OnMeshImported)) continue;

                        string mesherMeshPath = AssetDatabase.GetAssetPath(mesher.sourceMesh);

                        //Ignore procedural or null input meshes
                        if (string.IsNullOrEmpty(mesherMeshPath))
                        {
                            Debug.LogWarning($"[SplineMesher.ModelImportCallback] {mesher.name} skipped because its input mesh ({mesh.name}) is missing", mesher);
                            continue;
                        }

                        if (mesherMeshPath == importedMeshPath)
                        {
                            //Debug.Log($"[SplineMesher.ModelImportCallback] Mesh file \"{importedMeshPath}\" has been changed and is used by {mesher.name}.");

                            toRebuild.Add(mesher);
                        }
                        
                    }
                }

                foreach (var mesher in toRebuild)
                {
                    Debug.Log($"[SplineMesher.ModelImportCallback] {mesher.name} rebuilt because one of its input meshes was reimported.");
                    
                    mesher.Rebuild();
                    EditorUtility.SetDirty(mesher);
                }
            }
        }
        
        private static void OnBakeryStart(object sender, EventArgs e)
        {
            OnLightBakeStart();
        }
       
       private static void OnLightBakeStart()
        {
            if (Preferences.AutoGeneratedLightmapUV == false) return;
            
            int count = 0;
            System.Diagnostics.Stopwatch lightmapUVUnwrapTimer = new System.Diagnostics.Stopwatch();
            
            //Find the spline meshes that still require lightmap UV's
            foreach (SplineMesher splineMesher in SplineMesher.Instances)
            {
                lightmapUVUnwrapTimer.Start();
                
                if (RequiresLightmapUV(splineMesher))
                {
                    //Debug.Log($"{splineMesher.name} requires new lightmap UV's");
                    
                    GenerateLightmapUV(splineMesher);

                    count++;
                }
                lightmapUVUnwrapTimer.Stop();
            }
            
            if (count > 0)
            {
                Debug.Log($"[Spline Mesher] Lightmap UV created for {count} spline meshes (Duration: {lightmapUVUnwrapTimer.ElapsedMilliseconds}ms)");
            }
        }

        public static bool RequiresLightmapUV(SplineMesher instance)
        {
            if (!instance.outputObject) return false;
            
            MeshFilter mf = instance.outputObject.GetComponent<MeshFilter>();
            
            //Improper set up
            if (mf == null) return false;
            
            StaticEditorFlags staticFlags = GameObjectUtility.GetStaticEditorFlags(mf.gameObject);
                
            //Mesh renderer marked as static
            if (staticFlags.HasFlag(StaticEditorFlags.ContributeGI))
            {
                Mesh mesh = mf.sharedMesh;

                if (mesh == null) return false;
                
                //Conditions that indicate the mesh has no lightmap UV's
                //Note that rebuilding the spline mesh clears the UV2 channel, automatically marking it as 'dirty' again.
                if (mesh.uv2 == null || mesh.uv2.Length == 0)
                {
                    return true;
                }
            }

            return false;
        }

        public static void GenerateLightmapUV(SplineMesher instance)
        {
            MeshFilter mf = instance.outputObject.GetComponent<MeshFilter>();

            if (!mf) return;
            
            Mesh mesh = mf.sharedMesh;
            if(mesh == null) return;
            
            UnwrapParam.SetDefaults(out var unwrapSettings);
            
            unwrapSettings.hardAngle = instance.settings.mesh.lightmapUVAngleThreshold;
            unwrapSettings.packMargin *= Mathf.Max(0.01f, instance.settings.mesh.lightmapUVMarginMultiplier);
            
            #if UNITY_2022_1_OR_NEWER
            if (Unwrapping.GenerateSecondaryUVSet(mesh, unwrapSettings) == false)
            {
                throw new Exception($"Lightmap UV generation for mesh \"{mesh.name}\" failed.");
            }
            #else
            Unwrapping.GenerateSecondaryUVSet(mesh, unwrapSettings);
            #endif
        }
        
        public static bool CheckInputMeshReadability(Mesh mesh)
        {
            if (mesh == null) return false;

            //Saved on disk
            if (EditorUtility.IsPersistent(mesh))
            {
                string assetPath = AssetDatabase.GetAssetPath(mesh);
                
                //Default meshes
                if (assetPath.StartsWith("Library")) return true;
            }
            
            return mesh.isReadable;
        }

        public static bool SetMeshReadWriteFlag(Mesh mesh)
        {
            if (mesh == null) return false;

            if (EditorUtility.IsPersistent(mesh) == false)
            {
                string msg = $"Can't enable the read/write option on \"{mesh.name}\". Because it is not an imported mesh." +
                             $"\n" +
                             $"\n" +
                             $"For script-based procedural geometry add the \"Mesh.UploadMeshData(false)\" function when creating the mesh to keep it readable.";
                EditorUtility.DisplayDialog("Spline Mesher", msg, "Ok");
                
                return false;
            }
            
            string assetPath = AssetDatabase.GetAssetPath(mesh);
            ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
            
            importer.isReadable = true;
            importer.SaveAndReimport();
            
            return true;
        }

        #region Editor menu additions
        #if SPLINES
        #if !SM2
        [MenuItem("CONTEXT/MeshFilter/Convert to Spline", true)]
        #endif
        private static bool AddMesherToMeshFilterValidation(MenuCommand cmd)
        {
            MeshFilter meshFilter = (MeshFilter)cmd.context;
            
            return !meshFilter.GetComponent<SplineMesher>();
        }
        
#if !SM2
        [MenuItem("CONTEXT/MeshFilter/Convert to Spline")]
#endif
        private static void AddMesherToMeshFilter(MenuCommand cmd)
        {
            MeshFilter meshFilter = (MeshFilter)cmd.context;

            if (meshFilter.sharedMesh == null)
            {
                throw new Exception("Mesh Filter component requires a mesh to convert");
            }
            
            if (EditorUtility.IsPersistent(meshFilter.sharedMesh) == false)
            {
                if (EditorUtility.DisplayDialog("Spline Mesher", "Mesh Filter uses a procedural mesh, it could have already been created from a spline.", "Continue", "Cancel") == false)
                {
                    return;
                }
            }

            SplineMesher component = meshFilter.gameObject.AddComponent<SplineMesher>();
            Undo.RegisterCreatedObjectUndo(component, "Created Spline Mesher");
            component.outputObject = meshFilter.gameObject;
            component.sourceMesh = meshFilter.sharedMesh;
            
            if (EditorUtility.DisplayDialog("Convert Mesh to Spline", "Create with a new spline?", "Yes", "No"))
            {
                SplineContainer splineContainer = meshFilter.gameObject.AddComponent<SplineContainer>();
                Undo.RegisterCreatedObjectUndo(splineContainer, "Created Spline Mesher");

                float meshLength = (meshFilter.sharedMesh.bounds.min.z - meshFilter.sharedMesh.bounds.max.z) * 2f;

                //Create new spline
                {
                    //One knot every 5 units
                    int knots = Mathf.RoundToInt(meshLength / 5f);
                    knots = Math.Max(2, knots);
                    
                    Spline spline = new Spline(knots, false);

                    for (int i = 0; i <= knots; i++)
                    {
                        float t = (float)i / (float)knots;
                        BezierKnot knot = new BezierKnot();
                        knot.Position = new Vector3(0, 0f, (t * meshLength) - (meshLength * 0.5f));
                        spline.Add(knot, TangentMode.Linear);
                    }

                    //Automatically recalculate tangents
                    spline.SetTangentMode(new SplineRange(0, spline.Count), TangentMode.AutoSmooth);
                    
                    //Spline container will be instantiated with a default spline, so overwrite it
                    splineContainer.Spline = spline;
                }

                component.splineContainer = splineContainer;

                //Activate the spline editor
                EditorApplication.delayCall += ToolManager.SetActiveContext<SplineToolContext>;
            }

            EditorUtility.SetDirty(meshFilter);
            
            component.Rebuild();

            if (Application.isPlaying == false) EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            
        }
#if !SM2
        [MenuItem("GameObject/3D Object/Spline Mesh", false, 0)]
#endif
        public static GameObject CreateSplineMesh()
        {
            Transform parent = Selection.activeGameObject ? Selection.activeGameObject.transform : null;
            GameObject gameObject = new GameObject(GameObjectUtility.GetUniqueNameForSibling(parent, "Spline Mesh"));
            Undo.RegisterCreatedObjectUndo(gameObject, "Created Spline Mesh Object");
            
            SplineMesher component = gameObject.AddComponent<SplineMesher>();
            
            SplineContainer splineContainer = gameObject.AddComponent<SplineContainer>();
            splineContainer.Splines = null;
            splineContainer.AddSpline(CreateDefaultSpline());

            component.splineContainer = splineContainer;
            component.outputObject = gameObject;

            SetupSplineRenderer(gameObject);
            
            component.Rebuild();

            if (parent) gameObject.transform.parent = parent;
            
            Selection.activeGameObject = gameObject;

            if (!parent) EditorApplication.ExecuteMenuItem("GameObject/Move To View");
            else gameObject.transform.localPosition = Vector3.zero;

            return gameObject;
        }

        public static SplineContainer AddSplineContainer(GameObject target)
        {
            if (target == null)
            {
                throw new Exception("Cannot add a Spline Container component to a null GameObject");
            }
            
            SplineContainer splineContainer = target.AddComponent<SplineContainer>();
            splineContainer.AddSpline(CreateDefaultSpline());

            EditorUtility.SetDirty(target);
            
            return splineContainer;
        }

        private static Spline CreateDefaultSpline()
        {
            int knots = 5;
            float amplitude = 2f;
            float length = 15f;
            
            Spline spline = new Spline(knots, false);

            for (int i = 0; i < knots; i++)
            {
                float t = (float)i / (float)knots;
                
                BezierKnot knot = new BezierKnot();
                knot.Position = new Vector3(Mathf.Sin(t * length) * amplitude, 0f, (t * length) - (length * 0.5f));
                spline.Add(knot, TangentMode.Linear);
            }

            //Automatically recalculate tangents
            spline.SetTangentMode(new SplineRange(0, spline.Count), TangentMode.AutoSmooth);

            return spline;
        }

#if !SM2
        [MenuItem("CONTEXT/SplineContainer/Add Spline Mesher", true, 3000)]
#endif
        private static bool AddMesherToSplineValidation(MenuCommand cmd)
        {
            SplineContainer splineContainer = (SplineContainer)cmd.context;
            
            return !splineContainer.GetComponent<SplineMesher>();
        }

#if !SM2
        [MenuItem("CONTEXT/SplineContainer/Add Spline Mesher", false, 3000)]
#endif
        private static void AddMesherToSpline(MenuCommand cmd)
        {
            SplineContainer splineContainer = (SplineContainer)cmd.context;

            SplineMesher component = splineContainer.GetComponent<SplineMesher>();

            if (component) return;
            
            component = splineContainer.gameObject.AddComponent<SplineMesher>();
            Undo.RegisterCreatedObjectUndo(component, $"Add Spline Mesher to {splineContainer.name}");
            
            component.splineContainer = splineContainer;
            component.outputObject = component.gameObject;

            SetupSplineRenderer(component.outputObject);
            
            component.Rebuild();
            
            EditorUtility.SetDirty(splineContainer.gameObject);
        }
        #endif

        /// <summary>
        /// Adds a Mesh Filter and Mesh Renderer (with default material) if missing on the provided Game Object.
        /// </summary>
        /// <param name="targetObject"></param>
        public static void SetupSplineRenderer(GameObject targetObject)
        {
            MeshFilter meshFilter = targetObject.GetComponent<MeshFilter>();
            if (meshFilter == null)
            {
                meshFilter = targetObject.AddComponent<MeshFilter>();
                Undo.RegisterCreatedObjectUndo(meshFilter, $"Setup Spline Mesher on {targetObject.name}");
            }
            
            MeshRenderer meshRenderer = targetObject.GetComponent<MeshRenderer>();
            if (meshRenderer == null)
            {
                meshRenderer = targetObject.AddComponent<MeshRenderer>();
                Undo.RegisterCreatedObjectUndo(meshRenderer, $"Setup Spline Mesher on {targetObject.name}");

                meshRenderer.sharedMaterial = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline 
                    ? UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline.defaultMaterial :
                    AssetDatabase.GetBuiltinExtraResource<Material>("Default-Material.mat");
            }
        }

        public static int RebuildAllInstances()
        {
            SplineMesher[] splineMeshers = Object.FindObjectsByType<SplineMesher>(FindObjectsSortMode.None);

            int count = 0;
            for (int i = 0; i < splineMeshers.Length; i++)
            {
                count++;
                splineMeshers[i].Rebuild();
            }
            return count;
        }

        public static int RebuildAllInstancesUsingMesh(Mesh sourceMesh)
        {
            SplineMesher[] splineMeshers = Object.FindObjectsByType<SplineMesher>(FindObjectsSortMode.None);

            int count = 0;
            for (int i = 0; i < splineMeshers.Length; i++)
            {
                if(splineMeshers[i].sourceMesh == null) continue;

                if (splineMeshers[i].sourceMesh == sourceMesh)
                {
                    splineMeshers[i].Rebuild();
                    count++;
                }
            }
            
            Debug.Log($"Rebuilt {count} Spline Mesher components using the \"{sourceMesh.name}\" mesh.");
            
            return count;
        }
        #endregion
        
        public static class FBX
        {
            /// <summary>
            /// Export any sort of meshes contained in the object, and load+reference them back to it
            /// </summary>
            /// <param name="mesher"></param>
            /// <param name="filePath"></param>
            /// <exception cref="Exception">Empty file path, or missing the FBX extension</exception>
            public static void SaveToFileAndReference(SplineMesher mesher, string filePath)
            {
                #if FBX_EXPORTER
                if (filePath == string.Empty)
                {
                    throw new Exception("Failed to save mesh(es) to an FBX file, file path is empty");
                }
                if (filePath.EndsWith(".fbx") == false)
                {
                    throw new Exception("Failed to save mesh(es) to an FBX file, file path must end with \".fbx\"");
                }

                GameObject gameObject = mesher.gameObject;

                MeshCollider collider = mesher.GetComponent<MeshCollider>();
                GameObject colliderObj = new GameObject();
                
                bool hasCollision = collider;
                if (hasCollision)
                {
                    //In order for the exporter to also export the collision mesh, it needs to be on a separate object
                    collider.transform.parent = mesher.transform;
                    collider.transform.localPosition = Vector3.zero;

                    MeshCollider tempCollider = colliderObj.AddComponent<MeshCollider>();
                    tempCollider.sharedMesh = collider.sharedMesh;
                    
                    MeshFilter tempMF = colliderObj.AddComponent<MeshFilter>();
                    tempMF.sharedMesh = collider.sharedMesh;
                }

                //Export
                ModelExporter.ExportObject(filePath, mesher);

                //Delete temp collider
                if (hasCollision)
                {
                    Object.DestroyImmediate(colliderObj);
                }
                
                //Import
                AssetDatabase.ImportAsset(filePath, ImportAssetOptions.ForceSynchronousImport);

                //Loading meshes and assigning them to MeshFilters
                Object[] subAssets = AssetDatabase.LoadAllAssetsAtPath(filePath);

                MeshFilter meshFilter = mesher.GetComponent<MeshFilter>();

                if (meshFilter)
                {
                    for (int i = 1; i < subAssets.Length; i++) //Skip the first, as it is the parent
                    {
                        if (subAssets[i] is Mesh)
                        {
                            if (hasCollision && subAssets[i].name.EndsWith("Collider"))
                            {
                                collider.sharedMesh = subAssets[i] as Mesh;
                                continue;
                            }
                            
                            meshFilter.sharedMesh = subAssets[i] as Mesh;
                        }
                    }
                }
                
                #endif
            }
        }
        
        public static class Preferences
        {
            public const string PreferencesPath = "Preferences/Splines/Spline Mesher";

            public enum SectionStyle
            {
                Accordion,
                Foldouts
            }
            public static SectionStyle SectionStyleMode
            {
                get => (SectionStyle)EditorPrefs.GetInt(PlayerSettings.productName + "_SM_SectionStyle", (int)SectionStyle.Accordion);
                set => EditorPrefs.SetInt(PlayerSettings.productName + "_SM_SectionStyle", (int)value);
            }
            public static bool RebuildEveryFrame
            {
                get => EditorPrefs.GetBool(PlayerSettings.productName + "_SM_RebuildEveryFrame", false);
                set => EditorPrefs.SetBool(PlayerSettings.productName + "_SM_RebuildEveryFrame", value);
            }
            public static bool AutoGeneratedLightmapUV
            {
                get => EditorPrefs.GetBool(PlayerSettings.productName + "_SM_AutoGeneratedLightmapUV", true);
                set => EditorPrefs.SetBool(PlayerSettings.productName + "_SM_AutoGeneratedLightmapUV", value);
            }
            
            [SettingsProvider]
            public static SettingsProvider PreferencesProvider()
            {
                var provider = new SettingsProvider(PreferencesPath, SettingsScope.User)
                {
                    label = $"Spline Mesher Standard",
                    guiHandler = (searchContent) =>
                    {
                        EditorGUILayout.LabelField($"v{AssetInfo.VERSION}", EditorStyles.miniLabel);
                        EditorGUILayout.Space();

                        SectionStyleMode = (SectionStyle)EditorGUILayout.EnumPopup("Inspector section style", SectionStyleMode, GUILayout.Width(EditorGUIUtility.labelWidth + 180f));
                        EditorGUILayout.HelpBox("• Accordion: Allows one section to expand at a time\n" +
                                                        "• Foldouts: Manually expand/collapse sections", MessageType.Info);
                        
                        EditorGUILayout.Space();
                        
                        RebuildEveryFrame = EditorGUILayout.Toggle("Rebuild every frame", RebuildEveryFrame);
                        EditorGUILayout.HelpBox("When disabled, the spline mesh is rebuild at a lower frame rate when changing parameters in the Spline Mesh inspector. This yields better editor performance." +
                                                "\n\nWhen enabled, rebuilding occurs every time the UI repaints (much smoother)", MessageType.Info);
                        
                        EditorGUILayout.Space();
                        
                        AutoGeneratedLightmapUV = EditorGUILayout.Toggle("Lightmap UV generation", AutoGeneratedLightmapUV);
                        EditorGUILayout.HelpBox("When light baking starts, check all (static) Spline Mesher components in the scene and auto-generate lightmap UVs for the created meshes.", MessageType.Info);

                        EditorGUILayout.Space();

                        HelpWindow.DrawReviewButton();
                        
                        EditorGUILayout.Space();
                        
                        HelpWindow.DrawProInfo();

                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.FlexibleSpace();
                            if (GUILayout.Button("Rebuild all instances in scene"))
                            {
                                int count = RebuildAllInstances();
                                Debug.Log($"Rebuilt {count} Spline Mesher component instances.");
                            }
                        }
                    },

                    keywords = new HashSet<string>(new[] { "Spline", "Spline Mesh" })
                };
                
                return provider;
            }
        }
    }
}