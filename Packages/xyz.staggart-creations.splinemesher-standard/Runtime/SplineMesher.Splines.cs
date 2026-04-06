// Staggart Creations (http://staggart.xyz)
// Copyright protected under Unity Asset Store EULA
// Copying or referencing source code for the production of new asset store, or public content, is strictly prohibited!

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if MATHEMATICS
using Unity.Mathematics;
#endif
#if SPLINES
using UnityEngine.Splines;
#endif

namespace sc.modeling.splines.runtime
{
    public partial class SplineMesher
    {
        #if SPLINES
        /// <summary>
        /// Geometry will be created from splines within this container.
        /// </summary>
        public SplineContainer splineContainer;
        [SerializeField] [ HideInInspector]
        private int splineCount; //Change tracking
        
        public enum SplineChangeReaction
        {
            [InspectorName("During Changes")]
            During,
            [InspectorName("After Changes")]
            WhenDone,
        }
        [Tooltip("Determines when a change to the spline should be detected. Using the After Changes option for complex set ups to improve performance.")]
        public SplineChangeReaction splineChangeMode = SplineChangeReaction.During;
        
        //[HideInInspector]
        /// <summary>
        /// Scale information for each spline within the container. <seealso cref="SampleScale(float,int)"/>
        /// </summary>
        public List<SplineData<float3>> scaleData = new List<SplineData<float3>>();
        /// <summary>
        /// Roll (angle in degrees) information for each spline within the container. <seealso cref="SampleRoll(float,int)"/>
        /// </summary>
        public List<SplineData<float>> rollData = new List<SplineData<float>>();
        /// <summary>
        /// Normalized conforming strength information for each spline within the container. <seealso cref="SampleRoll(float,int)"/>
        /// </summary>
        public List<SplineData<float>> conformingStrength = new List<SplineData<float>>();
        
        /// <summary>Structure to hold vertex color value for a specific location on the track.</summary>
        [Serializable]
        public struct VertexColorChannel
        {
            public float value;
            public bool blend;

            ///<summary>Implicit conversion to float</summary>
            public static implicit operator float(VertexColorChannel value) => value.value;
            ///<summary>Implicit conversion from float</summary>
            public static implicit operator VertexColorChannel(float value) => new () { value = value };
            
            public struct LerpVertexColorData : IInterpolator<VertexColorChannel>
            {
                //Represents the vertex color channel value of the original mesh
                private readonly float baseValue;
                
                public LerpVertexColorData(float baseValue)
                {
                    this.baseValue = baseValue;
                }
                
                public VertexColorChannel Interpolate(VertexColorChannel a, VertexColorChannel b, float t)
                {
                    //Values blending with base value
                    float aValue = BlendVertexColorChannel(a, baseValue);
                    float bValue = BlendVertexColorChannel(b, baseValue);
                    
                    return Mathf.Lerp(aValue, bValue, t);
                }
            }
        }
        
        public List<SplineData<VertexColorChannel>> vertexColorRedData = new List<SplineData<VertexColorChannel>>();
        public List<SplineData<VertexColorChannel>> vertexColorGreenData = new List<SplineData<VertexColorChannel>>();
        public List<SplineData<VertexColorChannel>> vertexColorBlueData = new List<SplineData<VertexColorChannel>>();
        public List<SplineData<VertexColorChannel>> vertexColorAlphaData = new List<SplineData<VertexColorChannel>>();

        private static float BlendVertexColorChannel(VertexColorChannel data, float baseValue)
        {
            float output = baseValue;
            
            if (data.blend)
            {
                output += data.value;
            }
            else
            {
                output = data.value;
            }
            
            return output;
        }
        
        public struct Float3Interpolator : IInterpolator<float3>
        {
            public Settings.InterpolationType mode;
            
            public float3 Interpolate(float3 a, float3 b, float t)
            {
                if (mode == Settings.InterpolationType.Linear)
                {
                    return math.lerp(a, b, t);
                }
                else if (mode == Settings.InterpolationType.EaseInEaseOut)
                {
                    float EaseInOut()
                    {
                        float eased = 2f * t * t;
                        if (t > 0.5f) eased = 4f * t - eased - 1f;
                            
                        return eased;
                    }

                    return math.lerp(a, b, EaseInOut());
                }

                return a;
            }
        }
        public static Float3Interpolator scaleInterpolator = new Float3Interpolator();
        
