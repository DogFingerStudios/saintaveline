// Staggart Creations (http://staggart.xyz)
// Copyright protected under Unity Asset Store EULA
// Copying or referencing source code for the production of new asset store, or public content, is strictly prohibited!

using System;
using sc.modeling.splines.runtime;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.UIElements;

#if MATHEMATICS
using Unity.Mathematics;
using UnityEditor.Overlays;
using UnityEngine;
#endif

#if SPLINES
using UnityEngine.Splines;
using UnityEditor.Splines;
#endif

namespace sc.modeling.splines.editor
{
    [EditorTool("Spline Mesh Scale", typeof(SplineMesher))]
    public class ScaleTool : EditorTool
    {
        #if SPLINES && MATHEMATICS
        private GUIContent m_IconContent;
        public override GUIContent toolbarIcon => m_IconContent;
        private IDrawSelectedHandles drawSelectedHandlesImplementation;
        protected const float k_MinSliderSize = 1.35f;
        private const float k_HandleSize = 0.1f;
        private bool m_DisableHandles = false;

        private ScaleToolUI ui;
        
        public static Texture2D LoadIcon()
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"{SplineMesher.kPackageRoot}/Editor/Resources/spline-mesher-scale-icon-64px.psd");
        }
        
        public static bool UniformScaling
        {
            get => EditorPrefs.GetBool("SM_SCALE_UNIFORM", true);
            set => EditorPrefs.SetBool("SM_SCALE_UNIFORM", value);
        }
        
        void OnEnable()
        {
            name = "Spline Mesh Scale";
            m_IconContent = new GUIContent()
            {
                image = LoadIcon(),
                text = "Spline Mesh Scale Tool",
                tooltip = "Adjust the scale of the created spline mesh."
            };
        }

        public override void OnActivated()
        {
            #if UNITY_2022_1_OR_NEWER
            SceneView.AddOverlayToActiveView(ui = new ScaleToolUI());
            #endif
            
            ScaleToolUI.Show = true;
            
            foreach (var m_target in targets)
            {
                SplineMesher modeler = m_target as SplineMesher;

                if (modeler == null || modeler.splineContainer == null)
                    return;

                modeler.ValidateData();

                for (int i = 0; i < modeler.scaleData.Count; i++)
                {
                    for (int j = 0; j < modeler.scaleData[i].Count; j++)
                    {
                        if (math.length(modeler.scaleData[i][j].Value) < 0.01f)
                        {
                            Debug.LogError($"{modeler.name} has a Scale data point for Spline #{i} with a value of (0,0,0). This creates invalid geometry. It has been reset to (1,1,1)");

                            DataPoint<float3> p = modeler.scaleData[i][j];
                            p.Value = new float3(1f);
                            modeler.scaleData[i][j] = p;

                            EditorUtility.SetDirty(modeler);
                        }
                    }
                }
            }
        }

        public override void OnWillBeDeactivated()
        {
            #if UNITY_2022_1_OR_NEWER
            SceneView.RemoveOverlayFromActiveView(ui);
            #endif
            
            ScaleToolUI.Show = false;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            foreach (var m_target in targets)
            {
                var modeler = m_target as SplineMesher;
                if (modeler == null || modeler.splineContainer == null)
                    return;

                base.OnToolGUI(window);

                Handles.color = Color.yellow;
                m_DisableHandles = false;

                var splines = modeler.splineContainer.Splines;
                for (var i = 0; i < splines.Count; i++)
                {
                    if (i < modeler.scaleData.Count)
                    {
                        NativeSpline nativeSpline = new NativeSpline(splines[i], modeler.splineContainer.transform.localToWorldMatrix);

                        Undo.RecordObject(modeler, "Modifying Mesh Scale");

                        // User defined handles to manipulate width
                        DrawDataPoints(nativeSpline, modeler.scaleData[i]);

                        // Using the out-of the box behaviour to manipulate indexes
                        nativeSpline.DataPointHandles(modeler.scaleData[i], true, i);

                        if (GUI.changed)
                        {
                            modeler.Rebuild();
                            modeler.UpdateCaps();
                        }
                    }
                }
            }
        }
        
        private bool DrawDataPoints(ISpline spline, SplineData<float3> splineData)
        {
            SplineMesher modeler = target as SplineMesher;

            var inUse = false;
            for (int dataFrameIndex = 0; dataFrameIndex < splineData.Count; dataFrameIndex++)
            {
                var dataPoint = splineData[dataFrameIndex];

                var normalizedT = SplineUtility.GetNormalizedInterpolation(spline, dataPoint.Index, splineData.PathIndexUnit);
                spline.Evaluate(normalizedT, out var position, out var tangent, out var up);

                if (DrawDataPoint(position, tangent, up, dataPoint.Value, out var result))
                {
                    dataPoint.Value = result;
                    splineData[dataFrameIndex] = dataPoint;
                    inUse = true;
                    
                    modeler.Rebuild();
                }
            }
            return inUse;
        }
        
        private bool DrawDataPoint(Vector3 position, Vector3 tangent, Vector3 up, float3 inValue, out float3 outValue)
        {
            int id = m_DisableHandles ? -1 : GUIUtility.GetControlID(FocusType.Passive);
            int id2 = m_DisableHandles ? -1 : GUIUtility.GetControlID(FocusType.Passive);

            outValue = inValue;
            if (tangent == Vector3.zero)
                return false;

            if (Event.current.type == EventType.MouseUp
                && Event.current.button != 0
                && (GUIUtility.hotControl == id || GUIUtility.hotControl == id2))
            {
                Event.current.Use();
                return false;
            }

            var handleColor = Handles.color;
            if ((GUIUtility.hotControl == id || GUIUtility.hotControl == id2))
                handleColor = Handles.selectedColor;
            else if (GUIUtility.hotControl == 0 && (HandleUtility.nearestControl == id || HandleUtility.nearestControl == id2))
                handleColor = Handles.preselectionColor;

            var splineDataTarget = target as SplineMesher;

            up = math.up();
            Vector3 right = math.normalize(math.cross(tangent, up));

            float handleScale = HandleUtility.GetHandleSize(position);

            /*
            Vector3 scale = inValue;
            quaternion rotation = quaternion.LookRotationSafe(tangent, up);

            using (new Handles.DrawingScope(handleColor))
            {
                scale = Handles.ScaleHandle(inValue, position, rotation, k_HandleSize * handleScale * 10);
            }
            
            if (GUIUtility.hotControl == id && math.abs(scale.magnitude - math.length(inValue)) > 0f)
            {
                outValue = scale / handleScale;
                return true;
            }
            */
            
            Vector3 x = position - (right * inValue.x * handleScale);
            Vector3 y = position + (up * inValue.y * handleScale);

            Vector3 width, height;

            using (new Handles.DrawingScope(handleColor))
            {
                Handles.color = Color.red;
                if (Event.current.type == EventType.Repaint)
                {
                    Handles.DrawAAPolyLine(Texture2D.whiteTexture, 3f, new []{position, x});
                }
                width = Handles.Slider(id, x, right, k_HandleSize * handleScale, CustomHandleCap, 0);

                Handles.color = Color.green;
                if (Event.current.type == EventType.Repaint)
                {
                    Handles.DrawAAPolyLine(Texture2D.whiteTexture, 3f, new []{position, y});
                }
                height = Handles.Slider(id2, y, up, k_HandleSize * handleScale, CustomHandleCap, 0);

                if (GUIUtility.hotControl == id)
                {
                    RollTool.DrawLabel(position, $"X: {Math.Round(inValue.x, 4)}");
                }
                if (GUIUtility.hotControl == id2)
                {
                    RollTool.DrawLabel(position, $"Y: {Math.Round(inValue.y, 4)}");
                }
            }

            if (GUIUtility.hotControl == id && math.abs(width.x - x.x) > 0f)
            {
                outValue.x = math.distance(width, position) / handleScale;
                if (UniformScaling) outValue.y = outValue.x;
                
                return true;
            }

            if (GUIUtility.hotControl == id2 && math.abs(height.y - y.y) > 0f)
            {
                outValue.y = math.distance(height, position) / handleScale;
                if (UniformScaling) outValue.x = outValue.y;

                return true;
            }

            return false;
        }
        
        private void CustomHandleCap(int controlID, Vector3 position, Quaternion rotation, float size, EventType eventType)
        {
            if (m_DisableHandles) // If disabled, do nothing unless it's a repaint event
            {
                if (Event.current.type == EventType.Repaint)
                    Handles.CubeHandleCap(controlID, position, rotation, size, eventType);
            }
            else
                Handles.CubeHandleCap(controlID, position, rotation, size, eventType);
        }
        #endif
    }

    #if SPLINES && MATHEMATICS
    #if UNITY_2022_1_OR_NEWER
    [Overlay(defaultDisplay = true)]
    #else
    [Overlay(typeof(SceneView), "Spline Mesh Scale Tool")]
    #endif
    public class ScaleToolUI : Overlay, ITransientOverlay
    {
        public static bool Show;
        public bool visible => Show;
        
        public override VisualElement CreatePanelContent()
        {
            var root = new VisualElement();
            
            this.displayName = "Spline Scale Tool";
            
            Toggle uniformScaling = new Toggle("Uniform scaling")
            {
                value = ScaleTool.UniformScaling,
                tooltip = "Use any of the handles to uniformly scale the data point"
            };
            uniformScaling.RegisterValueChangedCallback(evt => { ScaleTool.UniformScaling = evt.newValue; });
            
            root.Add(uniformScaling);
            
            return root;
        }
    }
    #endif
}