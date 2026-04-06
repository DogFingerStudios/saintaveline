// Staggart Creations (http://staggart.xyz)
// Copyright protected under Unity Asset Store EULA
// Copying or referencing source code for the production of new asset store, or public content, is strictly prohibited!

using System;
using System.Collections.Generic;
using sc.modeling.splines.runtime;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.UIElements;
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
    [EditorTool("Spline Mesh Vertex Color", typeof(SplineMesher))]
    public class VertexColorTool : EditorTool
    {
        #if SPLINES
        GUIContent m_IconContent;
        public override GUIContent toolbarIcon => m_IconContent;

        protected bool m_DisableHandles = false;
        protected const float SLIDER_WIDTH = 150f;
        
        static readonly Color headerBackgroundDark = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        static readonly Color headerBackgroundLight = new Color(1f, 1f, 1f, 0.9f);
        public static Color headerBackground => EditorGUIUtility.isProSkin ? headerBackgroundDark : headerBackgroundLight;

        public enum Channel
        {
            Red,
            Green,
            Blue,
            Alpha
        }
        private static Channel targetChannel
        {
            get => (Channel)SessionState.GetInt(PlayerSettings.productName + "_SM_targetChannel", (int)Channel.Red);
            set => SessionState.SetInt(PlayerSettings.productName + "_SM_targetChannel", (int)value);
        }
        
        public enum VisualizationChannel
        {
            None,
            Current,
            All
        }
        private static VisualizationChannel visualizationChannel
        {
            get => (VisualizationChannel)SessionState.GetInt(PlayerSettings.productName + "_SM_visualizationChannel", (int)VisualizationChannel.Current);
            set => SessionState.SetInt(PlayerSettings.productName + "_SM_visualizationChannel", (int)value);
        }
        
        private VertexColorToolUI ui;
        private Material vertexColorMaterial;
        
        public static Texture2D LoadIcon()
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"{SplineMesher.kPackageRoot}/Editor/Resources/spline-mesher-color-icon-64px.psd");
        }

        void OnEnable()
        {
            m_IconContent = new GUIContent
            {
                image = LoadIcon(),
                tooltip = "Adjust the mesh's vertex color along the spline"
            };

            Shader vertexColorShader = Resources.Load<Shader>("VisualizeVertexAttributes");

            if (!vertexColorShader)
            {
                throw new Exception("[Spline Mesher] Could not locate the vertex color shader, was it deleted or not imported?");
            }
            vertexColorMaterial = new Material(vertexColorShader);
        }

        public override void OnActivated()
        {
            #if UNITY_2022_1_OR_NEWER
            SceneView.AddOverlayToActiveView(ui = new VertexColorToolUI());
            #endif

            VertexColorToolUI.Show = true;

        }

        private Color GetColor()
        {
            switch (targetChannel)
            {
                case Channel.Red: return Color.red;
                case Channel.Green: return Color.green;
                case Channel.Blue: return Color.blue;
                case Channel.Alpha: return Color.white;
                default: return Color.white;
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            foreach (var m_target in targets)
            {
                SplineMesher splineMesher = (SplineMesher)m_target;
                
                if (splineMesher == null || splineMesher.splineContainer == null)
                    continue;
                
                if (visualizationChannel != VisualizationChannel.None)
                {
                    int channel = -1;
                    bool transparent = false;

                    if (visualizationChannel == VisualizationChannel.All)
                    {
                        channel = 4;
                        transparent = true;
                    }
                    else channel = (int)targetChannel;
                    
                    vertexColorMaterial.EnableKeyword("_DISPLAY_COLOR");
                    vertexColorMaterial.SetFloat("_ColorChannel", channel);
                    vertexColorMaterial.SetFloat("_Transparent", transparent ? 1 : 0);
                    vertexColorMaterial.SetPass(0);

                    if (splineMesher.outputObject)
                    {
                        MeshFilter mf = splineMesher.outputObject.GetComponent<MeshFilter>();

                        if (mf) Graphics.DrawMeshNow(mf.sharedMesh, mf.transform.localToWorldMatrix);
                    }
                }
                
                var splines = splineMesher.splineContainer.Splines;

                List<SplineData<SplineMesher.VertexColorChannel>> data = null;

                switch (targetChannel)
                {
                    case Channel.Red: data = splineMesher.vertexColorRedData;
                        break;
                    case Channel.Green: data = splineMesher.vertexColorGreenData;
                        break;
                    case Channel.Blue: data = splineMesher.vertexColorBlueData;
                        break;
                    case Channel.Alpha: data = splineMesher.vertexColorAlphaData;
                        break;
                }

                Handles.color = GetColor();
                
                for (var i = 0; i < splines.Count; i++)
                {
                    if (i < data.Count)
                    {
                        var nativeSpline = new NativeSpline(splines[i], splineMesher.splineContainer.transform.localToWorldMatrix);

                        Undo.RecordObject(splineMesher, "Modifying Spline Mesh vertex color");

                        // User defined handles to manipulate width
                        DrawDataPoints(nativeSpline, data[i]);

                        nativeSpline.DataPointHandles<ISpline, SplineMesher.VertexColorChannel>(data[i], true, i);
                    
                        if (GUI.changed)
                        {
                            splineMesher.Rebuild();
                        }
                    }
                }
            }
        }
        
        protected bool DrawDataPoints(ISpline spline, SplineData<SplineMesher.VertexColorChannel> splineData)
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

        private const float boxPadding = 5f;
        
        protected bool DrawDataPoint(Vector3 position, Vector3 tangent, Vector3 up, SplineMesher.VertexColorChannel inValue, out SplineMesher.VertexColorChannel outValue)
        {
            int id = m_DisableHandles ? -1 : GUIUtility.GetControlID(FocusType.Passive);

            outValue = inValue;
            
            if (tangent == Vector3.zero) return false;

            if (Event.current.type == EventType.MouseUp && Event.current.button != 0 && (GUIUtility.hotControl == id))
            {
                Event.current.Use();
                return false;
            }

            var handleColor = Handles.color;
            if (GUIUtility.hotControl == id)
                handleColor = Handles.selectedColor;
            else if (GUIUtility.hotControl == 0 && (HandleUtility.nearestControl == id))
                handleColor = Handles.preselectionColor;

            var right = math.normalize(math.cross(tangent, up));

            EditorGUI.BeginChangeCheck();
            //if (GUIUtility.hotControl == id)
            {
                using (new Handles.DrawingScope(handleColor))
                {
                    Handles.BeginGUI();

                    Vector2 screenPos = HandleUtility.WorldToGUIPoint(position);
                    Rect bgRect = new Rect(screenPos.x - (SLIDER_WIDTH * 0.5f) - boxPadding, screenPos.y - 80f, SLIDER_WIDTH + boxPadding, 55);
                    EditorGUI.DrawRect(bgRect, headerBackground);
                    
                    Rect sliderRect = new Rect(screenPos.x - (SLIDER_WIDTH * 0.5f), screenPos.y - 50f, SLIDER_WIDTH - boxPadding, 22f);

                    outValue.value = EditorGUI.Slider(sliderRect, inValue.value, -1f, 1f);
                    outValue.value = math.clamp(outValue.value, -1f, 1f);
                    
                    sliderRect.y -= 27f;
                    outValue.blend = EditorGUI.ToggleLeft(sliderRect, new GUIContent("Blend", "Blend the value with the original vertex color value"), inValue.blend);
                    
                    Handles.EndGUI();
                }
            }

            if (inValue.value != outValue.value)
            {
                //return true;
            }

            if (EditorGUI.EndChangeCheck()) return true;

            return false;
        }

        public override void OnWillBeDeactivated()
        {
            #if UNITY_2022_1_OR_NEWER
            SceneView.RemoveOverlayFromActiveView(ui);
            #endif
            
            VertexColorToolUI.Show = false;
        }
        
        #if UNITY_2022_1_OR_NEWER
        [Overlay(defaultDisplay = true)]
        #else
        [Overlay(typeof(SceneView), "Spline Mesh Vertex Tool")]
        #endif
        public class VertexColorToolUI : Overlay, ITransientOverlay
        {
            public static bool Show;
            public bool visible => Show;

            public override VisualElement CreatePanelContent()
            {
                this.displayName = "Vertex Colors";
                
                var root = new VisualElement();
                
                EnumField targetChannel = new EnumField("Channel")
                {
                    value = VertexColorTool.targetChannel,
                    tooltip = "Select which vertex color channel to edit"
                };
                targetChannel.Init(targetChannel.value);
                targetChannel.RegisterValueChangedCallback(evt => { VertexColorTool.targetChannel = (Channel)evt.newValue; } );

                root.Add(targetChannel);

                EnumField vizChannel = new EnumField("Visualize")
                {
                    value = VertexColorTool.visualizationChannel,
                    tooltip = "Draw the spline mesh with its vertex color value visualized"
                };
                vizChannel.Init(vizChannel.value);
                vizChannel.RegisterValueChangedCallback(evt => { VertexColorTool.visualizationChannel = (VisualizationChannel)evt.newValue; } );

                root.Add(vizChannel);

                return root;
            }
        }
        #endif
    }
}