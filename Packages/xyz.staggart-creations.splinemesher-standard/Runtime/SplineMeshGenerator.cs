// Staggart Creations (http://staggart.xyz)
// Copyright protected under Unity Asset Store EULA
// Copying or referencing source code for the production of new asset store, or public content, is strictly prohibited!

//^ NOTE: Transforming the spline point to world-space results in the mesh respecting non-uniformly scaled splines.
//But this breaks them when the spline is rotated.
//In favor of rotation, this is not being done
//#define SM_WORLD_SPACE_TRANSFORM

//#define SM_ADDITIONAL_DATA

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.Rendering;

#if MATHEMATICS
using Unity.Mathematics;
#endif
#if SPLINES
using UnityEngine.Splines;
using Interpolators = UnityEngine.Splines.Interpolators;
#endif

namespace sc.modeling.splines.runtime
{
    public static class SplineMeshGenerator
    {
        #if SPLINES && MATHEMATICS
        //Mesh data
        private static readonly List<Vector3> vertices = new List<Vector3>();
        private static readonly List<Vector3> normals = new List<Vector3>();
        private static readonly List<Vector4> tangents = new List<Vector4>();
        private static readonly List<Vector4> uv0 = new List<Vector4>();
        #if SM_ADDITIONAL_DATA
        private static readonly List<Vector4> uv2 = new List<Vector4>();
        #endif
        
        //Holds an array of indices for each submesh
        private static readonly List<List<int>> triangles = new List<List<int>>();
        private static readonly List<Color> colors = new List<Color>();
        
        private static Vector3[] sourceVertices;
        private static List<int[]> sourceTriangles = new List<int[]>();
        private static Vector3[] sourceNormals;
        private static List<Vector4> sourceUv0 = new List<Vector4>();
        private static Vector4[] sourceTangents;
        private static Color[] sourceColors;

        private static bool hasTangents;
        private static bool hasUV;
        private static bool hasSourceVertexColor;
        private static bool setVertexColor;
        
        private static List<CombineInstance> combineInstances = new List<CombineInstance>();

        private static Bounds bounds;
        private static float3 boundsMin;
        private static float3 boundsMax;
        
        private static float4x4 splineLocalToWorld;
        
        public static readonly Interpolators.LerpFloat FloatInterpolator = new Interpolators.LerpFloat();

        private static int CalculateSegmentCount(Settings settings, float splineLength, float meshLength, bool closed)
        {
            int segmentCount = settings.distribution.segments;

            if (settings.distribution.autoSegmentCount)
            {
                //Seems to need one extra segment to full close the loop
                if (closed) splineLength += 0.001f;
                
                if (settings.distribution.stretchToFit)
                {
                    return (int)math.ceil((splineLength / meshLength));
                }
                
                if (settings.distribution.evenOnly)
                {
                    return (int)math.floor((splineLength / meshLength));
                }
                else
                {
                    return (int)math.ceil((splineLength / meshLength));
                }
            }

            return segmentCount;
        }
        