        private partial void SubscribeSplineCallbacks()
        {
            #if MATHEMATICS
            SplineContainer.SplineAdded += OnSplineAdded;
            SplineContainer.SplineRemoved += OnSplineRemoved;
            Spline.Changed += OnSplineChanged;
            #endif
        }
        
        private partial void UnsubscribeSplineCallbacks()
        {
            #if MATHEMATICS
            SplineContainer.SplineAdded -= OnSplineAdded;
            SplineContainer.SplineRemoved -= OnSplineRemoved;
            Spline.Changed -= OnSplineChanged;
            #endif
        }
        
        private Spline lastEditedSpline;
        private int lastEditedSplineIndex = -1;
        
        private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modificationType)
        {
            if (!splineContainer) return;

            if (rebuildTriggers.HasFlag(RebuildTriggers.OnSplineChanged) == false) return;

            //Spline belongs to the assigned container?
            var splineIndex = Array.IndexOf(splineContainer.Splines.ToArray(), spline);
            if (splineIndex < 0)
                return;

            splineCount = splineContainer.Splines.Count;
            
            lastEditedSpline = spline;
            lastEditedSplineIndex = splineIndex;
            
            if (splineChangeMode == SplineChangeReaction.WhenDone)
            {
                lastChangeTime = Time.realtimeSinceStartup;

                if (Application.isPlaying)
                {
                    //Coroutines only work in play mode and builds
                    
                    //Cancel any existing debounce coroutine
                    if (debounceCoroutine != null) StopCoroutine(debounceCoroutine);
                
                    debounceCoroutine = StartCoroutine(DebounceCoroutine());
                }
                else
                {
                    if (!isTrackingChanges)
                    {
                        isTrackingChanges = true;
                        
                        #if UNITY_EDITOR
                        UnityEditor.EditorApplication.update += EditorUpdate;
                        #endif
                    }
                    
                }
            }
            else if (splineChangeMode == SplineChangeReaction.During)
            {
                ExecuteAfterSplineChanges();
            }
        }
        
        public float debounceTime = 0.1f;

        private float lastChangeTime = -1f;
        private bool isTrackingChanges = false;
        
        private void EditorUpdate()
        {
            if (isTrackingChanges && Time.realtimeSinceStartup - lastChangeTime >= debounceTime)
            {
                ExecuteAfterSplineChanges();
                
                isTrackingChanges = false;
                
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.update -= EditorUpdate;
                #endif
            }
        }
        
        private Coroutine debounceCoroutine;
        private IEnumerator DebounceCoroutine()
        {
            yield return new WaitForSeconds(debounceTime);
            
            ExecuteAfterSplineChanges();
        }

        private void ExecuteAfterSplineChanges()
        {
            if(lastEditedSplineIndex < 0) return;
            
            Rebuild();

            UpdateCaps();
        }
        
        private void OnSplineAdded(SplineContainer container, int index)
        {
            if (!splineContainer) return;
            
            if (rebuildTriggers.HasFlag(RebuildTriggers.OnSplineAdded) == false) return;

            if (container.GetHashCode() != splineContainer.GetHashCode())
                return;
            
            splineCount = splineContainer.Splines.Count;

            Rebuild();
        }

        private void OnSplineRemoved(SplineContainer container, int index)
        {
            if (!splineContainer) return;

            if (rebuildTriggers.HasFlag(RebuildTriggers.OnSplineRemoved) == false) return;

            if (container != splineContainer)
                return;

            splineCount = splineContainer.Splines.Count;
            
            #if UNITY_EDITOR
            UnityEditor.Undo.RecordObject(this, "Deleting Spline Mesh data");
            #endif
            
            if (index < scaleData.Count) scaleData.RemoveAt(index);
            if (index < rollData.Count) rollData.RemoveAt(index);
            if (index < conformingStrength.Count) conformingStrength.RemoveAt(index);

            if (index < vertexColorRedData.Count) vertexColorRedData.RemoveAt(index);
            if (index < vertexColorGreenData.Count) vertexColorGreenData.RemoveAt(index);
            if (index < vertexColorBlueData.Count) vertexColorBlueData.RemoveAt(index);
            if (index < vertexColorAlphaData.Count) vertexColorAlphaData.RemoveAt(index);
            
            Rebuild();
        }
        
        /// <summary>
        /// Clears the scale data for every spline. If no scale data is found for a spline, a default value is used.
        /// </summary>
        public void ResetScaleData()
        {
            if (!splineContainer) return;
            
            scaleData.Clear();
            ValidateData();
            
            Rebuild();
        }
        
