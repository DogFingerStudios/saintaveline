using System;
using UnityEngine;
#if SPLINES
using UnityEngine.Splines;
#endif

namespace sc.modeling.splines.runtime
{
    [Serializable]
    public class Settings
    {
        public enum ColliderType
        {
            Box,
            Mesh
        }

        public enum InterpolationType
        {
            Linear,
            EaseInEaseOut,
        }

        [Serializable]
        public class Collision
        {
            [Tooltip("Add a Mesh Collider component and also generate a collision mesh for it")]
            public bool enable;
            [Tooltip("Do not create a visible mesh, but only create the collision mesh")]
            public bool colliderOnly;
            
            [Tooltip("The \"Box\" type is an automatically created collider mesh, based on the source mesh's bounding box.")]
            public ColliderType type;
            [Min(0)]
            [Tooltip("Subdivide the collision box, ensures it bends better in curves.")]
            public int boxSubdivisions = 0;
            public Mesh collisionMesh;
        }
        public Collision collision = new Collision();

        [Serializable]
        public class Distribution
        {
            [Min(1)]
            public int segments = 1;
            [Tooltip("Automatically calculate the number of segments based on the length of the spline")]
            public bool autoSegmentCount = true;
            
            
            [Tooltip("Stretch the segments so that they fit exactly over the entire spline")]
            public bool stretchToFit = true;
            [Tooltip("Ensure the input mesh is repeated evenly, instead of cutting it off when it doesn't fit on the remainder of the spline.")]
            public bool evenOnly = false;

            [Min(0f)]
            [Tooltip("Shift the mesh X number of units from the start of the spline")]
            public float trimStart;
            [Min(0f)]
            [Tooltip("Shift the mesh X number of units from the end of the spline")]
            public float trimEnd;

            
            //[Min(0f)]
            [Tooltip("Space between each mesh segment")]
            public float spacing;
        }
        public Distribution distribution = new Distribution();

        [Serializable]
        public class Deforming
        {
            [Tooltip("Note that offsetting can cause vertices to sort of bunch up." +
                     "\n\nFor the best results, create a separate spline parallel to the one you are trying to offset from.")]
            [UnityEngine.Serialization.FormerlySerializedAs("offset")]
            public Vector2 curveOffset;
            [Tooltip("Adds a global offset to all vertices, effectively moving its pivot." +
                     "\n\nNote: if the pivot is already centered, this appears to do exactly the same as the Curve Offset parameter")]
            public Vector2 pivotOffset;
            public Vector3 scale = Vector3.one;
            #if SPLINES
            public PathIndexUnit scalePathIndexUnit = PathIndexUnit.Distance;
            [Tooltip("Defines how the data is interpolated from one data point, to the other")]
            public InterpolationType scaleInterpolation = InterpolationType.Linear;
            #endif
            
            [UnityEngine.Serialization.FormerlySerializedAs("ignoreRoll")]
            [Tooltip("Ignore the spline's roll rotation and ensure the geometry stays flat")]
            public bool ignoreKnotRotation = false;
            [Tooltip("The amount of times a complete rotation is completed over this distance. With a value of 1, a complete roll is created over 1 unit over the spline curve")]

            public enum RollMode
            {
                PerVertex,
                PerSegment
            }
            [Tooltip("Specify if the rotation roll is calculated for every vertex, or once and applied over the entire segment")]
            public RollMode rollMode;
            [Min(0f)]
            public float rollFrequency = 0.1f;
            [Range(-360f, 360f)]
            public float rollAngle = 0f;
            #if SPLINES
            public PathIndexUnit rollPathIndexUnit = PathIndexUnit.Distance;
            #endif
        }
        public Deforming deforming = new Deforming();

        [Serializable]
        public class UV
        {
            public Vector2 scale = Vector2.one;
            public Vector2 offset = Vector2.zero;

            public enum StretchMode
            {
                None,
                [InspectorName("U (X)")]
                U,
                [InspectorName("V (Y)")]
                V
            }
            [Tooltip("Overwrite the target UV value with that of the vertex position over the spline (normalized 0-1 value)")]
            public StretchMode stretchMode;
        }
        public UV uv = new UV();

        [Serializable]
        public class Color
        {
            #if SPLINES
            public PathIndexUnit pathIndexUnit = PathIndexUnit.Distance;
            #endif
        }
        public Color color;
        
        [Serializable]
        public class Conforming
        {
            [Tooltip("Project the spline curve into the geometry underneath it. Relies on physics raycasts.")]
            public bool enable;
            
            #if SPLINES
            public PathIndexUnit pathIndexUnit = PathIndexUnit.Distance;
            #endif

            public enum Direction
            {
                StraightDown,
                SplineNormal
            }
            public Direction direction = Direction.StraightDown;

            [Tooltip("A ray is shot this high above every vertex, and reach this much units below it." +
                     "\n\n" +
                     "If a spline is dug into the terrain too much, increase this value to still get valid raycast hits." +
                     "\n\n" +
                     "Internally, the minimum distance is always higher than the mesh's total height.")]
            public float seekDistance = 5f;

            [Tooltip("Ignore raycast hits from colliders that aren't from a Terrain")]
            public bool terrainOnly;
            [Tooltip("Only accept raycast hits from colliders on these layers")]
            public LayerMask layerMask = -1;

            [Tooltip("Rotate the geometry to match the orientation of the surface beneath it")]
            public bool align = true;
            [Tooltip("Reorient the geometry normals to match the surface hit, for correct lighting")]
            public bool blendNormal = true;
        }
        public Conforming conforming = new Conforming();

        [Serializable]
        public class OutputMesh
        {
            [Tooltip("If enabled, Unity will keep a readable copy of the mesh around in memory. Allowing other scripts to access its data, and possible alter it.")]
            public bool keepReadable = false;

            [Tooltip("Save relative vertex positions in the (assumingly) unused UV components. If disabled, the source mesh's original values are retained." +
                     "\n" +
                     "\n[UV0.Z]: (0-1) distance over spline length" +
                     "\n[UV0.W]: (0-1) distance over height of mesh" +
                     "\n\n" +
                     "This data may be used in shaders for tailored effects, such as animations.")]
            public bool storeGradientsInUV = true;
            [Tooltip("Multiplier for the pack-margin value. A value of 1 equates to 1 texel")]
            
            [Space]
            
            [Min(0.01f)]
            public float lightmapUVMarginMultiplier = 1f;
            [Range(15f, 90f)]
            [Tooltip("This angle (in degrees) or greater between triangles will cause UV seam to be created.")]
            public float lightmapUVAngleThreshold = 88f;
        }
        public OutputMesh mesh = new OutputMesh();
    }
}