        /// <summary>
        /// Tiles and deforms the sourceMesh along splines within the container
        /// </summary>
        /// <param name="outputMesh"></param>
        /// <param name="splineContainer"></param>
        /// <param name="sourceMesh">Input mesh</param>
        /// <param name="worldToLocalMatrix">Transform matrix of the renderer the mesh is to be used on</param>
        /// <param name="settings"></param>
        /// <param name="scaleData"></param>
        /// <param name="rollData"></param>
        /// <param name="redVertexColor"></param>
        /// <param name="greenVertexColor"></param>
        /// <param name="blueVertexColor"></param>
        /// <param name="alphaVertexColor"></param>
        /// <returns></returns>
        /// <exception cref="Exception">Bails out if the spline is too short</exception>
        public static Mesh CreateMesh(ref Mesh outputMesh, SplineContainer splineContainer, Mesh sourceMesh, float4x4 worldToLocalMatrix, Settings settings, 
            List<SplineData<float3>> scaleData = null, 
            List<SplineData<float>> rollData = null,
            List<SplineData<float>> conformingStrength = null,
            List<SplineData<SplineMesher.VertexColorChannel>> redVertexColor = null,
            List<SplineData<SplineMesher.VertexColorChannel>> greenVertexColor = null,
            List<SplineData<SplineMesher.VertexColorChannel>> blueVertexColor = null,
            List<SplineData<SplineMesher.VertexColorChannel>> alphaVertexColor = null
            )
        {
            int submeshCount = sourceMesh.subMeshCount;
            
            var splineCount = splineContainer.Splines.Count;
            //Note: every submesh requires its own CombineInstance
            combineInstances.Clear();

            //Debug.Log($"Generating for {splineCount} spline(s) from {sourceMesh.name} with {submeshCount} submesh(es)");
            
            boundsMin = Vector3.one * -math.INFINITY;
            boundsMax = Vector3.one * math.INFINITY;

            //Get initial arrays
            sourceVertices = sourceMesh.vertices;
            int sourceVertexCount = sourceVertices.Length;
            sourceNormals = sourceMesh.normals;
            sourceMesh.GetUVs(0, sourceUv0);
            sourceTangents = sourceMesh.tangents;
            sourceColors = sourceMesh.colors;
            bounds = sourceMesh.bounds;
            
            sourceTriangles.Clear();
            for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
            {
                //Input
                sourceTriangles.Add(sourceMesh.GetTriangles(submeshIndex));
            }

            hasUV = sourceUv0.Count > 0;
            hasTangents = sourceTangents.Length > 0;
            hasSourceVertexColor = sourceColors.Length > 0;
            
            Color vertexColor = Color.black;
            setVertexColor = hasSourceVertexColor;
            
            splineLocalToWorld = splineContainer.transform.localToWorldMatrix;
            
            int vertexCount = 0;

            Profiler.BeginSample($"Spline Mesher: Process {splineCount} Spline(s)");
            
            #if SM_WORLD_SPACE_TRANSFORM
            float3 containerScale = splineContainer.transform.lossyScale;
            #endif

            bool hasScaleData = scaleData != null;
            bool hasRollData = rollData != null;
            bool hasConformingData = conformingStrength != null;
            bool hasRedColorData = redVertexColor != null;
            bool hasGreenColorData = greenVertexColor != null;
            bool hasBlueColorData = blueVertexColor != null;
            bool hasAlphaColorData = alphaVertexColor != null;

            float2 trimming = new float2(settings.distribution.trimStart, settings.distribution.trimEnd);
            for (int splineIndex = 0; splineIndex < splineCount; splineIndex++)
            {
                Spline spline = splineContainer.Splines[splineIndex];
                
                float splineLength = spline.CalculateLength(splineLocalToWorld);
                
                //T-values of the trimming
                float2 trimRange = new float2((trimming.x / splineLength), 1f - (trimming.y / splineLength));
                
                float trimLength = trimming.x + trimming.y;
                
                splineLength -= trimLength;

                float zScale = settings.deforming.scale.z;
                
                float meshHeight = bounds.size.y;
                float meshLength = math.max(0.1f, bounds.size.z * zScale);
                float segmentLength = meshLength + settings.distribution.spacing;

                if (splineLength <= 0.02f)
                {
                    //Debug.LogError($"Spline #{splineIndex} in {splineContainer.name} is too short ({splineLength})");
                    continue;
                }

                //Too short
                if (splineLength < segmentLength)
                {
                    //Debug.LogWarning($"Input mesh ({sourceMesh.name}) is larger ({meshLength}) than the length of the spline #{splineIndex} ({splineLength}), no output mesh possible");
                    //continue;
                }
                
                int CalculateSegments()
                {
                    //Spline length needs a tiny bit of padding, otherwise its possible that it miscalculates the count by 1 to few
                    return CalculateSegmentCount(settings, splineLength, segmentLength, spline.Closed);
                }
                int segments = CalculateSegments();
                if (segments == 0) continue;
                
                //Scale each segment by the right amount so that they all evenly fit along the spline
                if (settings.distribution.stretchToFit)
                {
                    float totalMeshLength = segments * segmentLength;

                    //Scale value needed for each individual segment to achieve full coverage
                    float zScaleDelta = splineLength / totalMeshLength;
                    
                    zScale *= zScaleDelta;

                    //Factor in new scale
                    meshLength = math.max(0.1f, bounds.size.z * zScale);
                    segmentLength = meshLength + settings.distribution.spacing;
                    
                    //Recalculate
                    segments = CalculateSegments();
                    
                    //Debug.Log($"Segments:{segments} Segment length: {segmentLength} - Total mesh length: {segments * segmentLength} Spline length {splineLength}. Delta:{zScaleDelta}. Z-scale: {zScale} (new mesh length:{segments * bounds.size.z * zScale})");
                    if (segments == 0) continue;
                }

                var splineMesh = new Mesh();
                splineMesh.subMeshCount = submeshCount;
                
                //Clear data for current spline, which is to get its own mesh.
                triangles.Clear();
                for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                {
                    triangles.Add(new List<int>());
                }
                vertices.Clear();
                normals.Clear();
                tangents.Clear();
                uv0.Clear();
                colors.Clear();
                #if SM_ADDITIONAL_DATA
                uv2.Clear();
                #endif
                
                float3 origin = 0f;
                float3 tangent = 0f;
                float3 up = 0f;
                float3 forward = 0f;
                float3 right = 0f;
                quaternion rotation = quaternion.identity;
                quaternion normalRotation = quaternion.identity;
                float3 splineScale = new float3(1f);
                
                //int splineSamples = 0;
                
                for (int i = 0; i < segments; i++)
                {
                    float segmentOffset = ((float)i * (segmentLength));
                    float prevZ = -1;
                    
                    for (int v = 0; v < sourceVertexCount; v++)
                    {
                        //t-value of vertex over the length of the mesh. Normalized value 0-1
                        float localVertPos = (sourceVertices[v].z - bounds.min.z) / (bounds.max.z - bounds.min.z);
                        //localVertPos = math.clamp(localVertPos, 0f, 1f);

                        float distance = (localVertPos * meshLength) + segmentOffset;
                        float center = (0.5f * meshLength) + segmentOffset;
                        
                        //Check if Z-value of vertex is changing, meaning sampling moves forward
                        var resample = (math.abs(distance - prevZ) > 0f);
                        //resample = true;
                        if (resample) prevZ = distance;
                        
                        float3 splinePoint = origin;
                        
                        float t = distance / splineLength;

                        //Important optimization. If a mesh has edge loops (vertices sharing the same Z-value) the spline gets unnecessarily re-sampled
                        //In this case, all the spline-related information is identical, so doesn't need to be recalculated.
                        if (resample)
                        {
                            Profiler.BeginSample("Spline Mesher: Sample Spline");
                            {
                                //Remap normalized (0-1) t-range to trimmed range
                                t = math.lerp(trimRange.x, trimRange.y, t);
                                
                                t = math.clamp(t, 0.000001f, 0.999999f); //Ensure a tangent can always be derived

                                spline.Evaluate(t, out origin, out tangent, out up);
                                //SplineCache.Evaluate(splineContainer, splineIndex, t, out origin, out tangent, out up);
                                //splineSamples++;
                                
                                //Recalculate tangent, required for a correct value on scaled spline containers
                                //float3 prevPosition = spline.EvaluatePosition(t - 0.001f);
                                //tangent = origin - prevPosition;
                                
                                forward = math.normalize(tangent);
                                right = math.cross(up, forward);
                                rotation = quaternion.LookRotation(forward, up);

                                if (settings.deforming.ignoreKnotRotation && settings.deforming.rollAngle == 0)
                                {
                                    rotation = RollCorrectedRotation(forward);
                                    right = math.rotate(rotation, math.right());
                                }

                                if (settings.deforming.rollAngle != 0f || hasRollData)
                                {
                                    //Aligned conforming will completely override this rotation, so skip the calculations
                                    if ((settings.conforming.enable && settings.conforming.align) == false)
                                    {
                                        Profiler.BeginSample("Spline Mesher: Calculate roll rotation");
                                        {
                                            float rollInterpolator = settings.deforming.rollMode == Settings.Deforming.RollMode.PerSegment ? (center / splineLength) : t;

                                            float rollFrequency = settings.deforming.rollFrequency > 0 ? settings.deforming.rollFrequency * (rollInterpolator * splineLength) : 1f;
                                            float rollAngle = settings.deforming.rollAngle;

                                            float rollValue = rollAngle * rollFrequency;

                                            if (hasRollData)
                                            {
                                                if (rollData[splineIndex].Count > 0)
                                                {
                                                    rollValue += rollData[splineIndex].Evaluate(spline,
                                                        spline.ConvertIndexUnit(rollInterpolator, PathIndexUnit.Normalized, settings.deforming.rollPathIndexUnit),
                                                        settings.deforming.rollPathIndexUnit, FloatInterpolator);
                                                }
                                            }

                                            //Not needed, only a tiny bit of skewing
                                            //forward = math.normalize(spline.EvaluateTangent(rollInterpolator));

                                            //rotation = Quaternion.AngleAxis(-rollValue, forward) * rotation;
                                            rotation = math.mul(quaternion.AxisAngle(forward, -rollValue * Mathf.Deg2Rad), rotation);
                                            
                                            //Recalculate vectors, particularly for the curve offset functionality later on
                                            right = math.mul(rotation, math.right());
                                            up = math.mul(rotation, math.up());
                                        }
                                        Profiler.EndSample();
                                    }
                                }
                            }
                            Profiler.EndSample();
                            
                            splineScale = new float3(1f);
                            if (hasScaleData)
                            {
                                //Important to not attempt to sample empty data, as this results in a scale of (0,0,0)
                                if (scaleData[splineIndex].Count > 0)
                                {
                                    SplineMesher.scaleInterpolator.mode = settings.deforming.scaleInterpolation;
                                    
                                    splineScale = scaleData[splineIndex].Evaluate(spline, 
                                        spline.ConvertIndexUnit(distance, PathIndexUnit.Distance, settings.deforming.scalePathIndexUnit), 
                                        settings.deforming.scalePathIndexUnit, SplineMesher.scaleInterpolator);
                                }
                            }

                            //Counter scale of spline container transform
                            //splineScale /= containerScale;
                            splineScale.x *= settings.deforming.scale.x;
                            splineScale.y *= settings.deforming.scale.y; 

                            #if SM_WORLD_SPACE_TRANSFORM
                            //Counter scale again to allow for non-uniform scaling of the spline
                            splineScale.x /= containerScale.x;
                            splineScale.y /= containerScale.y;
                            #endif
                            
                            //Never scale the Z-axis, as this affects distribution of the vertices
                            splineScale.z = 0f;

                            //Update
                            splinePoint = origin;
                            normalRotation = rotation;
                        }
                        
                        //Outside of resampling scope, since the color may be different over the vertical length of the mesh
                        Profiler.BeginSample("Spline Mesher: Sample Vertex Colors");
                        {
                            vertexColor = hasSourceVertexColor ? sourceColors[v] : Color.clear;

                            float vcSamplePos = spline.ConvertIndexUnit(distance, PathIndexUnit.Distance, settings.color.pathIndexUnit);
                            
                            if (hasRedColorData)
                            {
                                if (redVertexColor[splineIndex].Count > 0)
                                {
                                    vertexColor.r = redVertexColor[splineIndex].Evaluate(spline, vcSamplePos, settings.color.pathIndexUnit,
                                        new SplineMesher.VertexColorChannel.LerpVertexColorData(vertexColor.r));
                                    
                                    setVertexColor = true;
                                }
                            }
                            if (hasGreenColorData)
                            {
                                if (greenVertexColor[splineIndex].Count > 0)
                                {
                                    vertexColor.g = greenVertexColor[splineIndex].Evaluate(spline, vcSamplePos, settings.color.pathIndexUnit,
                                        new SplineMesher.VertexColorChannel.LerpVertexColorData(vertexColor.g));

                                    setVertexColor = true;
                                }
                            }
                            if (hasBlueColorData)
                            {
                                if (blueVertexColor[splineIndex].Count > 0)
                                {
                                    vertexColor.b = blueVertexColor[splineIndex].Evaluate(spline, vcSamplePos, settings.color.pathIndexUnit,
                                        new SplineMesher.VertexColorChannel.LerpVertexColorData(vertexColor.b));

                                    setVertexColor = true;
                                }
                            }
                            if (hasAlphaColorData)
                            {
                                if (alphaVertexColor[splineIndex].Count > 0)
                                {
                                    vertexColor.a = alphaVertexColor[splineIndex].Evaluate(spline, vcSamplePos, settings.color.pathIndexUnit,
                                        new SplineMesher.VertexColorChannel.LerpVertexColorData(vertexColor.a));

                                    setVertexColor = true;
                                }
                            }
                        }
                        Profiler.EndSample();

                        if (settings.conforming.enable)
                        {
                            Profiler.BeginSample("Spline Mesher: Conform to colliders");
                            
                            //Vertex position in spline's local space to world-space
                            float3 positionWS = math.transform(splineLocalToWorld, splinePoint);

                            if (PerformConforming(positionWS, settings.conforming.direction == Settings.Conforming.Direction.StraightDown ? -math.up() : -up, settings.conforming, meshHeight, out float3 hitPosition, out float3 hitNormal))
                            {
                                float strength = 1.0f;

                                if (hasConformingData && conformingStrength[splineIndex].Count > 0)
                                {
                                    strength = conformingStrength[splineIndex].Evaluate(spline, 
                                        spline.ConvertIndexUnit(t, PathIndexUnit.Normalized, settings.conforming.pathIndexUnit), 
                                        settings.conforming.pathIndexUnit, FloatInterpolator);
                                }
                                
                                if (strength > 0)
                                {
                                    //Convert information from world-space back to spline's local space
                                    hitPosition = splineContainer.transform.InverseTransformPoint(hitPosition);
                                    hitNormal = splineContainer.transform.InverseTransformVector(hitNormal);

                                    splinePoint = math.lerp(splinePoint, hitPosition, strength);

                                    quaternion hitRotation = quaternion.LookRotationSafe(tangent, hitNormal);

                                    //Copy normal of surface, to be used for deforming
                                    if (settings.conforming.align)
                                    {
                                        rotation = hitRotation;
                                    }

                                    if (settings.conforming.blendNormal)
                                    {
                                        normalRotation = hitRotation;
                                    }
                                }
                            }
                            Profiler.EndSample();
                        }

                        splinePoint += right * settings.deforming.curveOffset.x;
                        splinePoint.y += settings.deforming.curveOffset.y;

                        Profiler.BeginSample("Spline Mesher: Transform Vertices");
                        
                        float3 vertexPositionLocal = (float3)sourceVertices[v] + (math.forward() * settings.distribution.spacing);
                        vertexPositionLocal.x += settings.deforming.pivotOffset.x;
                        vertexPositionLocal.y += settings.deforming.pivotOffset.y;
                        
                        //Transform vertex to spline point and rotation (spline's world-space)
                        #if SM_WORLD_SPACE_TRANSFORM //See note at top
                        splinePoint = math.mul(splineLocalToWorld, new float4(splinePoint, 1.0f)).xyz;
                        //rotation = splineContainer.transform.rotation * rotation;
                        #endif

                        float3 position = splinePoint + math.rotate(rotation, vertexPositionLocal * splineScale);
                        
                        #if !SM_WORLD_SPACE_TRANSFORM
                        //Transform position from spline's local-space into world-space
                        float3 vertexPosition = math.mul(splineLocalToWorld, new float4(position, 1.0f)).xyz;
                        #else
                        //Already in world-space
                        float3 vertexPosition = position;
                        #endif
                        
                        //Make that the local-space position of the mesh filter
                        vertexPosition = math.mul(worldToLocalMatrix, new float4(vertexPosition, 1.0f)).xyz;

                        if (hasUV)
                        {
                            Vector4 uv = sourceUv0[v];

                            if (settings.uv.stretchMode == Settings.UV.StretchMode.U) uv.x = t;
                            if (settings.uv.stretchMode == Settings.UV.StretchMode.V) uv.y = t;
                            
                            uv = (uv * settings.uv.scale) + settings.uv.offset;

                            if (settings.mesh.storeGradientsInUV)
                            {
                                //Normalized Distance
                                uv.z = t;
                                //Normalized Height
                                uv.w = (float)math.abs(vertexPositionLocal.y / (meshHeight * splineScale.y));
                            }

                            uv0.Add(uv);
                        }
                        
                        //Also transform the normal
                        float3 vertexNormal = math.rotate(normalRotation, sourceNormals[v]);
                        
                        if (hasTangents)
                        {
                            float4 sourceTangent = new float4(sourceTangents[v]);
                            
                            float3 vertexTangent = math.rotate(normalRotation, sourceTangent.xyz);

                            //Flipping the UV also requires to flip the .w component (bi-tangent)
                            if (hasUV)
                            {
                                if (settings.uv.scale.y < 0)
                                {
                                    sourceTangent.w *= -1;
                                }
                            }
                            tangents.Add(new float4(vertexTangent, sourceTangent.w));
                        }
                        
                        Profiler.EndSample();

                        //Extend bounds as it expands
                        boundsMin = math.min(position, boundsMin);
                        boundsMax = math.max(position, boundsMax);
                        
                        //Assign vertex attributes
                        vertices.Add(vertexPosition);
                        normals.Add(vertexNormal);
                        
                        if(setVertexColor) colors.Add(vertexColor);
                        
                        #if SM_ADDITIONAL_DATA
                        uv2.Add(new Vector4(
                            (float)i/segments, //Segment ID
                            (float)(v * (i+1))/(segments * sourceVertexCount), //Vertex ID
                            ((float)splineIndex/splineCount), //Spline ID
                            noise.cnoise(new float2(v+ distance * splineLength, i + vertexPositionLocal.y * meshHeight)) //Noise
                                ));
                        #endif
                    }
                    
                    for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                    {
                        var triCount = sourceTriangles[submeshIndex].Length;
                        for (int v = 0; v < triCount; v++)
                        {
                            triangles[submeshIndex].Insert(i * triCount + v, sourceTriangles[submeshIndex][v] + (sourceVertexCount * i));
                        }
                    }
                }

                var splineVertCount = vertices.Count;
                vertexCount += splineVertCount * submeshCount;

                //Debug.Log($"Estimated spline samples: {segments * sourceVertexCount}. Actual spline samples performed: {splineSamples}. Improvement: {100f/(1f/((float)(segments * sourceVertexCount) / (float)splineSamples) * 100f)*100f}%");
                
                Profiler.BeginSample($"Spline Mesher: Set mesh data for spline #{splineIndex}");
                
                splineMesh.indexFormat = splineVertCount >= 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
                splineMesh.SetVertices(vertices, 0, splineVertCount, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
                splineMesh.SetNormals(normals, 0, splineVertCount, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);
                
                if(hasTangents) splineMesh.SetTangents(tangents);
                if(hasUV) splineMesh.SetUVs(0, uv0);
                #if SM_ADDITIONAL_DATA
                splineMesh.SetUVs(2, uv2);
                #endif
                if(setVertexColor) splineMesh.SetColors(colors);
                
                for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                {
                    splineMesh.SetIndices(triangles[submeshIndex], MeshTopology.Triangles, submeshIndex, false);
                    
                    CombineInstance combineInstance = new CombineInstance()
                    {
                        mesh = splineMesh,
                        subMeshIndex = submeshIndex
                    };
                    
                    combineInstances.Add(combineInstance);
                }
                
                Profiler.EndSample();
                //Debug.Log($"Generated mesh for spline #{splineIndex} with {submeshCount} submeshs");
            }
            
            Profiler.EndSample();
            
            Profiler.BeginSample("Spline Mesher: Composite Output Mesh");
            
            outputMesh.Clear();
            outputMesh.indexFormat = vertexCount >= 65535 ? IndexFormat.UInt32 : IndexFormat.UInt16;
            
            //Note: Warning about Instance X being null is attributed to a Spline having a length of 0. Therefor it was counted, but no mesh was created for it.
            //Bug: if submeshes aren't merged, a submesh is created for each spline
            outputMesh.CombineMeshes(combineInstances.ToArray(), submeshCount == 1, false);
            
            //Debug.Log($"Combined {splineCount} spline meshes from {sourceMesh.name} with {outputMesh.subMeshCount} submeshes");
            
            outputMesh.UploadMeshData(!settings.mesh.keepReadable);
            outputMesh.bounds.SetMinMax(boundsMin, boundsMax);

            outputMesh.name = $"{sourceMesh.name} Spline";
            
            Profiler.EndSample();
            //Test, to verify if normals were rotated correctly
            //outputMesh.RecalculateNormals();

            return outputMesh;
        }

        public static bool PerformConforming(float3 positionWS, float3 direction, Settings.Conforming settings, float objectHeight, out float3 hitPosition, out float3 hitNormal)
        {
            var validHit = false;
            
            float dist = math.max(objectHeight + settings.seekDistance, 1f);

            hitPosition = float3.zero;
            hitNormal = float3.zero;

            if (Physics.Raycast(positionWS + (-direction * dist), direction, out RaycastHit hit, dist * 2f, settings.layerMask, QueryTriggerInteraction.Ignore))
            {
                validHit = true;
                
                #if TERRAIN_PHYSICS
                if (settings.terrainOnly)
                {
                    validHit = hit.collider.GetType() == typeof(TerrainCollider);;

                    if (validHit == false) return false;
                }
                #endif

                hitPosition = hit.point; 
                hitNormal = hit.normal;
            }

            return validHit;
        }

        /// <summary>
        /// Apply generic transforms to the mesh
        /// </summary>
        /// <param name="input"></param>
        /// <param name="rotation">Euler rotation</param>
        /// <param name="flipX"></param>
        /// <param name="flipY"></param>
        /// <returns></returns>
        public static Mesh TransformMesh(Mesh input, Vector3 rotation, bool flipX, bool flipY)
        {
            var rotationAmount = math.abs(math.length(rotation));
            
            if (rotationAmount > 0.01f || flipX || flipY)
            {
                Vector3[] outputVertices = input.vertices;
                int vertexCount = outputVertices.Length;
                Vector3[] outputNormals = input.normals;
                int submeshCount = input.subMeshCount;
                
                Bounds outputBounds = input.bounds;
                if (rotationAmount > 0.01f)
                {
                    (rotation.x, rotation.z) = (rotation.z, rotation.x);
                    
                    outputBounds = new Bounds();

                    Quaternion m_meshRotation = Quaternion.Euler(rotation);
                    for (int i = 0; i < vertexCount; i++)
                    {
                        outputVertices[i] = math.rotate(m_meshRotation, outputVertices[i]);

                        outputBounds.Encapsulate(outputVertices[i]);

                        outputNormals[i] = math.rotate(m_meshRotation, outputNormals[i]);
                    }
                }

                if (flipX || flipY)
                {
                    //Reverse triangle order if negatively scaled
                    for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                    {
                        int[] outputTriangles = input.GetTriangles(submeshIndex);
                        int triCount = outputTriangles.Length;
                        var triangleCount = triCount / 3;
                
                        for (int j = 0; j < triangleCount; j++)
                        {
                            (outputTriangles[j * 3], outputTriangles[j * 3 + 1]) = (outputTriangles[j * 3 + 1], outputTriangles[j * 3]);
                        }
                    }
                    
                    //Rotate normals
                    Quaternion m_meshRotation = Quaternion.Euler(flipY ? 180f : 0f, flipX ? 180f : 0f, 0f);
                    for (int i = 0; i < vertexCount; i++)
                    {
                        outputNormals[i] = math.rotate(m_meshRotation, outputNormals[i]);
                    }
                }

                Mesh output = new Mesh();
                output.name = input.name;
                output.SetVertices(outputVertices, 0, vertexCount, MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontRecalculateBounds);


                //output.RecalculateBounds();
                output.bounds = outputBounds;
                
                output.uv = input.uv;
                output.uv2 = input.uv2;
                output.normals = outputNormals;
                output.colors = input.colors;
                output.tangents = input.tangents;
                output.subMeshCount = input.subMeshCount;
                
                for (int submeshIndex = 0; submeshIndex < submeshCount; submeshIndex++)
                {
                    output.SetTriangles(input.GetTriangles(submeshIndex), submeshIndex);
                }
                
                //Copy readable state
                output.UploadMeshData(!input.isReadable);

                return output;
            }
            
            return input;
        }

        public static quaternion RollCorrectedRotation(float3 forward)
        {
			float3 euler = Quaternion.LookRotation(forward, math.up()).eulerAngles;
            return quaternion.AxisAngle(math.up(), euler.y * Mathf.Deg2Rad);
            //return quaternion.LookRotation(forward, math.up());
        }

        private static readonly Vector2[] corners = new[]
        {
            new Vector2(-0.5f, -0.5f), //Bottom-left
            new Vector2(-0.5f, 0.5f), //Top-left
            new Vector2(0.5f, 0.5f),  //Top-right
            new Vector2(0.5f, -0.5f), //Bottom-right
            new Vector2(-0.5f, -0.5f), //Bottom-right
        };
        
        /// <summary>
        /// Creates a cube mesh from the input mesh's bounds.
        /// </summary>
        /// <param name="sourceMesh"></param>
        /// <param name="subdivisions">Number of edge loops across the length</param>
        /// <param name="caps">Create two triangles at the front and back</param>
        /// <returns></returns>
        public static Mesh CreateBoundsMesh(Mesh sourceMesh, int subdivisions = 0, bool caps = false)
        {
            Bounds m_bounds  = sourceMesh.bounds;
            
            Mesh boundsMesh = new Mesh();
            boundsMesh.name = $"{sourceMesh.name} Box";

            Vector3 scale = m_bounds.size;
            Vector3 offset = m_bounds.center;
            
            int edges = 4;
            subdivisions = Mathf.Max(0, subdivisions);
            int lengthSegments = subdivisions+1;

            int xCount = edges + 1;
            int zCount = lengthSegments + 1;
            int numVertices = xCount * zCount;

            List<Vector3> mVertices = new List<Vector3>();
            List<int> mTriangles = new List<int>();
            
            float scaleZ = scale.z / lengthSegments;
            
            for (int z = 0; z < zCount; z++)
            {
                //Move clockwise to position vertices in each corner around the edge loop
                for (int x = 0; x < xCount; x++)
                {
                    Vector3 vertex;
                    
                    vertex.x = (corners[x].x * scale.x) + offset.x;
                    vertex.y = (corners[x].y * scale.y) + offset.y;
                    vertex.z = z * scaleZ - (scale.z * 0.5f) + offset.z;

                    mVertices.Add(vertex);
                }
                
                if (z < zCount-1) //Stop at 2nd last row
                {
                    for (int x = 0; x < edges; x++)
                    {
                        mTriangles.Insert(0, (z * xCount) + x);
                        mTriangles.Insert(1, ((z + 1) * xCount) + x);
                        mTriangles.Insert(2, (z * xCount) + x + 1);

                        mTriangles.Insert(3, ((z + 1) * xCount) + x);
                        mTriangles.Insert(4, ((z + 1) * xCount) + x + 1);
                        mTriangles.Insert(5, (z * xCount) + x + 1);
                    }
                }
            }

            if (caps)
            {
                //Back quad
                
                mTriangles.Add(1);
                mTriangles.Add(2);
                mTriangles.Add(0);
                
                mTriangles.Add(2);
                mTriangles.Add(3);
                mTriangles.Add(0);
                
                //Front quad
                mTriangles.Add(numVertices-4);
                mTriangles.Add(numVertices-5);
                mTriangles.Add(numVertices-3);
                
                mTriangles.Add(numVertices-2);
                mTriangles.Add(numVertices-3);
                mTriangles.Add(numVertices-5);

            }
            
            boundsMesh.SetVertices(mVertices, 0, numVertices, MeshUpdateFlags.DontNotifyMeshUsers | MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            boundsMesh.subMeshCount = 1;
            boundsMesh.SetIndices(mTriangles, MeshTopology.Triangles, 0, false);
            boundsMesh.RecalculateNormals();
            boundsMesh.bounds = m_bounds;
            //boundsMesh.UploadMeshData(false);

            return boundsMesh;
        }
        #endif
    }
}