        /// <summary>
        /// Clears the roll data for every spline.
        /// </summary>
        public void ResetRollData()
        {
            if (!splineContainer) return;
            
            rollData.Clear();
            ValidateData();
            
            Rebuild();
        }
        
        public void ResetConformingData()
        {
            if (!splineContainer) return;
            
            conformingStrength.Clear();
            ValidateData();
            
            Rebuild();
        }

        /// <summary>
        /// Clears the roll data for every spline.
        /// </summary>
        public void ResetVertexColorData()
        {
            if (!splineContainer) return;
            
            vertexColorRedData.Clear();
            vertexColorGreenData.Clear();
            vertexColorBlueData.Clear();
            vertexColorAlphaData.Clear();
            
            ValidateData();
            
            Rebuild();
        }
        
        public void ReverseSpline()
        {
            if (!splineContainer) return;
            
            for (int s = 0; s < splineContainer.Splines.Count; s++)
            {
                SplineUtility.ReverseFlow(splineContainer.Splines[s]);
            }
        }

        //It is important to ensure that for every spline in the container a data set is present.
        //Subsequently, changes to the path index unit also require conversion
        public void ValidateData()
        {
            if (!splineContainer) return;
            
            #if MATHEMATICS
            splineCount = splineContainer.Splines.Count;
            
            ValidateScaleData();
            ValidateRollData();
            ValidateConformingData();
            
            ValidateVertexColorData(ref vertexColorRedData);
            ValidateVertexColorData(ref vertexColorGreenData);
            ValidateVertexColorData(ref vertexColorBlueData);
            ValidateVertexColorData(ref vertexColorAlphaData);
            #endif
        }

        private void ValidateScaleData()
        {
            if (scaleData.Count < splineCount)
            {
                var delta = splineCount - scaleData.Count;
                
                for (var i = 0; i < delta; i++)
                {
                    #if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(this, "Modifying Spline Mesh Scale");
                    #endif
                    
                    //float length = container.Splines[i].CalculateLength(container.transform.localToWorldMatrix);
                    
                    SplineData<float3> data = new SplineData<float3>();
                    data.DefaultValue = Vector3.one;
                    data.PathIndexUnit = settings.deforming.scalePathIndexUnit;

                    scaleData.Add(data);
                }
            }
            
            //One for every spline
            for (int j = 0; j < scaleData.Count; j++)
            {
                //Index unit has changed, convert the index value
                if (scaleData[j].PathIndexUnit != settings.deforming.scalePathIndexUnit)
                {
                    //Debug.Log($"Scale index unit changed from {scaleData[j].PathIndexUnit} to {settings.deforming.scalePathIndexUnit}");
                    
                    ConvertIndexUnit(splineContainer.Splines[j], ref scaleData, j, settings.deforming.scalePathIndexUnit);
                }
            }
        }

        private void ValidateRollData()
        {
            if (rollData.Count < splineCount)
            {
                var delta = splineCount - rollData.Count;
                
                for (var i = 0; i < delta; i++)
                {
                    #if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(this, "Modifying Spline Mesh Roll");
                    #endif

                    SplineData<float> data = new SplineData<float>();
                    data.DefaultValue = 0f;
                    data.PathIndexUnit = settings.deforming.rollPathIndexUnit;

                    rollData.Add(data);
                }
            }
            
            //One for every spline
            for (int j = 0; j < rollData.Count; j++)
            {
                //Index unit has changed, convert the index value
                if (rollData[j].PathIndexUnit != settings.deforming.rollPathIndexUnit)
                {
                    ConvertIndexUnit(splineContainer.Splines[j], ref rollData, j, settings.deforming.rollPathIndexUnit);
                }
            }
        }

        void ValidateConformingData()
        {
            if (conformingStrength.Count < splineCount)
            {
                var delta = splineCount - conformingStrength.Count;
                
                for (var i = 0; i < delta; i++)
                {
                    #if UNITY_EDITOR
                    UnityEditor.Undo.RecordObject(this, "Modifying Spline Mesh Conforming");
                    #endif

                    SplineData<float> data = new SplineData<float>();
                    data.DefaultValue = 1f;
                    data.PathIndexUnit = settings.conforming.pathIndexUnit;

                    conformingStrength.Add(data);
                }
            }
            
            //One for every spline
            for (int j = 0; j < conformingStrength.Count; j++)
            {
                //Index unit has changed, convert the index value
                if (conformingStrength[j].PathIndexUnit != settings.conforming.pathIndexUnit)
                {
                    ConvertIndexUnit(splineContainer.Splines[j], ref conformingStrength, j, settings.conforming.pathIndexUnit);
                }
            }
        }

