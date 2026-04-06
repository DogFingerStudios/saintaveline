// Staggart Creations (http://staggart.xyz)
// Copyright protected under Unity Asset Store EULA
// Copying or referencing source code for the production of new asset store, or public content, is strictly prohibited!

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
using System.Collections.Generic;
using UnityEngine.Splines;
using UnityEditor.Splines;
#endif

namespace sc.modeling.splines.editor
{
    [EditorTool("Spline Mesh Conforming", typeof(SplineMesher))]
    sealed class ConformingTool : EditorTool
    {
        #if SPLINES
        GUIContent m_IconContent;
        public override GUIContent toolbarIcon => m_IconContent;
        
        private bool m_DisableHandles = false;
        private const float SLIDER_WIDTH = 150f;
        
        static readonly Color headerBackgroundDark = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        static readonly Color headerBackgroundLight = new Color(1f, 1f, 1f, 0.9f);
        public static Color headerBackground => EditorGUIUtility.isProSkin ? headerBackgroundDark : headerBackgroundLight;
        
        public static Texture2D LoadIcon()
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"{SplineMesher.kPackageRoot}/Editor/Resources/spline-mesher-conforming-icon-64px.psd");
        }

        void OnEnable()
        {
            m_IconContent = new GUIContent
            {
                image = LoadIcon(),
                tooltip = "Adjust the mesh's conforming strength along the spline"
            };
        }

        bool GetTargets(out SplineMesher splineMesher, out SplineContainer spline)
        {
            splineMesher = target as SplineMesher;
            if (splineMesher != null)
            {
                spline = splineMesher.splineContainer as SplineContainer;
                return spline != null && spline.Spline != null;
            }
            spline = null;
            return false;
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
                
                var splines = modeler.splineContainer.Splines;
                for (var i = 0; i < splines.Count; i++)
                {
                    if (i < modeler.conformingStrength.Count)
                    {
                        NativeSpline nativeSpline = new NativeSpline(splines[i], modeler.splineContainer.transform.localToWorldMatrix);

                        Undo.RecordObject(modeler, "Modifying Mesh Conforming");
                        
                        // User defined handles to manipulate width
                        DrawDataPoints(nativeSpline, modeler.conformingStrength[i]);

                        nativeSpline.DataPointHandles<ISpline, float>(modeler.conformingStrength[i], true, i);
                    
                        if (GUI.changed)
                        {
                            modeler.Rebuild();
                        }
                    }
                }
            }
        }
        
        private bool DrawDataPoints(ISpline spline, SplineData<float> splineData)
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
        
        private bool DrawDataPoint(Vector3 position, Vector3 tangent, Vector3 up, float inValue, out float outValue)
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
                    Rect bgRect = new Rect(screenPos.x - (SLIDER_WIDTH * 0.5f) - boxPadding, screenPos.y - 52f, SLIDER_WIDTH + boxPadding, 28);
                    EditorGUI.DrawRect(bgRect, headerBackground);
                    
                    Rect sliderRect = new Rect(screenPos.x - (SLIDER_WIDTH * 0.5f), screenPos.y - 50f, SLIDER_WIDTH - boxPadding, 22f);

                    outValue = EditorGUI.Slider(sliderRect, inValue, 0f, 1f);
                    outValue = math.clamp(outValue, 0f, 1f);
    
                    Handles.EndGUI();
                }
            }

            if (inValue != outValue)
            {
                //return true;
            }

            if (EditorGUI.EndChangeCheck()) return true;

            return false;
        }
        
        public static void DrawLabel(Vector3 position, string text)
        {
            var labelOffset = HandleUtility.GetHandleSize(position) / 1.5f;
            
            Handles.Label(position + new Vector3(0, -labelOffset, 0), text, Label);
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
                            left = 5,
                            right = 0,
                            top = 0,
                            bottom = 0
                        }
                    };
                    
                    _Label.normal.textColor = Color.black; // Set the text color to black
                    _Label.normal.background = Texture2D.whiteTexture;
                }

                return _Label;
            }
        }
        #endif
    }
}