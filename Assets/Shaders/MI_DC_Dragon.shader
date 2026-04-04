Shader "Unreal/MI_DC_Dragon"
{
    Properties
    {
        [NonModifiableTextureData][NoScaleOffset]_SampleTexture2D_d8cb4ef1f3be4e35b48ab7a4727a4f98_Texture_1("Texture2D", 2D) = "white" {}
        [HideInInspector]_DrawOrder("Draw Order", Range(-50, 50)) = 0
        [HideInInspector][Enum(Depth Bias, 0, View Bias, 1)]_DecalMeshBiasType("DecalMesh BiasType", Float) = 0
        [HideInInspector]_DecalMeshDepthBias("DecalMesh DepthBias", Float) = 0
        [HideInInspector]_DecalMeshViewBias("DecalMesh ViewBias", Float) = 0
        [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
        [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
        //_MainTex( "MainTex (RGB)", 2D ) = "white" {}
		Material_Texture2D_0( "Decal Texture", 2D ) = "white" {}

        View_BufferSizeAndInvSize( "View_BufferSizeAndInvSize", Vector ) = ( 1920,1080,0.00052, 0.00092 )//1920,1080,1/1920, 1/1080
        LocalObjectBoundsMin( "LocalObjectBoundsMin", Vector ) = ( 0, 0, 0, 0 )
        LocalObjectBoundsMax( "LocalObjectBoundsMax", Vector ) = ( 100, 100, 100, 0 )
    }
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            // RenderType: <None>
            "PreviewType"="Plane"
            // Queue: <None>
            "ShaderGraphShader"="true"
            "ShaderGraphTargetId"=""
        }
        Pass
        { 
            Name "DBufferProjector"
            Tags 
            { 
                "LightMode" = "DBufferProjector"
            }
        
            // Render State
            Cull Front
        Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        ZTest Greater
        ZWrite Off
        ColorMask RGBA
        ColorMask RGBA 1
        ColorMask 0 2
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 3.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        
            // Keywords
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DBUFFER_PROJECTOR
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        
            // HybridV1InjectedBuiltinProperties: <None>
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.texCoord0;
            output.interp3.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.texCoord0 = input.interp2.xyzw;
            output.color = input.interp3.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 _SampleTexture2D_d8cb4ef1f3be4e35b48ab7a4727a4f98_Texture_1_TexelSize;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        // Object and Global properties        
        
            // Graph Functions
            
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
        };
        
        #include "MI_DC_Dragon.hlsl"
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            Input In;
            In.uv_MainTex = IN.uv0.xy;
            In.uv2_Material_Texture2D_0 = IN.uv0.xy;
            In.color = IN.VertexColor;
            In.tangent = float4( 1, 0, 0, 1 );
            In.viewDir = GetWorldSpaceViewDir( IN.WorldSpacePosition );
            In.screenPos = float4( 0, 0, 0, 0 );
            In.worldPos = IN.WorldSpacePosition;
            In.normal2 = IN.WorldSpaceNormal;

            SurfaceOutputStandard Out;
            SurfaceReplacement( In, Out );

            SurfaceDescription surface = (SurfaceDescription)0;
            float3 BaseColor = Out.Albedo;
            if( length( Out.Emission ) > 1 )
                Out.Emission = normalize( Out.Emission );
            surface.BaseColor = BaseColor + Out.Emission;// IsGammaSpace() ? BaseColor : SRGBToLinear( BaseColor );
            surface.NormalTS = Out.Normal;
            //surface.Emission = Out.Emission;
            surface.Metallic = Out.Metallic;
            surface.Smoothness = Out.Smoothness;
            surface.Occlusion = Out.Occlusion;
            surface.Alpha = clamp( Out.Alpha, 0, 1 );
            surface.NormalAlpha = surface.Alpha;
            surface.MAOSAlpha = surface.Alpha;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS =                       input.normalWS;
            const float renormFactor =                          1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal =                           renormFactor*input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition =                         input.positionWS;
            output.uv0 =                                        input.texCoord0;
            output.VertexColor =                                input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            uint2 ComputeFadeMaskSeed(uint2 positionSS)
            {
                uint2 fadeMaskSeed;
        
                // Can't use the view direction, it is the same across the entire screen.
                fadeMaskSeed = positionSS;
        
                return fadeMaskSeed;
            }
        
            #if UNITY_VERSION >= 202130
                void GetSurfaceData( Varyings input, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData )
            #else
                void GetSurfaceData(Varyings input, half3 viewDirectioWS, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            #endif
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                #else
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                        LODDitheringTransition(ComputeFadeMaskSeed(positionSS), unity_LODFade.x);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DecalScreenSpaceProjector"
            Tags 
            { 
                "LightMode" = "DecalScreenSpaceProjector"
            }
        
            // Render State
            Cull Front
        Blend SrcAlpha OneMinusSrcAlpha
        ZTest Greater
        ZWrite Off
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 2.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        
            // Keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ _CLUSTERED_RENDERING
        #pragma multi_compile _DECAL_NORMAL_BLEND_LOW _DECAL_NORMAL_BLEND_MEDIUM _DECAL_NORMAL_BLEND_HIGH
            #pragma multi_compile _ _FORWARD_PLUS
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define VARYINGS_NEED_SH
            #define VARYINGS_NEED_STATIC_LIGHTMAP_UV
            #define VARYINGS_NEED_DYNAMIC_LIGHTMAP_UV
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        
            // HybridV1InjectedBuiltinProperties: <None>
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float3 interp4 : INTERP4;
             float2 interp5 : INTERP5;
             float2 interp6 : INTERP6;
             float3 interp7 : INTERP7;
             float4 interp8 : INTERP8;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.texCoord0;
            output.interp3.xyzw =  input.color;
            output.interp4.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp5.xy =  input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.interp6.xy =  input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp7.xyz =  input.sh;
            #endif
            output.interp8.xyzw =  input.fogFactorAndVertexLight;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.texCoord0 = input.interp2.xyzw;
            output.color = input.interp3.xyzw;
            output.viewDirectionWS = input.interp4.xyz;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.interp5.xy;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.interp6.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp7.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp8.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 _SampleTexture2D_d8cb4ef1f3be4e35b48ab7a4727a4f98_Texture_1_TexelSize;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        // Object and Global properties
        
            // Graph Functions
            
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
        };
        
        #include "MI_DC_Dragon.hlsl"
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            Input In;
            In.uv_MainTex = IN.uv0.xy;
            In.uv2_Material_Texture2D_0 = IN.uv0.xy;
            In.color = IN.VertexColor;
            In.tangent = float4( 1, 0, 0, 1 );
            In.viewDir = GetWorldSpaceViewDir( IN.WorldSpacePosition );
            In.screenPos = float4( 0, 0, 0, 0 );
            In.worldPos = IN.WorldSpacePosition;
            In.normal2 = IN.WorldSpaceNormal;

            SurfaceOutputStandard Out;
            SurfaceReplacement( In, Out );

            SurfaceDescription surface = (SurfaceDescription)0;
            float3 BaseColor = Out.Albedo;
            if( length( Out.Emission ) > 1 )
                Out.Emission = normalize( Out.Emission );
            surface.BaseColor = BaseColor + Out.Emission;// IsGammaSpace() ? BaseColor : SRGBToLinear( BaseColor );
            surface.NormalTS = Out.Normal;
            //surface.Emission = Out.Emission;
            surface.Metallic = Out.Metallic;
            surface.Smoothness = Out.Smoothness;
            surface.Occlusion = Out.Occlusion;
            surface.Alpha = clamp( Out.Alpha, 0, 1 );
            surface.NormalAlpha = surface.Alpha;
            surface.MAOSAlpha = surface.Alpha;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS =                       input.normalWS;
            const float renormFactor =                          1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal =                           renormFactor*input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition =                         input.positionWS;
            output.uv0 =                                        input.texCoord0;
            output.VertexColor =                                input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            uint2 ComputeFadeMaskSeed(uint2 positionSS)
            {
                uint2 fadeMaskSeed;
        
                // Can't use the view direction, it is the same across the entire screen.
                fadeMaskSeed = positionSS;
        
                return fadeMaskSeed;
            }
        
            #if UNITY_VERSION >= 202130
                void GetSurfaceData( Varyings input, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData )
            #else
                void GetSurfaceData(Varyings input, half3 viewDirectioWS, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            #endif
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                #else
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                        LODDitheringTransition(ComputeFadeMaskSeed(positionSS), unity_LODFade.x);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DecalGBufferProjector"
            Tags 
            { 
                "LightMode" = "DecalGBufferProjector"
            }
        
            // Render State
            Cull Front
        Blend 0 SrcAlpha OneMinusSrcAlpha
        Blend 1 SrcAlpha OneMinusSrcAlpha
        Blend 2 SrcAlpha OneMinusSrcAlpha
        Blend 3 SrcAlpha OneMinusSrcAlpha
        ZTest Greater
        ZWrite Off
        ColorMask RGB
        ColorMask 0 1
        ColorMask RGB 2
        ColorMask RGB 3
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 3.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        
            // Keywords
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _DECAL_NORMAL_BLEND_LOW _DECAL_NORMAL_BLEND_MEDIUM _DECAL_NORMAL_BLEND_HIGH
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define VARYINGS_NEED_SH
            #define VARYINGS_NEED_STATIC_LIGHTMAP_UV
            #define VARYINGS_NEED_DYNAMIC_LIGHTMAP_UV
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DECAL_GBUFFER_PROJECTOR
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        
            // HybridV1InjectedBuiltinProperties: <None>
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 uv0 : TEXCOORD0;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 texCoord0;
             float4 color;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float3 interp4 : INTERP4;
             float2 interp5 : INTERP5;
             float2 interp6 : INTERP6;
             float3 interp7 : INTERP7;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.texCoord0;
            output.interp3.xyzw =  input.color;
            output.interp4.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp5.xy =  input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.interp6.xy =  input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp7.xyz =  input.sh;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.texCoord0 = input.interp2.xyzw;
            output.color = input.interp3.xyzw;
            output.viewDirectionWS = input.interp4.xyz;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.interp5.xy;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.interp6.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp7.xyz;
            #endif
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 _SampleTexture2D_d8cb4ef1f3be4e35b48ab7a4727a4f98_Texture_1_TexelSize;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        // Object and Global properties
        
            // Graph Functions
            
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
        };
        
        #include "MI_DC_Dragon.hlsl"
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            Input In;
            In.uv_MainTex = IN.uv0.xy;
            In.uv2_Material_Texture2D_0 = IN.uv0.xy;
            In.color = IN.VertexColor;
            In.tangent = float4( 1, 0, 0, 1 );
            In.viewDir = GetWorldSpaceViewDir( IN.WorldSpacePosition );
            In.screenPos = float4( 0, 0, 0, 0 );
            In.worldPos = IN.WorldSpacePosition;
            In.normal2 = IN.WorldSpaceNormal;

            SurfaceOutputStandard Out;
            SurfaceReplacement( In, Out );

            SurfaceDescription surface = (SurfaceDescription)0;
            float3 BaseColor = Out.Albedo;
            if( length( Out.Emission ) > 1 )
                Out.Emission = normalize( Out.Emission );
            surface.BaseColor = BaseColor + Out.Emission;// IsGammaSpace() ? BaseColor : SRGBToLinear( BaseColor );
            surface.NormalTS = Out.Normal;
            //surface.Emission = Out.Emission;
            surface.Metallic = Out.Metallic;
            surface.Smoothness = Out.Smoothness;
            surface.Occlusion = Out.Occlusion;
            surface.Alpha = clamp( Out.Alpha, 0, 1 );
            surface.NormalAlpha = surface.Alpha;
            surface.MAOSAlpha = surface.Alpha;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS =                       input.normalWS;
            const float renormFactor =                          1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal =                           renormFactor*input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition =                         input.positionWS;
            output.uv0 =                                        input.texCoord0;
            output.VertexColor =                                input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            uint2 ComputeFadeMaskSeed(uint2 positionSS)
            {
                uint2 fadeMaskSeed;
        
                // Can't use the view direction, it is the same across the entire screen.
                fadeMaskSeed = positionSS;
        
                return fadeMaskSeed;
            }
        
            #if UNITY_VERSION >= 202130
                void GetSurfaceData( Varyings input, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData )
            #else
                void GetSurfaceData(Varyings input, half3 viewDirectioWS, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            #endif
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                #else
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                        LODDitheringTransition(ComputeFadeMaskSeed(positionSS), unity_LODFade.x);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DBufferMesh"
            Tags 
            { 
                "LightMode" = "DBufferMesh"
            }
        
            // Render State
            Blend 0 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        Blend 1 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        Blend 2 SrcAlpha OneMinusSrcAlpha, Zero OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        ColorMask RGBA
        ColorMask RGBA 1
        ColorMask RGBA 2
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 3.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        
            // Keywords
            #pragma multi_compile_fragment _ _DBUFFER_MRT1 _DBUFFER_MRT2 _DBUFFER_MRT3
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DBUFFER_MESH
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        
            // HybridV1InjectedBuiltinProperties: <None>
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DBuffer.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 color;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyzw =  input.color;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.color = input.interp4.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 _SampleTexture2D_d8cb4ef1f3be4e35b48ab7a4727a4f98_Texture_1_TexelSize;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        // Object and Global properties
        
            // Graph Functions
            
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
        };
        
        #include "MI_DC_Dragon.hlsl"
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            Input In;
            In.uv_MainTex = IN.uv0.xy;
            In.uv2_Material_Texture2D_0 = IN.uv0.xy;
            In.color = IN.VertexColor;
            In.tangent = float4( 1, 0, 0, 1 );
            In.viewDir = GetWorldSpaceViewDir( IN.WorldSpacePosition );
            In.screenPos = float4( 0, 0, 0, 0 );
            In.worldPos = IN.WorldSpacePosition;
            In.normal2 = IN.WorldSpaceNormal;

            SurfaceOutputStandard Out;
            SurfaceReplacement( In, Out );

            SurfaceDescription surface = (SurfaceDescription)0;
            float3 BaseColor = Out.Albedo;
            if( length( Out.Emission ) > 1 )
                Out.Emission = normalize( Out.Emission );
            surface.BaseColor = BaseColor + Out.Emission;// IsGammaSpace() ? BaseColor : SRGBToLinear( BaseColor );
            surface.NormalTS = Out.Normal;
            //surface.Emission = Out.Emission;
            surface.Metallic = Out.Metallic;
            surface.Smoothness = Out.Smoothness;
            surface.Occlusion = Out.Occlusion;
            surface.Alpha = clamp( Out.Alpha, 0, 1 );
            surface.NormalAlpha = surface.Alpha;
            surface.MAOSAlpha = surface.Alpha;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS =                       input.normalWS;
            const float renormFactor =                          1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal =                           renormFactor*input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition =                         input.positionWS;
            output.uv0 =                                        input.texCoord0;
            output.VertexColor =                                input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            uint2 ComputeFadeMaskSeed(uint2 positionSS)
            {
                uint2 fadeMaskSeed;
        
                // Can't use the view direction, it is the same across the entire screen.
                fadeMaskSeed = positionSS;
        
                return fadeMaskSeed;
            }
        
            #if UNITY_VERSION >= 202130
                void GetSurfaceData( Varyings input, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData )
            #else
                void GetSurfaceData(Varyings input, half3 viewDirectioWS, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            #endif
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                #else
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                        LODDitheringTransition(ComputeFadeMaskSeed(positionSS), unity_LODFade.x);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DecalScreenSpaceMesh"
            Tags 
            { 
                "LightMode" = "DecalScreenSpaceMesh"
            }
        
            // Render State
            Blend SrcAlpha OneMinusSrcAlpha
        ZTest LEqual
        ZWrite Off
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 2.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        
            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
        #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ SHADOWS_SHADOWMASK
        #pragma multi_compile _ _CLUSTERED_RENDERING
        #pragma multi_compile _DECAL_NORMAL_BLEND_LOW _DECAL_NORMAL_BLEND_MEDIUM _DECAL_NORMAL_BLEND_HIGH
        #pragma multi_compile _ _FORWARD_PLUS
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define VARYINGS_NEED_SH
            #define VARYINGS_NEED_STATIC_LIGHTMAP_UV
            #define VARYINGS_NEED_DYNAMIC_LIGHTMAP_UV
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DECAL_SCREEN_SPACE_MESH
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        
            // HybridV1InjectedBuiltinProperties: <None>
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 color;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
             float3 interp5 : INTERP5;
             float2 interp6 : INTERP6;
             float2 interp7 : INTERP7;
             float3 interp8 : INTERP8;
             float4 interp9 : INTERP9;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyzw =  input.color;
            output.interp5.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp6.xy =  input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.interp7.xy =  input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp8.xyz =  input.sh;
            #endif
            output.interp9.xyzw =  input.fogFactorAndVertexLight;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.color = input.interp4.xyzw;
            output.viewDirectionWS = input.interp5.xyz;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.interp6.xy;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.interp7.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp8.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp9.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 _SampleTexture2D_d8cb4ef1f3be4e35b48ab7a4727a4f98_Texture_1_TexelSize;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        // Object and Global properties
        
            // Graph Functions
            
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
        };
        
        #include "MI_DC_Dragon.hlsl"
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            Input In;
            In.uv_MainTex = IN.uv0.xy;
            In.uv2_Material_Texture2D_0 = IN.uv0.xy;
            In.color = IN.VertexColor;
            In.tangent = float4( 1, 0, 0, 1 );
            In.viewDir = GetWorldSpaceViewDir( IN.WorldSpacePosition );
            In.screenPos = float4( 0, 0, 0, 0 );
            In.worldPos = IN.WorldSpacePosition;
            In.normal2 = IN.WorldSpaceNormal;

            SurfaceOutputStandard Out;
            SurfaceReplacement( In, Out );

            SurfaceDescription surface = (SurfaceDescription)0;
            float3 BaseColor = Out.Albedo;
            if( length( Out.Emission ) > 1 )
                Out.Emission = normalize( Out.Emission );
            surface.BaseColor = BaseColor + Out.Emission;// IsGammaSpace() ? BaseColor : SRGBToLinear( BaseColor );
            surface.NormalTS = Out.Normal;
            //surface.Emission = Out.Emission;
            surface.Metallic = Out.Metallic;
            surface.Smoothness = Out.Smoothness;
            surface.Occlusion = Out.Occlusion;
            surface.Alpha = clamp( Out.Alpha, 0, 1 );
            surface.NormalAlpha = surface.Alpha;
            surface.MAOSAlpha = surface.Alpha;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS =                       input.normalWS;
            const float renormFactor =                          1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal =                           renormFactor*input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition =                         input.positionWS;
            output.uv0 =                                        input.texCoord0;
            output.VertexColor =                                input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            uint2 ComputeFadeMaskSeed(uint2 positionSS)
            {
                uint2 fadeMaskSeed;
        
                // Can't use the view direction, it is the same across the entire screen.
                fadeMaskSeed = positionSS;
        
                return fadeMaskSeed;
            }
        
            #if UNITY_VERSION >= 202130
                void GetSurfaceData( Varyings input, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData )
            #else
                void GetSurfaceData(Varyings input, half3 viewDirectioWS, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            #endif
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                #else
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                        LODDitheringTransition(ComputeFadeMaskSeed(positionSS), unity_LODFade.x);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "DecalGBufferMesh"
            Tags 
            { 
                "LightMode" = "DecalGBufferMesh"
            }
        
            // Render State
            Blend 0 SrcAlpha OneMinusSrcAlpha
        Blend 1 SrcAlpha OneMinusSrcAlpha
        Blend 2 SrcAlpha OneMinusSrcAlpha
        Blend 3 SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ColorMask RGB
        ColorMask 0 1
        ColorMask RGB 2
        ColorMask RGB 3
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 3.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        #pragma multi_compile_fog
        
            // Keywords
            #pragma multi_compile _ LIGHTMAP_ON
        #pragma multi_compile _ DYNAMICLIGHTMAP_ON
        #pragma multi_compile _ DIRLIGHTMAP_COMBINED
        #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
        #pragma multi_compile_fragment _ _SHADOWS_SOFT
        #pragma multi_compile _ LIGHTMAP_SHADOW_MIXING
        #pragma multi_compile _ _MIXED_LIGHTING_SUBTRACTIVE
        #pragma multi_compile _DECAL_NORMAL_BLEND_LOW _DECAL_NORMAL_BLEND_MEDIUM _DECAL_NORMAL_BLEND_HIGH
        #pragma multi_compile_fragment _ _GBUFFER_NORMALS_OCT
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            #define ATTRIBUTES_NEED_NORMAL
            #define ATTRIBUTES_NEED_TANGENT
            #define ATTRIBUTES_NEED_TEXCOORD0
            #define ATTRIBUTES_NEED_TEXCOORD1
            #define ATTRIBUTES_NEED_TEXCOORD2
            #define ATTRIBUTES_NEED_COLOR
            #define VARYINGS_NEED_POSITION_WS
            #define VARYINGS_NEED_NORMAL_WS
            #define VARYINGS_NEED_VIEWDIRECTION_WS
            #define VARYINGS_NEED_TANGENT_WS
            #define VARYINGS_NEED_TEXCOORD0
            #define VARYINGS_NEED_COLOR
            #define VARYINGS_NEED_FOG_AND_VERTEX_LIGHT
            #define VARYINGS_NEED_SH
            #define VARYINGS_NEED_STATIC_LIGHTMAP_UV
            #define VARYINGS_NEED_DYNAMIC_LIGHTMAP_UV
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DECAL_GBUFFER_MESH
        #define _MATERIAL_AFFECTS_ALBEDO 1
        #define _MATERIAL_AFFECTS_NORMAL 1
        #define _MATERIAL_AFFECTS_NORMAL_BLEND 1
        #define _MATERIAL_AFFECTS_MAOS 1
        
            // HybridV1InjectedBuiltinProperties: <None>
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/UnityGBuffer.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
             float3 normalOS : NORMAL;
             float4 tangentOS : TANGENT;
             float4 uv0 : TEXCOORD0;
             float4 uv1 : TEXCOORD1;
             float4 uv2 : TEXCOORD2;
             float4 color : COLOR;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
             float3 positionWS;
             float3 normalWS;
             float4 tangentWS;
             float4 texCoord0;
             float4 color;
             float3 viewDirectionWS;
            #if defined(LIGHTMAP_ON)
             float2 staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
             float2 dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
             float3 sh;
            #endif
             float4 fogFactorAndVertexLight;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
             float3 WorldSpaceNormal;
             float3 TangentSpaceNormal;
             float3 WorldSpacePosition;
             float4 uv0;
             float4 VertexColor;
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
             float3 interp0 : INTERP0;
             float3 interp1 : INTERP1;
             float4 interp2 : INTERP2;
             float4 interp3 : INTERP3;
             float4 interp4 : INTERP4;
             float3 interp5 : INTERP5;
             float2 interp6 : INTERP6;
             float2 interp7 : INTERP7;
             float3 interp8 : INTERP8;
             float4 interp9 : INTERP9;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            output.interp0.xyz =  input.positionWS;
            output.interp1.xyz =  input.normalWS;
            output.interp2.xyzw =  input.tangentWS;
            output.interp3.xyzw =  input.texCoord0;
            output.interp4.xyzw =  input.color;
            output.interp5.xyz =  input.viewDirectionWS;
            #if defined(LIGHTMAP_ON)
            output.interp6.xy =  input.staticLightmapUV;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.interp7.xy =  input.dynamicLightmapUV;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.interp8.xyz =  input.sh;
            #endif
            output.interp9.xyzw =  input.fogFactorAndVertexLight;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            output.positionWS = input.interp0.xyz;
            output.normalWS = input.interp1.xyz;
            output.tangentWS = input.interp2.xyzw;
            output.texCoord0 = input.interp3.xyzw;
            output.color = input.interp4.xyzw;
            output.viewDirectionWS = input.interp5.xyz;
            #if defined(LIGHTMAP_ON)
            output.staticLightmapUV = input.interp6.xy;
            #endif
            #if defined(DYNAMICLIGHTMAP_ON)
            output.dynamicLightmapUV = input.interp7.xy;
            #endif
            #if !defined(LIGHTMAP_ON)
            output.sh = input.interp8.xyz;
            #endif
            output.fogFactorAndVertexLight = input.interp9.xyzw;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 _SampleTexture2D_d8cb4ef1f3be4e35b48ab7a4727a4f98_Texture_1_TexelSize;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        // Object and Global properties
        
            // Graph Functions
            
        void Unity_Multiply_float4_float4(float4 A, float4 B, out float4 Out)
        {
            Out = A * B;
        }
        
        void Unity_Multiply_float3_float3(float3 A, float3 B, out float3 Out)
        {
            Out = A * B;
        }
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
            float3 BaseColor;
            float Alpha;
            float3 NormalTS;
            float NormalAlpha;
            float Metallic;
            float Occlusion;
            float Smoothness;
            float MAOSAlpha;
        };
        
        #include "MI_DC_Dragon.hlsl"
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            Input In;
            In.uv_MainTex = IN.uv0.xy;
            In.uv2_Material_Texture2D_0 = IN.uv0.xy;
            In.color = IN.VertexColor;
            In.tangent = float4( 1, 0, 0, 1 );
            In.viewDir = GetWorldSpaceViewDir( IN.WorldSpacePosition );
            In.screenPos = float4( 0, 0, 0, 0 );
            In.worldPos = IN.WorldSpacePosition;
            In.normal2 = IN.WorldSpaceNormal;

            SurfaceOutputStandard Out;
            SurfaceReplacement( In, Out );

            SurfaceDescription surface = (SurfaceDescription)0;
            float3 BaseColor = Out.Albedo;
            if( length( Out.Emission ) > 1 )
                Out.Emission = normalize( Out.Emission );
            surface.BaseColor = BaseColor + Out.Emission;// IsGammaSpace() ? BaseColor : SRGBToLinear( BaseColor );
            surface.NormalTS = Out.Normal;
            //surface.Emission = Out.Emission;
            surface.Metallic = Out.Metallic;
            surface.Smoothness = Out.Smoothness;
            surface.Occlusion = Out.Occlusion;
            surface.Alpha = clamp( Out.Alpha, 0, 1 );
            surface.NormalAlpha = surface.Alpha;
            surface.MAOSAlpha = surface.Alpha;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
            // must use interpolated tangent, bitangent and normal before they are normalized in the pixel shader.
            float3 unnormalizedNormalWS =                       input.normalWS;
            const float renormFactor =                          1.0 / length(unnormalizedNormalWS);
        
        
            output.WorldSpaceNormal =                           renormFactor*input.normalWS.xyz;      // we want a unit length Normal Vector node in shader graph
            output.TangentSpaceNormal =                         float3(0.0f, 0.0f, 1.0f);
        
        
            output.WorldSpacePosition =                         input.positionWS;
            output.uv0 =                                        input.texCoord0;
            output.VertexColor =                                input.color;
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            uint2 ComputeFadeMaskSeed(uint2 positionSS)
            {
                uint2 fadeMaskSeed;
        
                // Can't use the view direction, it is the same across the entire screen.
                fadeMaskSeed = positionSS;
        
                return fadeMaskSeed;
            }
        
            #if UNITY_VERSION >= 202130
                void GetSurfaceData( Varyings input, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData )
            #else
                void GetSurfaceData(Varyings input, half3 viewDirectioWS, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            #endif
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                    input.texCoord0.xy = input.texCoord0.xy * scale + offset;
                #else
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                        LODDitheringTransition(ComputeFadeMaskSeed(positionSS), unity_LODFade.x);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
        
                // copy across graph values, if defined
                surfaceData.baseColor.xyz = half3(surfaceDescription.BaseColor);
                surfaceData.baseColor.w = half(surfaceDescription.Alpha * fadeFactor);
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        surfaceData.normalWS.xyz = mul((half3x3)normalToWorld, surfaceDescription.NormalTS.xyz);
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                        surfaceData.normalWS.xyz = normalize(TransformTangentToWorld(surfaceDescription.NormalTS, tangentToWorld));
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
                surfaceData.normalWS.w = surfaceDescription.NormalAlpha * fadeFactor;
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
                surfaceData.metallic = half(surfaceDescription.Metallic);
                surfaceData.occlusion = half(surfaceDescription.Occlusion);
                surfaceData.smoothness = half(surfaceDescription.Smoothness);
                surfaceData.MAOSAlpha = half(surfaceDescription.MAOSAlpha * fadeFactor);
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
        Pass
        { 
            Name "ScenePickingPass"
            Tags 
            { 
                "LightMode" = "Picking"
            }
        
            // Render State
            Cull Back
        
            // Debug
            // <None>
        
            // --------------------------------------------------
            // Pass
        
            HLSLPROGRAM
        
            // Pragmas
            #pragma target 3.5
        #pragma vertex Vert
        #pragma fragment Frag
        #pragma multi_compile_instancing
        
            // Keywords
            // PassKeywords: <None>
            // GraphKeywords: <None>
        
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        
            // Defines
            
            #define HAVE_MESH_MODIFICATION
        
        
            #define SHADERPASS SHADERPASS_DEPTHONLY
        #define SCENEPICKINGPASS 1
        
            // HybridV1InjectedBuiltinProperties: <None>
        
            // -- Properties used by ScenePickingPass
            #ifdef SCENEPICKINGPASS
            float4 _SelectionID;
            #endif
        
            // Includes
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DecalInput.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderVariablesDecal.hlsl"
        
            // --------------------------------------------------
            // Structs and Packing
        
            struct Attributes
        {
             float3 positionOS : POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : INSTANCEID_SEMANTIC;
            #endif
        };
        struct Varyings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        struct SurfaceDescriptionInputs
        {
        };
        struct VertexDescriptionInputs
        {
        };
        struct PackedVaryings
        {
             float4 positionCS : SV_POSITION;
            #if UNITY_ANY_INSTANCING_ENABLED
             uint instanceID : CUSTOM_INSTANCE_ID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
             uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
             uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
             FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
            #endif
        };
        
            PackedVaryings PackVaryings (Varyings input)
        {
            PackedVaryings output;
            ZERO_INITIALIZE(PackedVaryings, output);
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        Varyings UnpackVaryings (PackedVaryings input)
        {
            Varyings output;
            output.positionCS = input.positionCS;
            #if UNITY_ANY_INSTANCING_ENABLED
            output.instanceID = input.instanceID;
            #endif
            #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
            output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
            #endif
            #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
            output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
            #endif
            #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
            output.cullFace = input.cullFace;
            #endif
            return output;
        }
        
        
            // --------------------------------------------------
            // Graph
        
            // Graph Properties
            CBUFFER_START(UnityPerMaterial)
        float4 _SampleTexture2D_d8cb4ef1f3be4e35b48ab7a4727a4f98_Texture_1_TexelSize;
        float _DrawOrder;
        float _DecalMeshBiasType;
        float _DecalMeshDepthBias;
        float _DecalMeshViewBias;
        CBUFFER_END
        
        // Object and Global properties
        
            // Graph Functions
            // GraphFunctions: <None>
        
            // Graph Vertex
            struct VertexDescription
        {
        };
        
        VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
        {
            VertexDescription description = (VertexDescription)0;
            return description;
        }
            
            // Graph Pixel
            struct SurfaceDescription
        {
        };
        
        SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
        {
            SurfaceDescription surface = (SurfaceDescription)0;
            return surface;
        }
        
            // --------------------------------------------------
            // Build Graph Inputs
        
            
        //     $features.graphVertex:  $include("VertexAnimation.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : VertexAnimation.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            
        //     $features.graphPixel:   $include("SharedCode.template.hlsl")
        //                                       ^ ERROR: $include cannot find file : SharedCode.template.hlsl. Looked into:
        // Packages/com.unity.shadergraph/Editor/Generation/Templates
        
            SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
        {
            SurfaceDescriptionInputs output;
            ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
        
            /* WARNING: $splice Could not find named fragment 'CustomInterpolatorCopyToSDI' */
        
        
        
        
        
        #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN                output.FaceSign =                                   IS_FRONT_VFACE(input.cullFace, true, false);
        #else
        #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        #endif
        #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
        
            return output;
        }
        
            // --------------------------------------------------
            // Build Surface Data
        
            uint2 ComputeFadeMaskSeed(uint2 positionSS)
            {
                uint2 fadeMaskSeed;
        
                // Can't use the view direction, it is the same across the entire screen.
                fadeMaskSeed = positionSS;
        
                return fadeMaskSeed;
            }
        
            #if UNITY_VERSION >= 202130
                void GetSurfaceData( Varyings input, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData )
            #else
                void GetSurfaceData(Varyings input, half3 viewDirectioWS, uint2 positionSS, float angleFadeFactor, out DecalSurfaceData surfaceData)
            #endif
            {
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_FORWARD_EMISSIVE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    half4x4 normalToWorld = UNITY_ACCESS_INSTANCED_PROP(Decal, _NormalToWorld);
                    half fadeFactor = clamp(normalToWorld[0][3], 0.0f, 1.0f) * angleFadeFactor;
                    float2 scale = float2(normalToWorld[3][0], normalToWorld[3][1]);
                    float2 offset = float2(normalToWorld[3][2], normalToWorld[3][3]);
                #else
                    #ifdef LOD_FADE_CROSSFADE // enable dithering LOD transition if user select CrossFade transition in LOD group
                        LODDitheringTransition(ComputeFadeMaskSeed(positionSS), unity_LODFade.x);
                    #endif
        
                    half fadeFactor = half(1.0);
                #endif
        
                SurfaceDescriptionInputs surfaceDescriptionInputs = BuildSurfaceDescriptionInputs(input);
                SurfaceDescription surfaceDescription = SurfaceDescriptionFunction(surfaceDescriptionInputs);
        
                // setup defaults -- these are used if the graph doesn't output a value
                ZERO_INITIALIZE(DecalSurfaceData, surfaceData);
                surfaceData.occlusion = half(1.0);
                surfaceData.smoothness = half(0);
        
                #ifdef _MATERIAL_AFFECTS_NORMAL
                    surfaceData.normalWS.w = half(1.0);
                #else
                    surfaceData.normalWS.w = half(0.0);
                #endif
        
        
                // copy across graph values, if defined
        
                #if (SHADERPASS == SHADERPASS_DBUFFER_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_PROJECTOR) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_PROJECTOR)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                    #else
                        surfaceData.normalWS.xyz = normalToWorld[2].xyz;
                    #endif
                #elif (SHADERPASS == SHADERPASS_DBUFFER_MESH) || (SHADERPASS == SHADERPASS_DECAL_SCREEN_SPACE_MESH) || (SHADERPASS == SHADERPASS_DECAL_GBUFFER_MESH)
                    #if defined(_MATERIAL_AFFECTS_NORMAL)
                        float sgn = input.tangentWS.w;      // should be either +1 or -1
                        float3 bitangent = sgn * cross(input.normalWS.xyz, input.tangentWS.xyz);
                        half3x3 tangentToWorld = half3x3(input.tangentWS.xyz, bitangent.xyz, input.normalWS.xyz);
        
                        // We need to normalize as we use mikkt tangent space and this is expected (tangent space is not normalize)
                    #else
                        surfaceData.normalWS.xyz = half3(input.normalWS); // Default to vertex normal
                    #endif
                #endif
        
        
                // In case of Smoothness / AO / Metal, all the three are always computed but color mask can change
            }
        
            // --------------------------------------------------
            // Main
        
            #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPassDecal.hlsl"
        
            ENDHLSL
        }
    }
    CustomEditorForRenderPipeline "UnityEditor.Rendering.Universal.DecalShaderGraphGUI" "UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset"
    CustomEditor "UnityEditor.ShaderGraph.GenericShaderGraphMaterialGUI"
    FallBack "Hidden/Shader Graph/FallbackError"
}