        private void ConvertIndexUnit<T>(ISpline spline, ref List<SplineData<T>> data, int index, PathIndexUnit targetUnit)
        {
            //Data points
            for (int i = 0; i < data[index].Count; i++)
            {
                data[index].ConvertPathUnit(spline, targetUnit);
            }

            //Set to new index unit
            data[index].PathIndexUnit = targetUnit;
        }

        private void ValidateVertexColorData(ref List<SplineData<VertexColorChannel>> channel)
        {
            var delta = splineCount - channel.Count;
                
            for (var i = 0; i < delta; i++)
            {
                #if UNITY_EDITOR
                UnityEditor.Undo.RecordObject(this, "Modifying Spline Mesh Vertex Color");
                #endif

                SplineData<VertexColorChannel> data = new SplineData<VertexColorChannel>();

                VertexColorChannel dataPoint = new VertexColorChannel();
                dataPoint.value = 0f;
                dataPoint.blend = true;
                
                data.DefaultValue = dataPoint;
                data.PathIndexUnit = settings.color.pathIndexUnit;
                
                channel.Add(data);
            }

            //One for every spline
            for (int j = 0; j < channel.Count; j++)
            {
                //Index unit has changed, convert the index value
                if (channel[j].PathIndexUnit != settings.color.pathIndexUnit)
                {
                    ConvertIndexUnit(splineContainer.Splines[j], ref channel, j, settings.color.pathIndexUnit);
                }
            }
        }
        
        #region Public API
        #if SPLINES && MATHEMATICS
        /// <summary>
        /// Sample the mesh scale data on the spline. If no data is present, a default scale of (1,1,1) is returned.
        /// </summary>
        /// <param name="distance">The distance along the spline curve</param>
        /// <param name="splineIndex">Spline index number</param>
        /// <returns></returns>
        public float3 SampleScale(float distance, int splineIndex)
        {
            float3 splineScale = 1f;
            
            if (scaleData != null)
            {
                scaleInterpolator.mode = settings.deforming.scaleInterpolation;
                
                if (scaleData[splineIndex].Count > 0)
                {
                    splineScale = scaleData[splineIndex].Evaluate(splineContainer.Splines[splineIndex], distance, scaleData[splineIndex].PathIndexUnit, scaleInterpolator);
                }
            }

            return splineScale;
        }

        public Quaternion SampleRollRotation(ISpline spline, Vector3 forward, float distance, int splineIndex)
        {
            float rollFrequency = settings.deforming.rollFrequency > 0 ? settings.deforming.rollFrequency * (distance) : 1f;
            float rollAngle = settings.deforming.rollAngle;

            float rollValue = rollAngle * rollFrequency;
            
            if (rollData != null)
            {
                if (rollData[splineIndex].Count > 0)
                {
                    
                    rollValue += rollData[splineIndex].Evaluate(spline, splineContainer.Splines[splineIndex].ConvertIndexUnit(distance, PathIndexUnit.Distance, settings.deforming.rollPathIndexUnit), settings.deforming.rollPathIndexUnit, SplineMeshGenerator.FloatInterpolator);
                }
            }

            return Quaternion.AngleAxis(-rollValue, forward);
        }
        
        /// <summary>
        /// Given a world-space position, attempt to find the nearest point on the spline, then sample the mesh scale data there.
        /// If all fails, a default scale of (1,1,1) is returned.
        /// </summary>
        /// <param name="worldPosition"></param>
        /// <returns></returns>
        public float3 SampleScale(Vector3 worldPosition)
        {
            Vector3 localPosition = splineContainer.transform.InverseTransformPoint(worldPosition);

            //Unclear how one would find the nearest spline, so default to the first
            int splineIndex = 0;
            
            //Find the position on the spline that's nearest to the box's center
            SplineUtility.GetNearestPoint(splineContainer.Splines[splineIndex], localPosition, out var nearestPoint, out float t, SplineUtility.PickResolutionMin, 2);

            //Convert the normalized t-index to the distances on the spline
            float distance = splineContainer.Splines[splineIndex].ConvertIndexUnit(t, PathIndexUnit.Normalized, scaleData[splineIndex].PathIndexUnit);

            return SampleScale(distance, splineIndex);
        }
        #endif
        #endregion

        [Obsolete("Use the native SplineUtility.FitSplineToPoints function instead.")]
        public void CreateSplineFromPoints(Vector3[] positions, bool smooth) { }
        #endif //SPLINES
    }
}