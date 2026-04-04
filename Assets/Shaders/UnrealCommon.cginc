
// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it uses non-square matrices
#pragma exclude_renderers gles


#define MaterialFloat float
#define MaterialFloat2 float2
#define MaterialFloat3 float3
#define MaterialFloat4 float4
#define MaterialFloat3x3 float3x3
#define MaterialFloat4x4 float4x4 
#define MaterialFloat4x3 float4x3 

#define LOOP UNITY_LOOP
#define UNROLL UNITY_UNROLL
#define BRANCH UNITY_BRANCH

//UE5 Stuff
#define DERIV_BASE_VALUE(_X) _X
float GetInitialisedStrataData() { return 0; }

#ifdef UE5	

	#define LWCADDRESSMODE_CLAMP 0u
	#define LWCADDRESSMODE_WRAP 1u
	#define LWCADDRESSMODE_MIRROR 2u

	float LWCApplyAddressModeWrap( FLWCScalar V )
	{
		// Compute the fractional part of the tile, then add to the offset
		// Let the texture unit apply the final coordinate wrapping, which will allow derivatives to work correctly unless we cross a tile boundary
		const float FracTile = frac( LWCGetTile( V ) * UE_LWC_RENDER_TILE_SIZE );
		return FracTile + V.Offset;
	}

	float LWCApplyAddressModeMirror( FLWCScalar v )
	{
		// Unclear what the best option is for MIRROR
		// We can apply the mirror logic directly, but that will break derivatives
		// We can use similar logic as WRAP, but in that case results will actually be incorrect at tile boundaries (not just wrong derivatives)
		// Or we can convert to float and accept precision loss for large values (but otherwise correct)
		// TODO - something better?

		//float t = LWCFrac(LWCMultiply(v, 0.5f)) * 2.0f;
		//return 1.0f - abs(t - 1.0f);
		return LWCToFloat( v );
	}

	float LWCApplyAddressModeClamp( FLWCScalar v )
	{
		// For the CLAMP case, a simple LWCToFloat() is sufficient.  This will lose a ton of precision for large values, but we don't care about this since the GPU will clamp to [0,1) anyway
		// It's possible certain GPUs might need a special case if the ToFloat() conversion overflows
		return LWCToFloat( v );
	}

	float LWCApplyAddressMode( FLWCScalar v, uint AddressMode )
	{
		if( AddressMode == LWCADDRESSMODE_WRAP ) return LWCApplyAddressModeWrap( v );
		else if( AddressMode == LWCADDRESSMODE_MIRROR ) return LWCApplyAddressModeMirror( v );
		else return LWCApplyAddressModeClamp( v );
	}
	float2 LWCApplyAddressMode( FLWCVector2 UV, uint AddressX, uint AddressY )
	{
		return float2( LWCApplyAddressMode( LWCGetX( UV ), AddressX ), LWCApplyAddressMode( LWCGetY( UV ), AddressY ) );
	}
	float3 LWCApplyAddressMode( FLWCVector3 UV, uint AddressX, uint AddressY, uint AddressZ )
	{
		return float3( LWCApplyAddressMode( LWCGetX( UV ), AddressX ), LWCApplyAddressMode( LWCGetY( UV ), AddressY ), LWCApplyAddressMode( LWCGetZ( UV ), AddressZ ) );
	}
#endif

/*struct FTexCoordScalesParams
{

	int2 PixelPosition;


	float4 OneOverDDU;
	float4 OneOverDDV;


	float MinScale;
	float MaxScale;

	float TexSample;


	float4 ScalesPerIndex;
};*/
#define FTexCoordScalesParams float2

struct FMaterialParticleParameters
{
	/** Relative time [0-1]. */
	half RelativeTime;
	/** Fade amount due to motion blur. */
	half MotionBlurFade;
	/** Random value per particle [0-1]. */
	half Random;
	/** XYZ: Direction, W: Speed. */
	half4 Velocity;
	/** Per-particle color. */
	half4 Color;
	/** Particle translated world space position and size(radius). */
	float4 TranslatedWorldPositionAndSize;
	/** Macro UV scale and bias. */
	half4 MacroUV;
	/** Dynamic parameter used by particle systems. */
	half4 DynamicParameter;
	/** mesh particle orientation */
	float4x4 LocalToWorld;

	#if 1//USE_PARTICLE_SUBUVS
	/** SubUV texture coordinates*/
	MaterialFloat2 SubUVCoords[ 2 ];
	/** SubUV interpolation value*/
	MaterialFloat SubUVLerp;
	#endif

	/** The size of the particle. */
	float2 Size;
};

struct FStrataData//Fake
{
	MaterialFloat3 EmissiveColor;
	MaterialFloat Opacity;
	MaterialFloat3 BaseColor;
	MaterialFloat Metallic;
	MaterialFloat Roughness;
	MaterialFloat3 Normal;
};

struct FPixelMaterialInputs
{
	MaterialFloat3 EmissiveColor;
	MaterialFloat Opacity;
	MaterialFloat OpacityMask;
	MaterialFloat3 BaseColor;
	MaterialFloat Metallic;
	MaterialFloat Specular;
	MaterialFloat Roughness;
	MaterialFloat3 Normal;
	MaterialFloat AmbientOcclusion;	
	MaterialFloat2 Refraction;
	MaterialFloat PixelDepthOffset;
	MaterialFloat Subsurface;
	MaterialFloat ShadingModel;
	
	MaterialFloat Anisotropy;
	MaterialFloat3 Tangent;
	
	MaterialFloat FrontMaterial;
	//5.2
	float SurfaceThickness;
	//5.3
	float Displacement;
};

struct FMaterialTessellationParameters
{
	// Note: Customized UVs are only evaluated in the vertex shader, which is not really what you want with tessellation, but keeps the code simpler 
	// (tessellation texcoords are the same as pixels shader texcoords)
#if NUM_TEX_COORD_INTERPOLATORS
	float2 TexCoords[ NUM_TEX_COORD_INTERPOLATORS ];
#endif
	float4 VertexColor;
	// TODO: Non translated world position
	float3 WorldPosition;
	float3 TangentToWorldPreScale;

	// TangentToWorld[2] is WorldVertexNormal, [0] and [1] are binormal and tangent
	float3x3 TangentToWorld;

	// Index into View.PrimitiveSceneData
	uint PrimitiveId;
};

struct FMaterialVertexParameters
{
	// Position in the translated world (VertexFactoryGetWorldPosition).
	// Previous position in the translated world (VertexFactoryGetPreviousWorldPosition) if
	//    computing material's output for previous frame (See {BasePassVertex,Velocity}Shader.usf).
	float3 WorldPosition;
	// TangentToWorld[2] is WorldVertexNormal
	half3x3 TangentToWorld;
#if USE_INSTANCING
	/** Per-instance properties. */
	float4x4 InstanceLocalToWorld;
	float3 InstanceLocalPosition;
	float4 PerInstanceParams;
	uint InstanceId;
	uint InstanceOffset;

#elif IS_MESHPARTICLE_FACTORY 
	/** Per-particle properties. */
	float4x4 InstanceLocalToWorld;
#endif
	// If either USE_INSTANCING or (IS_MESHPARTICLE_FACTORY && FEATURE_LEVEL >= FEATURE_LEVEL_SM4)
	// is true, PrevFrameLocalToWorld is a per-instance transform
	#ifdef UE5
		FLWCMatrix PrevFrameLocalToWorld;
	#else
		float4x4 PrevFrameLocalToWorld;
	#endif

	float3 PreSkinnedPosition;
	float3 PreSkinnedNormal;

#if GPU_SKINNED_MESH_FACTORY
	float3 PreSkinOffset;
	float3 PostSkinOffset;
#endif

	half4 VertexColor;
#if NUM_MATERIAL_TEXCOORDS_VERTEX
	float2 TexCoords[ NUM_MATERIAL_TEXCOORDS_VERTEX ];
#if ES3_1_PROFILE
	float2 TexCoordOffset; // Offset for UV localization for large UV values
#endif
#endif

	/** Per-particle properties. Only valid for particle vertex factories. */
	FMaterialParticleParameters Particle;

	// Index into View.PrimitiveSceneData
	uint PrimitiveId;

#if WATER_MESH_FACTORY
	uint WaterWaveParamIndex;
#endif
};

struct FStrataPixelFootprint
{
	float PixelRadiusInWorldSpace; // In cm
};
struct FStrataTree
{
	int BSDFCount;
	#if HDRP || URP
	FStrataData PromoteParameterBlendedBSDFToOperator( FStrataData StrataData, int OperatorIndex, int BSDFIndex, int LayerDepth, int bIsBottom )
	{
		return StrataData;
	}
	#endif
	//...
};

#define SHAREDLOCALBASIS_INDEX_0 0
#define SHAREDLOCALBASIS_INDEX_0_0 0
#define SHAREDLOCALBASIS_INDEX_0_1 0
#define STRATA_MAX_SHAREDLOCALBASES_REGISTERS		4


struct FSharedLocalBases
{
	uint Count;
	uint Types;
	float3 Normals[ STRATA_MAX_SHAREDLOCALBASES_REGISTERS ];	// once registered, normals are always world space
	float3 Tangents[ STRATA_MAX_SHAREDLOCALBASES_REGISTERS ];// idem for tangents
};

struct FMaterialPixelParameters
{
	#if NUM_TEX_COORD_INTERPOLATORS
	float2 TexCoords[ NUM_TEX_COORD_INTERPOLATORS ];
	#endif

	/** Interpolated vertex color, in linear color space. */
	half4 VertexColor;//TBD

	/** Normalized world space normal. */
	half3 WorldNormal;

	/** Normalized world space reflected camera vector. */
	half3 ReflectionVector;

	/** Normalized world space camera vector, which is the vector from the point being shaded to the camera position. */
	half3 CameraVector;

	/** World space light vector, only valid when rendering a light function. */
	half3 LightVector;

	/**
	* Like SV_Position (.xy is pixel position at pixel center, z:DeviceZ, .w:SceneDepth)
	* using shader generated value SV_POSITION
	* Note: this is not relative to the current viewport.  RelativePixelPosition = MaterialParameters.SvPosition.xy - View.ViewRectMin.xy;
	*/
	float4 SvPosition;

	/** Post projection position reconstructed from SvPosition, before the divide by W. left..top -1..1, bottom..top -1..1  within the viewport, W is the SceneDepth */
	float4 ScreenPosition;

	half UnMirrored;

	half TwoSidedSign;

	/**
	* Orthonormal rotation-only transform from tangent space to world space
	* The transpose(TangentToWorld) is WorldToTangent, and TangentToWorld[2] is WorldVertexNormal
	*/
	half3x3 TangentToWorld;

	/**
	* Interpolated worldspace position of this pixel
	* todo: Make this TranslatedWorldPosition and also rename the VS/DS/HS WorldPosition to be TranslatedWorldPosition
	*/
	float3 AbsoluteWorldPosition;

	/**
	* Interpolated worldspace position of this pixel, centered around the camera
	*/
	float3 WorldPosition_CamRelative;

	/**
	* Interpolated worldspace position of this pixel, not including any world position offset or displacement.
	* Only valid if shader is compiled with NEEDS_WORLD_POSITION_EXCLUDING_SHADER_OFFSETS, otherwise just contains 0
	*/
	float3 WorldPosition_NoOffsets;

	/**
	* Interpolated worldspace position of this pixel, not including any world position offset or displacement.
	* Only valid if shader is compiled with NEEDS_WORLD_POSITION_EXCLUDING_SHADER_OFFSETS, otherwise just contains 0
	*/
	float3 WorldPosition_NoOffsets_CamRelative;

	/** Offset applied to the lighting position for translucency, used to break up aliasing artifacts. */
	half3 LightingPositionOffset;

	float AOMaterialMask;

	#if LIGHTMAP_UV_ACCESS
	float2	LightmapUVs;
	#endif

	#if USE_INSTANCING
	half4 PerInstanceParams;
	#endif

	// Index into View.PrimitiveSceneData
	uint PrimitiveId;

	/** Per-particle properties. Only valid for particle vertex factories. */
	FMaterialParticleParameters Particle;

	#if (ES2_PROFILE || ES3_1_PROFILE)
	float4 LayerWeights;
	#endif

	#if TEX_COORD_SCALE_ANALYSIS
	/** Parameters used by the MaterialTexCoordScales shader. */
	FTexCoordScalesParams TexCoordScalesParams;
	#endif

	#if POST_PROCESS_MATERIAL && (FEATURE_LEVEL <= FEATURE_LEVEL_ES3_1)
	/** Used in mobile custom pp material to preserve original SceneColor Alpha */
	half BackupSceneColorAlpha;
	#endif

	#if COMPILER_HLSL
	// Workaround for "error X3067: 'GetObjectWorldPosition': ambiguous function call"
	// Which happens when FMaterialPixelParameters and FMaterialVertexParameters have the same number of floats with the HLSL compiler ver 9.29.952.3111
	// Function overload resolution appears to identify types based on how many floats / ints / etc they contain
	uint Dummy;
	#endif

	FTexCoordScalesParams TexCoordScalesParams;

	float3 WorldTangent;

	//Virtual Textures
	float VirtualTextureFeedback;

	FStrataPixelFootprint StrataPixelFootprint;
	FStrataTree StrataTree;
	FSharedLocalBases SharedLocalBases;
};

float4 View_TemporalAAParams;

//To be moved into InitializeExpressions
float UE_Material_PerFrameScalarExpression0;
float UE_Material_PerFrameScalarExpression1;

float3 ToUnrealPos( float3 Pos )
{
	Pos = Pos * 100;
	Pos = Pos.xzy;

	return Pos;
}
MaterialFloat3 ReflectionAboutCustomWorldNormal( FMaterialPixelParameters Parameters, MaterialFloat3 WorldNormal, bool bNormalizeInputNormal )
{
	if( bNormalizeInputNormal )
	{
		WorldNormal = normalize( WorldNormal );
	}

	//return reflect( Parameters.CameraVector, WorldNormal );
	return -Parameters.CameraVector + WorldNormal * dot( WorldNormal, Parameters.CameraVector ) * 2.0;
}
MaterialFloat4 ProcessMaterialColorTextureLookup(MaterialFloat4 TextureValue)
{
#if (ES2_PROFILE || ES3_1_PROFILE) && !METAL_PROFILE // Metal supports sRGB textures
	#if MOBILE_EMULATION
	if( View.MobilePreviewMode > 0.5f)
	{
		// undo HW srgb->lin
		TextureValue.rgb = pow(TextureValue.rgb, 1.0f / 2.2f); // TODO: replace with a more accurate lin -> sRGB conversion.
	}
	#endif

	// sRGB read approximation
	TextureValue.rgb *= TextureValue.rgb;
#endif 
	return TextureValue;
}
float  ProcessMaterialLinearGreyscaleTextureLookup( float  TextureValue )
{
	return TextureValue;
}
MaterialFloat3 TransformTangentVectorToWorld( MaterialFloat3x3 TangentToWorld, MaterialFloat3 InTangentVector )
{
	// Transform directly to world space
	// The vector transform is optimized for this case, only one vector-matrix multiply is needed
	return mul( InTangentVector, TangentToWorld );
}

#if DECAL_PRIMITIVE

float3 TransformTangentNormalToWorld( in FMaterialPixelParameters Parameters, float3 TangentNormal )
{
	// To transform the normals use tranpose(Inverse(DecalToWorld)) = transpose(WorldToDecal)
	// But we want to only rotate the normals (we don't want to non-uniformaly scale them).
	// We assume the matrix is only a scale and rotation, and we remove non-uniform scale:
	float3 lengthSqr = { length2( DecalToWorld._m00_m01_m02 ),
		length2( DecalToWorld._m10_m11_m12 ),
		length2( DecalToWorld._m20_m21_m22 ) };

	float3 scale = rsqrt( lengthSqr );

	// Pre-multiply by the inverse of the non-uniform scale in DecalToWorld
	float4 ScaledNormal = float4( -TangentNormal.z * scale.x, TangentNormal.y * scale.y, TangentNormal.x * scale.z, 0.f );

	// Compute the normal 
	return normalize( mul( ScaledNormal, DecalToWorld ).xyz );
}

#else //DECAL_PRIMITIVE

float3 TransformTangentNormalToWorld( MaterialFloat3x3 TangentToWorld, float3 TangentNormal )
{
	return normalize( float3( TransformTangentVectorToWorld( TangentToWorld, TangentNormal ) ) );
}
float3 TransformTangentNormalToWorld( in FMaterialPixelParameters Parameters, float3 TangentNormal )
{
	return normalize( float3( TransformTangentVectorToWorld( Parameters.TangentToWorld, TangentNormal ) ) );
}

#endif //DECAL_PRIMITIVE

//These 2 are the Unity Normal unpacking functions
/*fixed3 UnpackNormalmapRGorAG( fixed4 packednormal )
{
	// This do the trick
	packednormal.x *= packednormal.w;

	fixed3 normal;
	normal.xy = packednormal.xy * 2 - 1;
	normal.z = sqrt( 1 - saturate( dot( normal.xy, normal.xy ) ) );
	return normal;
}
inline fixed3 UnpackNormal( fixed4 packednormal )
{
#if defined(UNITY_NO_DXT5nm)
	return packednormal.xyz * 2 - 1;
#else
	return UnpackNormalmapRGorAG( packednormal );
#endif
}*/
void swap( inout float x, inout float y ){	float temp = x;	x = y;	y = temp;}
MaterialFloat4 UnpackNormalMap( MaterialFloat4 TextureSample )
{
	float3 Unpacked = UnpackNormal( TextureSample );
	//This is needed for textures that don't have flip Green channel on
	Unpacked.y *= -1;
	//Unpacked.x *= -1;
	//swap( Unpacked.x, Unpacked.y );	
	return MaterialFloat4( Unpacked.xy, Unpacked.z, 1.0f );
}
SamplerState GetMaterialSharedSampler(SamplerState TextureSampler, SamplerState SharedSampler)
{
	return SharedSampler;
}
SamplerState GetMaterialSharedSampler( SamplerState SharedSampler )
{
	return SharedSampler;
}
float3 GetActorWorldPosition()
{
	return transpose( UNITY_MATRIX_M)[ 3 ];
}
float3 GetActorWorldPosition( uint PrimitiveId )
{
	return transpose( UNITY_MATRIX_M)[ 3 ];
}

MaterialFloat4 Texture2DSample(Texture2D Tex, SamplerState Sampler, float2 UV)
{
	UV.y = 1.0 - UV.y;
#if COMPUTESHADER
	return tex2D( Tex, UV, 0);
#else
	#if HDRP || URP
		return SAMPLE_TEXTURE2D(Tex, Sampler, UV);
	#else
		return tex2D(Tex, UV);
	#endif
#endif
}
//Fixed Texture2DSample coming from CustomExpressions compiled in VertexShaders
MaterialFloat4 Texture2DSampleForVS( Texture2D Tex, SamplerState Sampler, float2 UV )
{
#if HDRP || URP
	return SAMPLE_TEXTURE2D_LOD( Tex, Sampler, UV, 0 );
#else
	return tex2Dgrad( Tex, UV, 0, 0 );
#endif
}
MaterialFloat4 Texture2DSampleGrad( Texture2D Tex, SamplerState Sampler, float2 UV, MaterialFloat2 DDX, MaterialFloat2 DDY )
{
	UV.y = 1.0 - UV.y;
	#if HDRP || URP
		return SAMPLE_TEXTURE2D( Tex, Sampler, UV );// , DDX, DDY );
	#else
		return tex2Dgrad( Tex, UV, DDX, DDY );
	#endif
}
MaterialFloat4 Texture2DSampleLevel( Texture2D Tex, SamplerState Sampler, float2 UV, MaterialFloat Mip )
{
	UV.y = 1.0 - UV.y;
	#if HDRP || URP
		return SAMPLE_TEXTURE2D_LOD( Tex, Sampler, UV, Mip );
	#else
		//tex2Dlod( Tex, float3( UV, Mip ) );
		return tex2D( Tex, float3( UV, Mip ) );
	#endif
}
MaterialFloat4 TextureCubeSample( TextureCube Tex, SamplerState Sampler, float3 UV)
{
	//#if COMPUTESHADER
	//	return Tex.SampleLevel(Sampler, UV, 0);
	//#else
	
	//Z Up to Y Up
	//UV = float3( UV.x, UV.z, UV.y );

	#if HDRP || URP
		return SAMPLE_TEXTURECUBE(Tex, Sampler, UV);
	#else
		return texCUBE(Tex, UV);
	#endif
	//#endif
}
#ifdef SHADER_STAGE_RAY_TRACING
	#define RAYTRACING 1
#else
	#define RAYTRACING 0
#endif
MaterialFloat4 TextureCubeSampleBias( TextureCube Tex, SamplerState Sampler, float3 UV, MaterialFloat MipBias )
{
#if USE_FORCE_TEXTURE_MIP
	return texCUBEbias( Tex, float4( UV, 0 ) );
#else
	#if HDRP || URP
		#if RAYTRACING
			return SAMPLE_TEXTURECUBE( Tex, Sampler, UV );
		#else
			return SAMPLE_TEXTURECUBE_BIAS( Tex, Sampler, UV, MipBias );
		#endif
	#else
		return texCUBEbias( Tex, float4( UV, MipBias ) );
	#endif
#endif
}
MaterialFloat4 TextureCubeSampleLevel( TextureCube Tex, SamplerState Sampler, float3 UV, MaterialFloat Mip )
{
	#if HDRP || URP
		return SAMPLE_TEXTURECUBE_LOD( Tex, Sampler, UV, Mip );
	#else
		return texCUBElod( Tex, float4(UV, Mip) );
	#endif
}
MaterialFloat4 Texture2DSampleBias( Texture2D Tex, SamplerState Sampler, float2 UV, float MipBias )
{
	UV.y = 1.0 - UV.y;
#if HDRP || URP
	#if RAYTRACING
		//UV.y = 1.0 - UV.y;
		return SAMPLE_TEXTURE2D( Tex, Sampler, UV );
	#else
		return SAMPLE_TEXTURE2D_BIAS( Tex, Sampler, UV, MipBias );
	#endif
#else
	return tex2Dbias( Tex, float4( UV, 0, MipBias ) );
#endif
}
#if HDRP || URP//3D Textures don't work with BuiltIn
	MaterialFloat4  Texture3DSample( Texture3D Tex, SamplerState Sampler, float3 UV )
	{
		UV.y = 1.0 - UV.y;
		return SAMPLE_TEXTURE3D( Tex, Sampler, UV );
	}
	MaterialFloat4 Texture2DArraySample(Texture2DArray Tex, SamplerState Sampler, float3 UV)
	{
		UV.y = 1.0 - UV.y;
		return SAMPLE_TEXTURE2D_ARRAY(Tex, Sampler, UV.xy, UV.z );
	}
	#if !defined (SHADER_API_GLES3) && !defined (SHADER_API_GLCORE)
	MaterialFloat4 TextureCubeArraySampleBias( TextureCubeArray Tex, SamplerState Sampler, float4 UV, MaterialFloat MipBias )
	{
		#if RAYTRACING
			return SAMPLE_TEXTURECUBE_ARRAY( Tex, Sampler, UV.xyz, UV.w );
		#else
			return SAMPLE_TEXTURECUBE_ARRAY_BIAS( Tex, Sampler, UV.xyz, UV.w, MipBias );
		#endif
	}
	#endif
	
	MaterialFloat4 Texture2DArraySampleGrad( Texture2DArray Tex, SamplerState Sampler, float3 UV, MaterialFloat2 DDX, MaterialFloat2 DDY )
	{
		UV.y = 1.0 - UV.y;
		return SAMPLE_TEXTURE2D_ARRAY_GRAD( Tex, Sampler, UV.xy, UV.z, DDX, DDY );
	}
	MaterialFloat4 Texture2DSampleGrad( Texture2DArray Tex, SamplerState Sampler, float3 UV, MaterialFloat2 DDX, MaterialFloat2 DDY )
	{
		return Texture2DArraySampleGrad( Tex, Sampler, UV, DDX, DDY );
	}
	
#endif

MaterialFloat4 Texture2DSample_Decal( Texture2D Tex, SamplerState Sampler, float2 UV )
{
#if METAL_PROFILE || COMPILER_GLSL_ES3_1
	return Texture2DSampleLevel( Tex, Sampler, UV, 0 );
#else
	return Texture2DSample( Tex, Sampler, UV );
#endif
}
MaterialFloat4 TextureCubeSample_Decal( TextureCube Tex, SamplerState Sampler, float3 UV )
{
#if METAL_PROFILE || COMPILER_GLSL_ES3_1
	return TextureCubeSampleLevel( Tex, Sampler, UV, 0 );
#else
	return TextureCubeSample( Tex, Sampler, UV );
#endif
}
half3 GetMaterialNormalRaw(FPixelMaterialInputs PixelMaterialInputs)
{
	return PixelMaterialInputs.Normal;
}

half3 GetMaterialNormal(FMaterialPixelParameters Parameters, FPixelMaterialInputs PixelMaterialInputs)
{
	half3 RetNormal;

	RetNormal = GetMaterialNormalRaw(PixelMaterialInputs);
		
	#if (USE_EDITOR_SHADERS && !(ES2_PROFILE || ES3_1_PROFILE || ESDEFERRED_PROFILE)) || MOBILE_EMULATION
	{
		// this feature is only needed for development/editor - we can compile it out for a shipping build (see r.CompileShadersForDevelopment cvar help)
		half3 OverrideNormal = View.NormalOverrideParameter.xyz;

		#if !MATERIAL_TANGENTSPACENORMAL
			OverrideNormal = Parameters.TangentToWorld[2] * (1 - View.NormalOverrideParameter.w);
		#endif

		RetNormal = RetNormal * View.NormalOverrideParameter.w + OverrideNormal;
	}
	#endif

	return RetNormal;
}
MaterialFloat PositiveClampedPow( MaterialFloat X, MaterialFloat Y )
{
	return pow( max( X, 0.0f ), Y );
}
MaterialFloat2 PositiveClampedPow( MaterialFloat2 X, MaterialFloat2 Y )
{
	return pow( max( X, MaterialFloat2( 0.0f, 0.0f ) ), Y );
}
MaterialFloat3 PositiveClampedPow( MaterialFloat3 X, MaterialFloat3 Y )
{
	return pow( max( X, MaterialFloat3( 0.0f, 0.0f, 0.0f ) ), Y );
}
MaterialFloat4 PositiveClampedPow( MaterialFloat4 X, MaterialFloat4 Y )
{
	return pow( max( X, MaterialFloat4( 0.0f, 0.0f, 0.0f, 0.0f ) ), Y );
}

float GetPerInstanceRandom( FMaterialVertexParameters Parameters )
{
#if USES_PER_INSTANCE_RANDOM && VF_USE_PRIMITIVE_SCENE_DATA
	return Parameters.PerInstanceRandom;
#else
	return 0.5;
#endif
}
float GetPerInstanceRandom(FMaterialPixelParameters Parameters)
{
#if USE_INSTANCING
	return Parameters.PerInstanceParams.x;
#else
	return 0.5;
#endif
}

MaterialFloat ProcessMaterialGreyscaleTextureLookup( MaterialFloat TextureValue )
{
#if (ES2_PROFILE || ES3_1_PROFILE) && !METAL_PROFILE // Metal supports R8 sRGB
#if MOBILE_EMULATION
	if ( View.MobilePreviewMode > 0.5f )
	{
		// undo HW srgb->lin
		TextureValue = pow( TextureValue, 1.0f / 2.2f ); // TODO: replace with a more accurate lin -> sRGB conversion.
	}
#endif
	// sRGB read approximation
	TextureValue *= TextureValue;
#endif 
	return TextureValue;
}
float3 GetTranslatedWorldPosition( FMaterialVertexParameters Parameters )
{
	return Parameters.WorldPosition;
}

MaterialFloat4 ProcessMaterialLinearColorTextureLookup( MaterialFloat4 TextureValue )
{
	return TextureValue;
}
float  StoreTexCoordScale( in out FTexCoordScalesParams Params, float2 UV, int TextureReferenceIndex )
{
	/*float GPUScaleX = length( ddx( UV ) );
	float GPUScaleY = length( ddy( UV ) );

	if ( TextureReferenceIndex >= 0 && TextureReferenceIndex <  32 )
	{
		float OneOverCPUScale = OneOverCPUTexCoordScales[ TextureReferenceIndex / 4 ][ TextureReferenceIndex % 4 ];

		int TexCoordIndex = TexCoordIndices[ TextureReferenceIndex / 4 ][ TextureReferenceIndex % 4 ];

		float GPUScale = min( GPUScaleX * GetComponent( Params.OneOverDDU, TexCoordIndex ), GPUScaleY * GetComponent( Params.OneOverDDV, TexCoordIndex ) );


		const bool bUpdateMinMax = ( OneOverCPUScale > 0 && ( AnalysisParams.x == -1 || AnalysisParams.x == TextureReferenceIndex ) );
		Params.MinScale = bUpdateMinMax ? min( Params.MinScale, GPUScale * OneOverCPUScale ) : Params.MinScale;
		Params.MaxScale = bUpdateMinMax ? max( Params.MaxScale, GPUScale * OneOverCPUScale ) : Params.MaxScale;


		const bool bUpdateScale = ( AnalysisParams.y  && Params.PixelPosition.x / 32 == TextureReferenceIndex / 4 );
		Params.ScalesPerIndex[ TextureReferenceIndex % 4 ] = bUpdateScale ? min( Params.ScalesPerIndex[ TextureReferenceIndex % 4 ], GPUScale ) : Params.ScalesPerIndex[ TextureReferenceIndex % 4 ];
	}*/
	return 1.f;
}
float  StoreTexSample( in out FTexCoordScalesParams Params, float4 C, int TextureReferenceIndex )
{
	//Params.TexSample = AnalysisParams.x == TextureReferenceIndex ? lerp( .4f, 1.f, saturate( Luminance( C.rgb ) ) ) : Params.TexSample;

	return 1.f;
}
float3 RotateAboutAxis( float4 NormalizedRotationAxisAndAngle, float3 PositionOnAxis, float3 Position )
{

	float3 ClosestPointOnAxis = PositionOnAxis + NormalizedRotationAxisAndAngle.xyz * dot( NormalizedRotationAxisAndAngle.xyz, Position - PositionOnAxis );

	float3 UAxis = Position - ClosestPointOnAxis;
	float3 VAxis = cross( NormalizedRotationAxisAndAngle.xyz, UAxis );
	float CosAngle;
	float SinAngle;
	sincos( NormalizedRotationAxisAndAngle.w, SinAngle, CosAngle );

	float3 R = UAxis * CosAngle + VAxis * SinAngle;

	float3 RotatedPosition = ClosestPointOnAxis + R;

	return RotatedPosition - Position;
}
float2 SvPositionToBufferUV( float4 SvPosition )
{
	return SvPosition.xy * View_BufferSizeAndInvSize.zw;
}
float2 GetSceneTextureUV( FMaterialPixelParameters Parameters )
{
	return SvPositionToBufferUV( Parameters.SvPosition );
}
MaterialFloat UnMirror( MaterialFloat Coordinate, FMaterialPixelParameters Parameters )
{
	return ( ( Coordinate )*( Parameters.UnMirrored )*0.5 + 0.5 );
}
MaterialFloat2 UnMirrorU( MaterialFloat2 UV, FMaterialPixelParameters Parameters )
{
	return MaterialFloat2( UnMirror( UV.x, Parameters ), UV.y );
}
MaterialFloat2 UnMirrorV( MaterialFloat2 UV, FMaterialPixelParameters Parameters )
{
	return MaterialFloat2( UV.x, UnMirror( UV.y, Parameters ) );
}
MaterialFloat2 UnMirrorUV( MaterialFloat2 UV, FMaterialPixelParameters Parameters )
{
	return MaterialFloat2( UnMirror( UV.x, Parameters ), UnMirror( UV.y, Parameters ) );
}
float4 GetScreenPosition( FMaterialPixelParameters Parameters )
{
	return Parameters.ScreenPosition;
}
float4 GetScreenPosition( FMaterialVertexParameters Parameters )
{
	return mul( float4( Parameters.WorldPosition, 1.0f ), ResolvedView.TranslatedWorldToClip );
}
float GetPixelDepth(FMaterialPixelParameters Parameters)
{
	//FLATTEN
	//if (View.ViewToClip[3][3] < 1.0f)
	//{
		// Perspective
		return GetScreenPosition(Parameters).w;
	//}
	//else
	//{
	//	// Ortho
	//	return ConvertFromDeviceZ(GetScreenPosition(Parameters).z);
	//}
}
float GetPixelDepth(FMaterialVertexParameters Parameters)
{
	//FLATTEN
	//if (View.ViewToClip[3][3] < 1.0f)
	//{
		// Perspective
		return GetScreenPosition(Parameters).w;
	//}
	//else
	//{
	//	// Ortho
	//	return ConvertFromDeviceZ(GetScreenPosition(Parameters).z);
	//}
}
uint Mod( uint a, uint b )
{

	return a % b;
}

uint2 Mod( uint2 a, uint2 b )
{

	return a % b;
}

uint3 Mod( uint3 a, uint3 b )
{

	return a % b;
}
float CalcSceneDepth( float2 ScreenUV )
{
	return 500;//"Random" value in world units
}
float CalcSceneDepth( uint2 PixelPos )
{
	return 500;//"Random" value in world units
}
float MaterialExpressionSceneDepthWithoutWater( float2 ViewportUV, float FallbackDepth )
{
	return FallbackDepth;
}
float2 ScreenPositionToBufferUV( float4 ScreenPosition )
{
	float4 View_ScreenPositionScaleBias = float4( 1, 1, 0, 0 );//TODO

	return float2( ScreenPosition.xy / ScreenPosition.w * View_ScreenPositionScaleBias.xy + View_ScreenPositionScaleBias.wz );
}
float2  ScreenAlignedPosition( float4 ScreenPosition )
{
	return  float2 ( ScreenPositionToBufferUV( ScreenPosition ) );
}
float2 GetSceneTextureUV( FMaterialVertexParameters Parameters )
{
	return ScreenAlignedPosition( GetScreenPosition( Parameters ) );
}
float2 BufferUVToViewportUV( float2 BufferUV )
{
	float2 PixelPos = BufferUV.xy * View_BufferSizeAndInvSize.xy - View.ViewRectMin.xy;
	return PixelPos.xy * View.ViewSizeAndInvSize.zw;
}
float2 GetViewportUV( FMaterialVertexParameters Parameters )
{
#if POST_PROCESS_MATERIAL
	return Parameters.WorldPosition.xy;
#else
	return BufferUVToViewportUV( GetSceneTextureUV( Parameters ) );
#endif
}
float2 GetPixelPosition( FMaterialVertexParameters Parameters )
{
	return GetViewportUV( Parameters ) * View.ViewSizeAndInvSize.xy;
}
float4 VoronoiCompare(float4 minval, float3 candidate, float3 offset, bool bDistanceOnly)
{
	if (bDistanceOnly)
	{
		return float4( 0, 0, 0, min(minval.w, dot(offset, offset)) );
	}
	else
	{
		float newdist = dot(offset, offset);
		return newdist > minval.w ? minval : float4( candidate, newdist );
	}
}

uint3 Rand3DPCG16(int3 p)
{
	// taking a signed int then reinterpreting as unsigned gives good behavior for negatives
	uint3 v = uint3( p );

	// Linear congruential step. These LCG constants are from Numerical Recipies
	// For additional #'s, PCG would do multiple LCG steps and scramble each on output
	// So v here is the RNG state
	v = v * 1664525u + 1013904223u;

	// PCG uses xorshift for the final shuffle, but it is expensive (and cheap
	// versions of xorshift have visible artifacts). Instead, use simple MAD Feistel steps
	//
	// Feistel ciphers divide the state into separate parts (usually by bits)
	// then apply a series of permutation steps one part at a time. The permutations
	// use a reversible operation (usually ^) to part being updated with the result of
	// a permutation function on the other parts and the key.
	//
	// In this case, I'm using v.x, v.y and v.z as the parts, using + instead of ^ for
	// the combination function, and just multiplying the other two parts (no key) for 
	// the permutation function.
	//
	// That gives a simple mad per round.
	v.x += v.y*v.z;
	v.y += v.z*v.x;
	v.z += v.x*v.y;
	v.x += v.y*v.z;
	v.y += v.z*v.x;
	v.z += v.x*v.y;

	// only top 16 bits are well shuffled
	return v >> 16u;
}
float3 VoronoiCornerSample(float3 pos, int Quality)
{
	// random values in [-0.5, 0.5]
	float3 noise = float3( Rand3DPCG16(int3( pos )) ) / 0xffff - 0.5;

	// quality level 1 or 2: searches a 2x2x2 neighborhood with points distributed on a sphere
	// scale factor to guarantee jittered points will be found within a 2x2x2 search
	if (Quality <= 2)
	{
		return normalize(noise) * 0.2588;
	}

	// quality level 3: searches a 3x3x3 neighborhood with points distributed on a sphere
	// scale factor to guarantee jittered points will be found within a 3x3x3 search
	if (Quality == 3)
	{
		return normalize(noise) * 0.3090;
	}

	// quality level 4: jitter to anywhere in the cell, needs 4x4x4 search
	return noise;
}
float3 NoiseTileWrap(float3 v, bool bTiling, float RepeatSize)
{
	return bTiling ? ( frac(v / RepeatSize) * RepeatSize ) : v;
}
// 220 instruction Worley noise
float4 VoronoiNoise3D_ALU(float3 v, int Quality, bool bTiling, float RepeatSize, bool bDistanceOnly)
{
	float3 fv = frac(v), fv2 = frac(v + 0.5);
	float3 iv = floor(v), iv2 = floor(v + 0.5);

	// with initial minimum distance = infinity (or at least bigger than 4), first min is optimized away
	float4 mindist = float4( 0, 0, 0, 100 );
	float3 p, offset;

	// quality level 3: do a 3x3x3 search
	if (Quality == 3)
	{
		UNROLL for (offset.x = -1; offset.x <= 1; ++offset.x)
		{
			UNROLL for (offset.y = -1; offset.y <= 1; ++offset.y)
			{
				UNROLL for (offset.z = -1; offset.z <= 1; ++offset.z)
				{
					p = offset + VoronoiCornerSample(NoiseTileWrap(iv2 + offset, bTiling, RepeatSize), Quality);
					mindist = VoronoiCompare(mindist, iv2 + p, fv2 - p, bDistanceOnly);
				}
			}
		}
	}

	// everybody else searches a base 2x2x2 neighborhood
	else
	{
		UNROLL for (offset.x = 0; offset.x <= 1; ++offset.x)
		{
			UNROLL for (offset.y = 0; offset.y <= 1; ++offset.y)
			{
				UNROLL for (offset.z = 0; offset.z <= 1; ++offset.z)
				{
					p = offset + VoronoiCornerSample(NoiseTileWrap(iv + offset, bTiling, RepeatSize), Quality);
					mindist = VoronoiCompare(mindist, iv + p, fv - p, bDistanceOnly);

					// quality level 2, do extra set of points, offset by half a cell
					if (Quality == 2)
					{
						// 467 is just an offset to a different area in the random number field to avoid similar neighbor artifacts
						p = offset + VoronoiCornerSample(NoiseTileWrap(iv2 + offset, bTiling, RepeatSize) + 467, Quality);
						mindist = VoronoiCompare(mindist, iv2 + p, fv2 - p, bDistanceOnly);
					}
				}
			}
		}
	}

	// quality level 4: add extra sets of four cells in each direction
	if (Quality >= 4)
	{
		UNROLL for (offset.x = -1; offset.x <= 2; offset.x += 3)
		{
			UNROLL for (offset.y = 0; offset.y <= 1; ++offset.y)
			{
				UNROLL for (offset.z = 0; offset.z <= 1; ++offset.z)
				{
					// along x axis
					p = offset.xyz + VoronoiCornerSample(NoiseTileWrap(iv + offset.xyz, bTiling, RepeatSize), Quality);
					mindist = VoronoiCompare(mindist, iv + p, fv - p, bDistanceOnly);

					// along y axis
					p = offset.yzx + VoronoiCornerSample(NoiseTileWrap(iv + offset.yzx, bTiling, RepeatSize), Quality);
					mindist = VoronoiCompare(mindist, iv + p, fv - p, bDistanceOnly);

					// along z axis
					p = offset.zxy + VoronoiCornerSample(NoiseTileWrap(iv + offset.zxy, bTiling, RepeatSize), Quality);
					mindist = VoronoiCompare(mindist, iv + p, fv - p, bDistanceOnly);
				}
			}
		}
	}

	// transform squared distance to real distance
	return float4( mindist.xyz, sqrt(mindist.w) );
}
float4x3 SimplexCorners( float3 v )
{
	// find base corner by skewing to tetrahedral space and back
	float3 tet = floor( v + v.x / 3 + v.y / 3 + v.z / 3 );
	float3 base = tet - tet.x / 6 - tet.y / 6 - tet.z / 6;
	float3 f = v - base;

	// Find offsets to other corners (McEwan did this in tetrahedral space,
	// but since skew is along x=y=z axis, this works in Euclidean space too.)
	float3 g = step( f.yzx, f.xyz ), h = 1 - g.zxy;
	float3 a1 = min( g, h ) - 1. / 6., a2 = max( g, h ) - 1. / 3.;

	// four corners
	return float4x3( base, base + a1, base + a2, base + 0.5 );
}
float4 SimplexSmooth( float4x3 f )
{
	const float scale = 1024. / 375.;	// scale factor to make noise -1..1
	float4 d = float4( dot( f[ 0 ], f[ 0 ] ), dot( f[ 1 ], f[ 1 ] ), dot( f[ 2 ], f[ 2 ] ), dot( f[ 3 ], f[ 3 ] ) );
	float4 s = saturate( 2 * d );
	return ( 1 * scale + s * ( -3 * scale + s * ( 3 * scale - s * scale ) ) );
}
float3x4 SimplexDSmooth( float4x3 f )
{
	const float scale = 1024. / 375.;	// scale factor to make noise -1..1
	float4 d = float4( dot( f[ 0 ], f[ 0 ] ), dot( f[ 1 ], f[ 1 ] ), dot( f[ 2 ], f[ 2 ] ), dot( f[ 3 ], f[ 3 ] ) );
	float4 s = saturate( 2 * d );
	s = -12 * scale + s * ( 24 * scale - s * 12 * scale );

	return float3x4(
		s * float4( f[ 0 ][ 0 ], f[ 1 ][ 0 ], f[ 2 ][ 0 ], f[ 3 ][ 0 ] ),
		s * float4( f[ 0 ][ 1 ], f[ 1 ][ 1 ], f[ 2 ][ 1 ], f[ 3 ][ 1 ] ),
		s * float4( f[ 0 ][ 2 ], f[ 1 ][ 2 ], f[ 2 ][ 2 ], f[ 3 ][ 2 ] ) );
}
#define MGradientMask int3(0x8000, 0x4000, 0x2000)
#define MGradientScale float3(1. / 0x4000, 1. / 0x2000, 1. / 0x1000)
float3x4 JacobianSimplex_ALU( float3 v, bool bTiling, float RepeatSize )
{
	// corners of tetrahedron
	float4x3 T = SimplexCorners( v );
	uint3 rand;
	float4x3 gvec[ 3 ], fv;
	float3x4 grad;

	// processing of tetrahedral vertices, unrolled
	// to compute gradient at each corner
	fv[ 0 ] = v - T[ 0 ];
	rand = Rand3DPCG16( int3( floor( NoiseTileWrap( 6 * T[ 0 ] + 0.5, bTiling, RepeatSize ) ) ) );
	gvec[ 0 ][ 0 ] = float3( rand.xxx & MGradientMask ) * MGradientScale - 1;
	gvec[ 1 ][ 0 ] = float3( rand.yyy & MGradientMask ) * MGradientScale - 1;
	gvec[ 2 ][ 0 ] = float3( rand.zzz & MGradientMask ) * MGradientScale - 1;
	grad[ 0 ][ 0 ] = dot( gvec[ 0 ][ 0 ], fv[ 0 ] );
	grad[ 1 ][ 0 ] = dot( gvec[ 1 ][ 0 ], fv[ 0 ] );
	grad[ 2 ][ 0 ] = dot( gvec[ 2 ][ 0 ], fv[ 0 ] );

	fv[ 1 ] = v - T[ 1 ];
	rand = Rand3DPCG16( int3( floor( NoiseTileWrap( 6 * T[ 1 ] + 0.5, bTiling, RepeatSize ) ) ) );
	gvec[ 0 ][ 1 ] = float3( rand.xxx & MGradientMask ) * MGradientScale - 1;
	gvec[ 1 ][ 1 ] = float3( rand.yyy & MGradientMask ) * MGradientScale - 1;
	gvec[ 2 ][ 1 ] = float3( rand.zzz & MGradientMask ) * MGradientScale - 1;
	grad[ 0 ][ 1 ] = dot( gvec[ 0 ][ 1 ], fv[ 1 ] );
	grad[ 1 ][ 1 ] = dot( gvec[ 1 ][ 1 ], fv[ 1 ] );
	grad[ 2 ][ 1 ] = dot( gvec[ 2 ][ 1 ], fv[ 1 ] );

	fv[ 2 ] = v - T[ 2 ];
	rand = Rand3DPCG16( int3( floor( NoiseTileWrap( 6 * T[ 2 ] + 0.5, bTiling, RepeatSize ) ) ) );
	gvec[ 0 ][ 2 ] = float3( rand.xxx & MGradientMask ) * MGradientScale - 1;
	gvec[ 1 ][ 2 ] = float3( rand.yyy & MGradientMask ) * MGradientScale - 1;
	gvec[ 2 ][ 2 ] = float3( rand.zzz & MGradientMask ) * MGradientScale - 1;
	grad[ 0 ][ 2 ] = dot( gvec[ 0 ][ 2 ], fv[ 2 ] );
	grad[ 1 ][ 2 ] = dot( gvec[ 1 ][ 2 ], fv[ 2 ] );
	grad[ 2 ][ 2 ] = dot( gvec[ 2 ][ 2 ], fv[ 2 ] );

	fv[ 3 ] = v - T[ 3 ];
	rand = Rand3DPCG16( int3( floor( NoiseTileWrap( 6 * T[ 3 ] + 0.5, bTiling, RepeatSize ) ) ) );
	gvec[ 0 ][ 3 ] = float3( rand.xxx & MGradientMask ) * MGradientScale - 1;
	gvec[ 1 ][ 3 ] = float3( rand.yyy & MGradientMask ) * MGradientScale - 1;
	gvec[ 2 ][ 3 ] = float3( rand.zzz & MGradientMask ) * MGradientScale - 1;
	grad[ 0 ][ 3 ] = dot( gvec[ 0 ][ 3 ], fv[ 3 ] );
	grad[ 1 ][ 3 ] = dot( gvec[ 1 ][ 3 ], fv[ 3 ] );
	grad[ 2 ][ 3 ] = dot( gvec[ 2 ][ 3 ], fv[ 3 ] );

	// blend gradients
	float4 sv = SimplexSmooth( fv );
	float3x4 ds = SimplexDSmooth( fv );

	float3x4 jacobian;
	jacobian[ 0 ] = float4( mul( sv, gvec[ 0 ] ) + mul( ds, grad[ 0 ] ), dot( sv, grad[ 0 ] ) );
	jacobian[ 1 ] = float4( mul( sv, gvec[ 1 ] ) + mul( ds, grad[ 1 ] ), dot( sv, grad[ 1 ] ) );
	jacobian[ 2 ] = float4( mul( sv, gvec[ 2 ] ) + mul( ds, grad[ 2 ] ), dot( sv, grad[ 2 ] ) );

	return jacobian;
}
float Noise3D_Multiplexer(int Function, float3 Position, int Quality, bool bTiling, uint RepeatSize)
{
	// verified, HLSL compiled out the switch if Function is a constant
	//switch (Function)
	//{
	//	case 0:
	//		return SimplexNoise3D_TEX(Position);
	//	case 1:
	//		return GradientNoise3D_TEX(Position, bTiling, RepeatSize);
	//	case 2:
	//		return FastGradientPerlinNoise3D_TEX(Position);
	//	case 3:
	//		return GradientNoise3D_ALU(Position, bTiling, RepeatSize);
	//	case 4:
	//		return ValueNoise3D_ALU(Position, bTiling, RepeatSize);
		//default:
	return VoronoiNoise3D_ALU(Position, Quality, bTiling, RepeatSize, true).w * 2. - 1.;
	//}
	//return 0;
}

// @param LevelScale usually 2 but higher values allow efficient use of few levels
// @return in user defined range (OutputMin..OutputMax)
MaterialFloat MaterialExpressionNoise(float3 Position, float Scale, int Quality, int Function, bool bTurbulence, uint Levels, float OutputMin, float OutputMax, float LevelScale, float FilterWidth, bool bTiling, float RepeatSize)
{
	Position *= Scale;
	FilterWidth *= Scale;

	float Out = 0.0f;
	float OutScale = 1.0f;
	float InvLevelScale = 1.0f / LevelScale;

	LOOP for (uint i = 0; i < Levels; ++i)
	{
		// fade out noise level that are too high frequent (not done through dynamic branching as it usually requires gradient instructions)
		OutScale *= saturate(1.0 - FilterWidth);

		if (bTurbulence)
		{
			Out += abs(Noise3D_Multiplexer(Function, Position, Quality, bTiling, RepeatSize)) * OutScale;
		}
		else
		{
			Out += Noise3D_Multiplexer(Function, Position, Quality, bTiling, RepeatSize) * OutScale;
		}

		Position *= LevelScale;
		RepeatSize *= LevelScale;
		OutScale *= InvLevelScale;
		FilterWidth *= LevelScale;
	}

	if (!bTurbulence)
	{
		// bring -1..1 to 0..1 range
		Out = Out * 0.5f + 0.5f;
	}

	// Out is in 0..1 range
	return lerp(OutputMin, OutputMax, Out);
}

#define PRIMITIVE_SCENE_DATA_FLAG_EVALUATE_WORLD_POSITION_OFFSET		0x1000000
#define NUM_CUSTOM_PRIMITIVE_DATA 8 // Num float4s used for custom data. Must match FCustomPrimitiveData::NumCustomPrimitiveDataFloat4s in SceneTypes.h

#ifdef UE5
	struct FPrimitiveSceneData
	{
		uint		Flags; // TODO: Use 16 bits?
		int			InstanceSceneDataOffset; // Link to the range of instances that belong to this primitive
		int			NumInstanceSceneDataEntries;
		int			PersistentPrimitiveIndex;
		uint		SingleCaptureIndex; // TODO: Use 16 bits? 8 bits?
		float3		TilePosition;
		uint		PrimitiveComponentId; // TODO: Refactor to use PersistentPrimitiveIndex, ENGINE USE ONLY - will be removed
		FLWCMatrix	LocalToWorld;
		FLWCInverseMatrix WorldToLocal;
		FLWCMatrix	PreviousLocalToWorld;
		FLWCInverseMatrix PreviousWorldToLocal;
		float3		InvNonUniformScale;
		float		ObjectBoundsX;
		FLWCVector3	ObjectWorldPosition;
		FLWCVector3	ActorWorldPosition;
		float		ObjectRadius;
		uint		LightmapUVIndex;   // TODO: Use 16 bits? // TODO: Move into associated array that disappears if static lighting is disabled
		float3		ObjectOrientation; // TODO: More efficient representation?
		uint		LightmapDataIndex; // TODO: Use 16 bits? // TODO: Move into associated array that disappears if static lighting is disabled
		float4		NonUniformScale;
		float3		PreSkinnedLocalBoundsMin;
		uint		NaniteResourceID;
		float3		PreSkinnedLocalBoundsMax;
		uint		NaniteHierarchyOffset;
		float3		LocalObjectBoundsMin;
		float		ObjectBoundsY;
		float3		LocalObjectBoundsMax;
		float		ObjectBoundsZ;
		uint		InstancePayloadDataOffset;
		uint		InstancePayloadDataStride; // TODO: Use 16 bits? 8 bits?
		float3		InstanceLocalBoundsCenter;
		float3		InstanceLocalBoundsExtent;
		float3		WireframeColor; // TODO: Should refactor out all editor data into a separate buffer
		float3		LevelColor; // TODO: Should refactor out all editor data into a separate buffer
		uint		NaniteImposterIndex;
		float4		CustomPrimitiveData[ NUM_CUSTOM_PRIMITIVE_DATA ]; // TODO: Move to associated array to shrink primitive data and pack cachelines more effectively
	};
#else
	struct FPrimitiveSceneData
	{
		float4x4 LocalToWorld;
		float4 InvNonUniformScaleAndDeterminantSign;
		float4 ObjectWorldPositionAndRadius;
		float4x4 WorldToLocal;
		float4x4 PreviousLocalToWorld;
		float4x4 PreviousWorldToLocal;
		float3 ActorWorldPosition;
		float UseSingleSampleShadowFromStationaryLights;
		float3 ObjectBounds;
		float LpvBiasMultiplier;
		float DecalReceiverMask;
		float PerObjectGBufferData;
		float UseVolumetricLightmapShadowFromStationaryLights;
		float DrawsVelocity;
		float4 ObjectOrientation;
		float4 NonUniformScale;
		float3 LocalObjectBoundsMin;
		uint LightingChannelMask;
		float3 LocalObjectBoundsMax;
		uint LightmapDataIndex;
		float3 PreSkinnedLocalBoundsMin;
		int SingleCaptureIndex;
		float3 PreSkinnedLocalBoundsMax;
		uint OutputVelocity;
		float4 CustomPrimitiveData[ NUM_CUSTOM_PRIMITIVE_DATA ];
	};
#endif

// Stride of a single primitive's data in float4's, must match C++
#define PRIMITIVE_SCENE_DATA_STRIDE 35

// Fetch from scene primitive buffer
FPrimitiveSceneData GetPrimitiveData( uint PrimitiveId )
{
	// Note: layout must match FPrimitiveSceneShaderData in C++
	// Relying on optimizer to remove unused loads

	FPrimitiveSceneData PrimitiveData;
#ifdef UE5
	PrimitiveData.Flags = PRIMITIVE_SCENE_DATA_FLAG_EVALUATE_WORLD_POSITION_OFFSET;//Always enable WPO for our shaders
#endif
	uint PrimitiveBaseOffset = PrimitiveId * PRIMITIVE_SCENE_DATA_STRIDE;

	float4x4 LocalToWorld;
	LocalToWorld[ 0 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 0 ];
	LocalToWorld[ 1 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 1 ];
	LocalToWorld[ 2 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 2 ];
	LocalToWorld[ 3 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 3 ];

	#ifdef UE5
		PrimitiveData.InvNonUniformScale = View.PrimitiveSceneData[ PrimitiveBaseOffset + 4 ].xyz;
		PrimitiveData.ObjectWorldPosition = LWCPromote( View.PrimitiveSceneData[ PrimitiveBaseOffset + 5 ].xyz );
		PrimitiveData.ActorWorldPosition = LWCPromote( View.PrimitiveSceneData[ PrimitiveBaseOffset + 18 ].xyz );
		PrimitiveData.ObjectBoundsX = View.PrimitiveSceneData[ PrimitiveBaseOffset + 19 ].x;
		PrimitiveData.ObjectBoundsY = View.PrimitiveSceneData[ PrimitiveBaseOffset + 19 ].y;
		PrimitiveData.ObjectBoundsZ = View.PrimitiveSceneData[ PrimitiveBaseOffset + 19 ].z;

		PrimitiveData.InstanceSceneDataOffset = 0;
		PrimitiveData.NumInstanceSceneDataEntries = 0;
		PrimitiveData.PersistentPrimitiveIndex = 0;
		PrimitiveData.TilePosition = float3( 0, 0, 0 );
		PrimitiveData.PrimitiveComponentId = 0;
		PrimitiveData.ObjectRadius = 100.0;
		PrimitiveData.LightmapUVIndex = 0;
		PrimitiveData.NaniteResourceID = 0;
		PrimitiveData.NaniteHierarchyOffset = 0;
		PrimitiveData.InstancePayloadDataOffset = 0;
		PrimitiveData.InstancePayloadDataStride = 0;
		PrimitiveData.InstanceLocalBoundsCenter = float3( 0, 0, 0 );
		PrimitiveData.InstanceLocalBoundsExtent = float3( 0, 0, 0 );
		PrimitiveData.WireframeColor = float3( 1, 1, 1 );
		PrimitiveData.LevelColor = float3( 1, 1, 1 );
		PrimitiveData.NaniteImposterIndex = 0;
	#else
		PrimitiveData.InvNonUniformScaleAndDeterminantSign = View.PrimitiveSceneData[ PrimitiveBaseOffset + 4 ];
		PrimitiveData.ObjectWorldPositionAndRadius = View.PrimitiveSceneData[ PrimitiveBaseOffset + 5 ];
		PrimitiveData.ActorWorldPosition = View.PrimitiveSceneData[ PrimitiveBaseOffset + 18 ].xyz;
		PrimitiveData.UseSingleSampleShadowFromStationaryLights = View.PrimitiveSceneData[ PrimitiveBaseOffset + 18 ].w;
		PrimitiveData.ObjectBounds = View.PrimitiveSceneData[ PrimitiveBaseOffset + 19 ].xyz;
		PrimitiveData.LpvBiasMultiplier = View.PrimitiveSceneData[ PrimitiveBaseOffset + 19 ].w;
		PrimitiveData.DecalReceiverMask = View.PrimitiveSceneData[ PrimitiveBaseOffset + 20 ].x;
		PrimitiveData.PerObjectGBufferData = View.PrimitiveSceneData[ PrimitiveBaseOffset + 20 ].y;
		PrimitiveData.UseVolumetricLightmapShadowFromStationaryLights = View.PrimitiveSceneData[ PrimitiveBaseOffset + 20 ].z;
		PrimitiveData.DrawsVelocity = View.PrimitiveSceneData[ PrimitiveBaseOffset + 20 ].w;
		PrimitiveData.LightingChannelMask = asuint( View.PrimitiveSceneData[ PrimitiveBaseOffset + 23 ].w );
		PrimitiveData.OutputVelocity = asuint( View.PrimitiveSceneData[ PrimitiveBaseOffset + 26 ].w );
	#endif
	
	float4x4 WorldToLocal;
	WorldToLocal[ 0 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 6 ];
	WorldToLocal[ 1 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 7 ];
	WorldToLocal[ 2 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 8 ];
	WorldToLocal[ 3 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 9 ];

	float4x4 PreviousLocalToWorld;
	PreviousLocalToWorld[ 0 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 10 ];
	PreviousLocalToWorld[ 1 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 11 ];
	PreviousLocalToWorld[ 2 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 12 ];
	PreviousLocalToWorld[ 3 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 13 ];

	float4x4 PreviousWorldToLocal;
	PreviousWorldToLocal[ 0 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 14 ];
	PreviousWorldToLocal[ 1 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 15 ];
	PreviousWorldToLocal[ 2 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 16 ];
	PreviousWorldToLocal[ 3 ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 17 ];

	#ifdef UE5
		PrimitiveData.LocalToWorld = MakeLWCMatrix( float3(0,0,0), LocalToWorld );
		PrimitiveData.WorldToLocal = MakeLWCInverseMatrix( float3( 0, 0, 0 ), WorldToLocal );
		PrimitiveData.PreviousLocalToWorld = MakeLWCMatrix( float3( 0, 0, 0 ), PreviousLocalToWorld );
		PrimitiveData.PreviousWorldToLocal = MakeLWCInverseMatrix( float3( 0, 0, 0 ), PreviousWorldToLocal );
	#else
		PrimitiveData.LocalToWorld = LocalToWorld;
		PrimitiveData.WorldToLocal = WorldToLocal;
		PrimitiveData.PreviousLocalToWorld = PreviousLocalToWorld;
		PrimitiveData.PreviousWorldToLocal = PreviousWorldToLocal;
	#endif

	

	PrimitiveData.ObjectOrientation = View.PrimitiveSceneData[ PrimitiveBaseOffset + 21 ];
	PrimitiveData.NonUniformScale = View.PrimitiveSceneData[ PrimitiveBaseOffset + 22 ];

	PrimitiveData.LocalObjectBoundsMin = View.PrimitiveSceneData[ PrimitiveBaseOffset + 23 ].xyz;
	

	PrimitiveData.LocalObjectBoundsMax = View.PrimitiveSceneData[ PrimitiveBaseOffset + 24 ].xyz;
	PrimitiveData.LightmapDataIndex = asuint( View.PrimitiveSceneData[ PrimitiveBaseOffset + 24 ].w );

	PrimitiveData.PreSkinnedLocalBoundsMin = View.PrimitiveSceneData[ PrimitiveBaseOffset + 25 ].xyz;
	PrimitiveData.SingleCaptureIndex = asuint( View.PrimitiveSceneData[ PrimitiveBaseOffset + 25 ].w );

	PrimitiveData.PreSkinnedLocalBoundsMax = View.PrimitiveSceneData[ PrimitiveBaseOffset + 26 ].xyz;
	

	UNROLL
		for( int i = 0; i < NUM_CUSTOM_PRIMITIVE_DATA; i++ )
		{
			PrimitiveData.CustomPrimitiveData[ i ] = View.PrimitiveSceneData[ PrimitiveBaseOffset + 27 + i ];
		}

	return PrimitiveData;
}
FPrimitiveSceneData GetPrimitiveData( FMaterialPixelParameters Parameters )
{
	return GetPrimitiveData( Parameters.PrimitiveId );
}
FPrimitiveSceneData GetPrimitiveData( FMaterialVertexParameters Parameters )
{
	return GetPrimitiveData( Parameters.PrimitiveId );
}
#ifdef UE5
	FLWCVector3 ToUnrealPos( FLWCVector3 Pos )
	{
		float3 PosFloat = LWCToFloat( Pos );
		PosFloat = ToUnrealPos( PosFloat );
		return LWCPromote( PosFloat );
	}
	FLWCVector3 GetWorldPosition_NoMaterialOffsets( FMaterialPixelParameters Parameters )
	{
		return LWCPromote(Parameters.WorldPosition_NoOffsets);
	}
	FLWCVector3 GetWorldPosition( FMaterialPixelParameters Parameters )
	{
		return LWCPromote(Parameters.AbsoluteWorldPosition);
	}
	FLWCVector3 GetWorldPosition( FMaterialVertexParameters Parameters )
	{
		//return LWCSubtract( GetTranslatedWorldPosition( Parameters ), ResolvedView.PreViewTranslation );
		return LWCPromote( GetTranslatedWorldPosition( Parameters ) );
	}

	FLWCVector3 GetPrevWorldPosition( FMaterialVertexParameters Parameters )
	{
		//return LWCSubtract( GetPrevTranslatedWorldPosition( Parameters ), ResolvedView.PrevPreViewTranslation );
		return LWCPromote( GetTranslatedWorldPosition( Parameters ) );
	}
	FLWCVector3 TransformLocalPositionToWorld( FMaterialVertexParameters Parameters, float3 InLocalPosition )
	{
	#if USE_INSTANCING || USE_INSTANCE_CULLING || IS_MESHPARTICLE_FACTORY
		return LWCMultiply( InLocalPosition, Parameters.InstanceLocalToWorld );
	#else
		FLWCVector3 Ret = LWCMultiply( InLocalPosition, GetPrimitiveData( Parameters ).LocalToWorld );
		return Ret;
	#endif
	}
	FLWCVector3 TransformLocalPositionToWorld( FMaterialPixelParameters Parameters, float3 InLocalPosition )
	{
		return LWCMultiply( InLocalPosition, GetPrimitiveData( Parameters ).LocalToWorld );
	}
	FLWCVector3 TransformLocalPositionToPrevWorld( FMaterialVertexParameters Parameters, float3 InLocalPosition )
	{
		return LWCMultiply( InLocalPosition, Parameters.PrevFrameLocalToWorld );
	}
	FLWCVector3 GetObjectWorldPosition( FMaterialVertexParameters Parameters )
	{
	#if USE_INSTANCING || USE_INSTANCE_CULLING || IS_MESHPARTICLE_FACTORY
		return LWCGetOrigin( Parameters.InstanceLocalToWorld );
	#else
		return GetPrimitiveData( Parameters ).ObjectWorldPosition;
	#endif
	}

	FLWCMatrix GetInstanceToWorld( FMaterialVertexParameters Parameters )
	{
	#if USE_INSTANCING || USE_INSTANCE_CULLING || IS_MESHPARTICLE_FACTORY
		return Parameters.InstanceLocalToWorld;
	#else
		return GetPrimitiveData( Parameters ).LocalToWorld;
	#endif
	}
	FLWCMatrix GetInstanceToWorld( FMaterialPixelParameters Parameters )
	{
	#if HAS_INSTANCE_LOCAL_TO_WORLD_PS
		return Parameters.InstanceLocalToWorld;
	#else
		return GetPrimitiveData( Parameters ).LocalToWorld;
	#endif
	}
	MaterialFloat3 TransformLocalVectorToPrevWorld( FMaterialVertexParameters Parameters, MaterialFloat3 InLocalVector )
	{
		return LWCMultiplyVector( InLocalVector, Parameters.PrevFrameLocalToWorld );
	}
	FLWCVector3 GetObjectWorldPosition( FMaterialPixelParameters Parameters )
	{
		return GetPrimitiveData( Parameters ).ObjectWorldPosition;
	}
	FLWCVector3 GetActorWorldPosition( FMaterialPixelParameters Parameters )
	{
	#if DECAL_PRIMITIVE
		return MakeLWCVector3( DecalTilePosition, DecalToWorld[ 3 ].xyz );
	#else
		return GetPrimitiveData( Parameters ).ActorWorldPosition;
	#endif
	}
	FLWCVector3 GetActorWorldPosition( FMaterialVertexParameters Parameters )
	{
	#if DECAL_PRIMITIVE
		return MakeLWCVector3( DecalTilePosition, DecalToWorld[ 3 ].xyz );
	#else
		return GetPrimitiveData( Parameters ).ActorWorldPosition;
	#endif
	}
	float4 MaterialExpressionSkyAtmosphereAerialPerspective( FMaterialPixelParameters Parameters, FLWCVector3 WorldPosition )
	{
	#if MATERIAL_SKY_ATMOSPHERE && PROJECT_SUPPORT_SKY_ATMOSPHERE
		//...
	#else
		return float4( 0.0f, 0.0f, 0.0f, 1.0f ); // RGB= null scattering, A= null transmittance
	#endif
	}
	float3 MaterialExpressionSkyAtmosphereLightIlluminance( FMaterialPixelParameters Parameters, FLWCVector3 WorldPosition, uint LightIndex )
	{
	#if MATERIAL_SKY_ATMOSPHERE && PROJECT_SUPPORT_SKY_ATMOSPHERE
		//...	
	#endif
		return float3( 0.0f, 0.0f, 0.0f );
	}
	float GetDistanceToNearestSurfaceGlobal( FLWCVector3 WorldPosition )
	{
		//Distance to nearest DistanceField voxel I think ?
		return 1000.0f;
	}
	float CalculateDistanceFieldApproxAO( FLWCVector3 WorldPosition, float3 WorldNormal, uint NumSteps, float StepDistance, float StepScale, float DistanceBias, float MaxDistance = 0.0 )
	{
		//...
		return 1.0;
	}
	FLWCMatrix GetPrevInstanceToWorld( FMaterialVertexParameters Parameters )
	{
		return GetPrimitiveData( Parameters ).PreviousLocalToWorld;
	}
	FLWCVector3 GetWorldCameraOrigin( FMaterialVertexParameters Parameters )
	{
		//return Parameters.LWCData.WorldCameraOrigin;
		return ResolvedView.WorldCameraOrigin;
	}
	FLWCVector3 GetWorldCameraOrigin( FMaterialPixelParameters Parameters )
	{
		//return Parameters.LWCData.WorldCameraOrigin;
		return ResolvedView.WorldCameraOrigin;
	}
	float3 GetDistanceFieldGradientGlobal( FLWCVector3 WorldPosition )
	{
		float3 Gradient = float3( 0, 0, 0.001f );
		return Gradient;
	}
	MaterialFloat MaterialExpressionNoise( FLWCVector3 Position, float Scale, int Quality, int Function, bool bTurbulence, uint Levels, float OutputMin, float OutputMax, float LevelScale, float FilterWidth, bool bTiling, float RepeatSize )
	{
		return MaterialExpressionNoise( LWCToFloat(Position), Scale, Quality, Function, bTurbulence, Levels, OutputMin, OutputMax, LevelScale, FilterWidth, bTiling, RepeatSize );
	}
	FLWCInverseMatrix GetWorldToLocal( FMaterialVertexParameters Parameters )
	{
		return GetPrimitiveData( Parameters ).WorldToLocal;//Parameters.LWCData.WorldToLocal;
	}
	FLWCInverseMatrix GetWorldToLocal( FMaterialPixelParameters Parameters )
	{
		return GetPrimitiveData( Parameters ).WorldToLocal;//Parameters.LWCData.WorldToLocal;
	}
#else
	float3 GetWorldPosition_NoMaterialOffsets( FMaterialPixelParameters Parameters )
	{
		return Parameters.WorldPosition_NoOffsets;
	}
	float3 GetWorldPosition( FMaterialPixelParameters Parameters )
	{
		return Parameters.AbsoluteWorldPosition;
	}
	float3 GetWorldPosition( FMaterialVertexParameters Parameters )
	{
		return GetTranslatedWorldPosition( Parameters );// -ResolvedView.PreViewTranslation;
	}
	float3 GetPrevTranslatedWorldPosition( FMaterialVertexParameters Parameters )
	{
		return GetTranslatedWorldPosition( Parameters );
	}
	float3 GetPrevWorldPosition( FMaterialVertexParameters Parameters )
	{
		return GetPrevTranslatedWorldPosition( Parameters );// -ResolvedView.PrevPreViewTranslation;
	}
	float3 GetObjectWorldPosition( FMaterialPixelParameters Parameters )
	{
		return GetPrimitiveData( Parameters ).ObjectWorldPositionAndRadius.xyz;
	}
	float3 GetObjectWorldPosition( FMaterialVertexParameters Parameters )
	{
	#if USE_INSTANCING || IS_MESHPARTICLE_FACTORY
		return Parameters.InstanceLocalToWorld[ 3 ].xyz;
	#else
		return GetPrimitiveData( Parameters.PrimitiveId ).ObjectWorldPositionAndRadius.xyz;
	#endif
	}
	float3 TransformLocalPositionToWorld( FMaterialVertexParameters Parameters, float3 InLocalPosition )
	{
	//#if USE_INSTANCING || IS_MESHPARTICLE_FACTORY
	//	return mul( float4( InLocalPosition, 1 ), Parameters.InstanceLocalToWorld ).xyz;
	//#else
		float3 Ret = mul( float4( InLocalPosition, 1 ), GetPrimitiveData( Parameters.PrimitiveId ).LocalToWorld ).xyz;
		return Ret;
	//#endif
	}
	float3 TransformLocalPositionToPrevWorld( FMaterialVertexParameters Parameters, float3 InLocalPosition )
	{
		float3 Ret = mul( float4( InLocalPosition, 1 ), Parameters.PrevFrameLocalToWorld ).xyz;
		return Ret;
	}
	MaterialFloat3 TransformLocalVectorToPrevWorld( FMaterialVertexParameters Parameters, MaterialFloat3 InLocalVector )
	{
		return mul( InLocalVector, (MaterialFloat3x3)Parameters.PrevFrameLocalToWorld );
	}
	float3 GetActorWorldPosition( FMaterialVertexParameters Parameters )
	{
		return GetPrimitiveData( Parameters ).ActorWorldPosition;
	}
	float3 TransformLocalPositionToWorld( FMaterialPixelParameters Parameters, float3 InLocalPosition )
	{
		float3 Ret = mul( float4( InLocalPosition, 1 ), Primitive.LocalToWorld ).xyz;
		return Ret;
	}
	float4 MaterialExpressionSkyAtmosphereAerialPerspective( FMaterialPixelParameters Parameters, float3 WorldPosition )
	{
	#if MATERIAL_SKY_ATMOSPHERE && PROJECT_SUPPORT_SKY_ATMOSPHERE
		//...
	#else
		return float4( 0.0f, 0.0f, 0.0f, 1.0f ); // RGB= null scattering, A= null transmittance
	#endif
	}
	float3 MaterialExpressionSkyAtmosphereLightIlluminance( FMaterialPixelParameters Parameters, float3 WorldPosition, uint LightIndex )
	{
	#if MATERIAL_SKY_ATMOSPHERE && PROJECT_SUPPORT_SKY_ATMOSPHERE
		//...	
	#endif
		return float3( 0.0f, 0.0f, 0.0f );
	}
#endif
#ifdef UE5
	FLWCInverseMatrix GetWorldToInstance(FMaterialPixelParameters Parameters)
	{
		#if HAS_INSTANCE_WORLD_TO_LOCAL_PS
			return Parameters.InstanceWorldToLocal;
		#else
			return GetPrimitiveData(Parameters).WorldToLocal;
		#endif
	}
	FLWCInverseMatrix GetWorldToInstance( FMaterialVertexParameters Parameters )
	{
	#if USE_INSTANCING || USE_INSTANCE_CULLING || IS_MESHPARTICLE_FACTORY
		return Parameters.InstanceWorldToLocal;
	#else
		return GetPrimitiveData( Parameters ).WorldToLocal;
	#endif
	}
	float3 RotateAboutAxis( float4 NormalizedRotationAxisAndAngle, FLWCVector3 PositionOnAxis, FLWCVector3 Position )
	{
		// Project Position onto the rotation axis and find the closest point on the axis to Position
		FLWCVector3 ClosestPointOnAxis = LWCAdd( PositionOnAxis, NormalizedRotationAxisAndAngle.xyz * dot( NormalizedRotationAxisAndAngle.xyz, LWCToFloat( LWCSubtract( Position, PositionOnAxis ) ) ) );
		// Construct orthogonal axes in the plane of the rotation
		float3 UAxis = LWCToFloat( LWCSubtract( Position, ClosestPointOnAxis ) );
		float3 VAxis = cross( NormalizedRotationAxisAndAngle.xyz, UAxis );
		float CosAngle;
		float SinAngle;
		sincos( NormalizedRotationAxisAndAngle.w, SinAngle, CosAngle );
		// Rotate using the orthogonal axes
		float3 R = UAxis * CosAngle + VAxis * SinAngle;

		// Here we want to compute the following values:
		// FLWCVector3 RotatedPosition = LWCAdd(ClosestPointOnAxis, R);
		// return LWCSubtract(RotatedPosition, Position);
		// This can logically be written like this:
		// return ClosestPointOnAxis + R - Position
		// Notice that UAxis is already defined as (Position - ClosestPointOnAxis)
		// So we can simply this as R - UAxis, to avoid some conversions to/from LWC
		return R - UAxis;
	}
#endif
float3 GetObjectOrientation( FMaterialVertexParameters Parameters )
{
#if DECAL_PRIMITIVE
	return DecalOrientation.xyz;
#else
	return GetPrimitiveData( Parameters ).ObjectOrientation;
#endif
}
float3 GetObjectOrientation( FMaterialPixelParameters Parameters )
{
#if DECAL_PRIMITIVE
	return DecalOrientation.xyz;
#else
	return GetPrimitiveData( Parameters ).ObjectOrientation * -1;//tested only with decals
#endif
}
float2 SvPositionToViewportUV( float4 SvPosition )
{
	// can be optimized from 2SUB+2MUL to 2MAD
	float2 PixelPos = SvPosition.xy - View.ViewRectMin.xy;

	return PixelPos.xy * View.ViewSizeAndInvSize.zw;
}

#if POST_PROCESS_MATERIAL

float2 GetPixelPosition( FMaterialPixelParameters Parameters )
{
	return Parameters.SvPosition.xy - float2( PostProcessOutput_ViewportMin );
}

float2 GetViewportUV( FMaterialPixelParameters Parameters )
{
	return GetPixelPosition( Parameters ) * PostProcessOutput_ViewportSizeInverse;
}

#else

float2 GetPixelPosition( FMaterialPixelParameters Parameters )
{
	return Parameters.SvPosition.xy - float2( View.ViewRectMin.xy );
}

float2 GetViewportUV( FMaterialPixelParameters Parameters )
{
	return SvPositionToViewportUV( Parameters.SvPosition );
}

#endif
float2 CalcScreenUVFromOffsetFraction( float4 ScreenPosition, float2 OffsetFraction )
{
	float2 NDC = ScreenPosition.xy / ScreenPosition.w;
	// Apply the offset in NDC space so that it is consistent regardless of scene color buffer size
	// Clamp to valid area of the screen to avoid reading garbage
	//@todo - soft clamp
	float2 OffsetNDC = clamp( NDC + OffsetFraction * float2( 2, -2 ), -.999f, .999f );
	return float2( OffsetNDC * ResolvedView.ScreenPositionScaleBias.xy + ResolvedView.ScreenPositionScaleBias.wz );
}
float3 DecodeSceneColorForMaterialNode( float2 ScreenUV )
{
	#if HDRP || URP
		return SHADERGRAPH_SAMPLE_SCENE_COLOR( ScreenUV.xy );
	#else
		return float3( 0.0f, 0.0f, 0.0f );
	#endif
//#if !defined(SceneColorCopyTexture)
//	// Hit proxies rendering pass doesn't have access to valid render buffers
//	return float3( 0.0f, 0.0f, 0.0f );
//#else
//	float4 EncodedSceneColor = Texture2DSample( SceneColorCopyTexture, SceneColorCopySampler, ScreenUV );
//
//	// Undo the function in EncodeSceneColorForMaterialNode
//	float3 SampledColor = pow( EncodedSceneColor.rgb, 4 ) * 10;
//
//#if USE_PREEXPOSURE
//	SampledColor *= View.OneOverPreExposure.xxx;
//#endif
//
//	return SampledColor;
//#endif
}
float4 DecodeSceneColorAndAlpharForMaterialNode( float2 ScreenUV )
{
#if HDRP || URP
	return float4( SHADERGRAPH_SAMPLE_SCENE_COLOR( ScreenUV.xy ), 0.0f );
#else
	return float4( 0.0f, 0.0f, 0.0f, 0.0f );
#endif
}

float3 MaterialExpressionBlackBody( float Temp )
{
	float u = ( 0.860117757f + 1.54118254e-4f * Temp + 1.28641212e-7f * Temp * Temp ) / ( 1.0f + 8.42420235e-4f * Temp + 7.08145163e-7f * Temp * Temp );
	float v = ( 0.317398726f + 4.22806245e-5f * Temp + 4.20481691e-8f * Temp * Temp ) / ( 1.0f - 2.89741816e-5f * Temp + 1.61456053e-7f * Temp * Temp );

	float x = 3 * u / ( 2 * u - 8 * v + 4 );
	float y = 2 * v / ( 2 * u - 8 * v + 4 );
	float z = 1 - x - y;

	float Y = 1;
	float X = Y / y * x;
	float Z = Y / y * z;

	float3x3 XYZtoRGB =
	{
		 3.2404542, -1.5371385, -0.4985314,
		-0.9692660,  1.8760108,  0.0415560,
		 0.0556434, -0.2040259,  1.0572252
	};

	return mul( XYZtoRGB, float3( X, Y, Z ) ) * pow( 0.0004 * Temp, 4 );
}

#define DDX ddx
#define DDY ddy

float GetPerInstanceFadeAmount( FMaterialPixelParameters Parameters )
{
#if USE_INSTANCING
	return float( Parameters.PerInstanceParams.y );
#else
	return float( 1.0 );
#endif
}

MaterialFloat3x3 GetLocalToWorld3x3( uint PrimitiveId )
{
	//return (MaterialFloat3x3)GetPrimitiveData( PrimitiveId ).LocalToWorld;
	return (MaterialFloat3x3)Primitive.LocalToWorld;
}

MaterialFloat3x3 GetLocalToWorld3x3()
{
	return (MaterialFloat3x3)Primitive.LocalToWorld;
}

MaterialFloat3 TransformLocalVectorToWorld( FMaterialPixelParameters Parameters, MaterialFloat3 InLocalVector )
{
	return mul( InLocalVector, GetLocalToWorld3x3( Parameters.PrimitiveId ) );
}
MaterialFloat3 TransformLocalVectorToWorld( FMaterialVertexParameters Parameters, MaterialFloat3 InLocalVector )
{
#if USE_INSTANCING || USE_INSTANCE_CULLING || IS_MESHPARTICLE_FACTORY
	return LWCMultiplyVector( InLocalVector, Parameters.InstanceLocalToWorld );
#else
	return mul( InLocalVector, GetLocalToWorld3x3( Parameters.PrimitiveId ) );
#endif
}

bool GetShadowReplaceState()
{
#ifdef SHADOW_DEPTH_SHADER
	return true;
#else
	return false;
#endif
}

float IsShadowDepthShader()
{
	return GetShadowReplaceState() ? 1.0f : 0.0f;
}

float3 GetTranslatedWorldPosition( FMaterialPixelParameters Parameters )
{
	return Parameters.WorldPosition_CamRelative;
}
float GetDistanceToNearestSurfaceGlobal( float3 Position )
{
	//Distance to nearest DistanceField voxel I think ?
	return 1000.0f;
}
float2 RotateScaleOffsetTexCoords( float2 InTexCoords, float4 InRotationScale, float2 InOffset )
{
	return float2( dot( InTexCoords, InRotationScale.xy ), dot( InTexCoords, InRotationScale.zw ) ) + InOffset;
}
float2 GetTanHalfFieldOfView()
{
	//@return tan(View.FieldOfViewWideAngles * .5)
	//return float2( View.ClipToView[ 0 ][ 0 ], View.ClipToView[ 1 ][ 1 ] );
	float EmulatedFOV = 3.14f / 2.0f;
	return float2( EmulatedFOV, EmulatedFOV );
}
float Pow2( float x )
{
	return x * x;
}
float Pow5( float x )
{
	float xx = x * x;
	return xx * xx * x;
}
float3 HairAbsorptionToColor( float3 A, float B = 0.3f )
{
	const float b2 = B * B;
	const float b3 = B * b2;
	const float b4 = b2 * b2;
	const float b5 = B * b4;
	const float D = ( 5.969f - 0.215f * B + 2.532f * b2 - 10.73f * b3 + 5.574f * b4 + 0.245f * b5 );
	return exp( -sqrt( A ) * D );
}
float3 HairColorToAbsorption( float3 C, float B = 0.3f )
{
	const float b2 = B * B;
	const float b3 = B * b2;
	const float b4 = b2 * b2;
	const float b5 = B * b4;
	const float D = ( 5.969f - 0.215f * B + 2.532f * b2 - 10.73f * b3 + 5.574f * b4 + 0.245f * b5 );
	return Pow2( log( C ) / D );
}
float3 GetHairColorFromMelanin( float InMelanin, float InRedness, float3 InDyeColor )
{
	InMelanin = saturate( InMelanin );
	InRedness = saturate( InRedness );
	const float Melanin = -log( max( 1 - InMelanin, 0.0001f ) );
	const float Eumelanin = Melanin * ( 1 - InRedness );
	const float Pheomelanin = Melanin * InRedness;

	const float3 DyeAbsorption = HairColorToAbsorption( saturate( InDyeColor ) );
	const float3 Absorption = Eumelanin * float3( 0.506f, 0.841f, 1.653f ) + Pheomelanin * float3( 0.343f, 0.733f, 1.924f );

	return HairAbsorptionToColor( Absorption + DyeAbsorption );
}

float3 MaterialExpressionGetHairColorFromMelanin( float Melanin, float Redness, float3 DyeColor )
{
	return GetHairColorFromMelanin( Melanin, Redness, DyeColor );
}
bool GetRayTracingQualitySwitch()
{
#if RAYHITGROUPSHADER
	return true;
#else
	return false;
#endif
}
bool GetPathTracingQualitySwitch()
{
#if RAYHITGROUPSHADER && PATH_TRACING
	return true;
#else
	return false;
#endif
}
bool GetRuntimeVirtualTextureOutputSwitch()
{
#if VIRTUAL_TEXTURE_PAGE_RENDER
	return true;
#else
	return false;
#endif
}
MaterialFloat2 GetDefaultSceneTextureUV( FMaterialPixelParameters Parameters, uint SceneTextureId )
{
	return float2( 0, 0 );
}
float4 SceneTextureLookup( float2 UV, int SceneTextureIndex, bool bFiltered )
{
	return float4( 0, 0, 0, 0 );
}
float3 MaterialExpressionAtmosphericLightVector( FMaterialPixelParameters Parameters )
{
#if MATERIAL_ATMOSPHERIC_FOG
	return ResolvedView.AtmosphereLightDirection[ 0 ].xyz;
#else
	return float3( 0.f, 0.f, 0.f );
#endif
}
float3 MaterialExpressionAtmosphericLightColor( FMaterialPixelParameters Parameters )
{
	//return ResolvedView.AtmosphereLightIlluminanceOnGroundPostTransmittance[ 0 ].rgb;
	return float3( 0, 0, 0 );
}
float3 MaterialExpressionSkyAtmosphereViewLuminance( FMaterialPixelParameters Parameters )
{
#if MATERIAL_SKY_ATMOSPHERE && PROJECT_SUPPORT_SKY_ATMOSPHERE
	//...
#else
	return float3( 0.0f, 0.0f, 0.0f );
#endif
}
float3 MaterialExpressionSkyAtmosphereLightDiskLuminance( FMaterialPixelParameters Parameters, uint LightIndex, float OverrideAtmosphereLightDiscCosHalfApexAngle = -1)
{
	float3 LightDiskLuminance = float3( 0.0f, 0.0f, 0.0f );
#if MATERIAL_SKY_ATMOSPHERE && PROJECT_SUPPORT_SKY_ATMOSPHERE
	//...
#endif
	return LightDiskLuminance;
}
float3 MaterialExpressionSkyAtmosphereLightDirection( FMaterialPixelParameters Parameters, uint LightIndex )
{
	return float3( 0.0f, 0.0f, 0.0f );
}
float3 MaterialExpressionSkyAtmosphereLightDirection( FMaterialVertexParameters Parameters, uint LightIndex )
{
	return float3( 0.0f, 0.0f, 0.0f );
}
float3 MaterialExpressionSkyAtmosphereDistantLightScatteredLuminance( FMaterialPixelParameters Parameters )
{
#if MATERIAL_SKY_ATMOSPHERE && PROJECT_SUPPORT_SKY_ATMOSPHERE
	//...	
#endif
	return float3( 0.0f, 0.0f, 0.0f );
}
float3 MaterialExpressionSkyLightEnvMapSample( float3 Direction, float Roughness )
{
	return 0.0f;
}
MaterialFloat2 ViewportUVToSceneTextureUV( MaterialFloat2 ViewportUV, uint SceneTextureId )
{
	//TODO
	return ViewportUV;
}
MaterialFloat2 ClampSceneTextureUV( MaterialFloat2 BufferUV, uint SceneTextureId )
{
	float4 MinMax = float4( 0, 0, 1, 1 );// GetSceneTextureUVMinMax( SceneTextureId );

	return clamp( BufferUV, MinMax.xy, MinMax.zw );
}

#if TEX_COORD_SCALE_ANALYSIS
	#define MaterialStoreTexCoordScale(Parameters, UV, TextureReferenceIndex) StoreTexCoordScale(Parameters.TexCoordScalesParams, UV, TextureReferenceIndex)
	#define MaterialStoreTexSample(Parameters, UV, TextureReferenceIndex) StoreTexSample(Parameters.TexCoordScalesParams, UV, TextureReferenceIndex)
	#define MaterialStoreVTSampleInfo(Parameters, PageTableResult, LayerIndex, TextureReferenceIndex) StoreVTSampleInfo(Parameters, PageTableResult, LayerIndex, TextureReferenceIndex)
#else
	#define MaterialStoreTexCoordScale(Parameters, UV, TextureReferenceIndex) 1.0f
	#define MaterialStoreTexSample(Parameters, UV, TextureReferenceIndex) 1.0f
	#define MaterialStoreVTSampleInfo(Parameters, PageTableResult, LayerIndex, TextureReferenceIndex) 1.0f
#endif

MaterialFloat4 MaterialExpressionVectorNoise( MaterialFloat3 Position, int Quality, int Function, bool bTiling, float TileSize )
{
	float4 result = float4( 0, 0, 0, 1 );
	float3x4 Jacobian = JacobianSimplex_ALU( Position, bTiling, TileSize );	// compiled out if not used

	// verified, HLSL compiled out the switch if Function is a constant
	switch( Function )
	{
		//case 0:	// Cellnoise
		//	result.xyz = float3( Rand3DPCG16( int3( floor( NoiseTileWrap( Position, bTiling, TileSize ) ) ) ) ) / 0xffff;
		//	break;
		//case 1: // Color noise
		//	result.xyz = float3( Jacobian[ 0 ].w, Jacobian[ 1 ].w, Jacobian[ 2 ].w );
		//	break;
		case 2: // Gradient
			result = Jacobian[ 0 ];
			break;
		//case 3: // Curl
		//	result.xyz = float3( Jacobian[ 2 ][ 1 ] - Jacobian[ 1 ][ 2 ], Jacobian[ 0 ][ 2 ] - Jacobian[ 2 ][ 0 ], Jacobian[ 1 ][ 0 ] - Jacobian[ 0 ][ 1 ] );
		//	break;
		default: // Voronoi
			result = VoronoiNoise3D_ALU( Position, Quality, bTiling, TileSize, false );
			break;
	}
	return result;
}
MaterialFloat4 ProcessMaterialAlphaTextureLookup( MaterialFloat4 TextureValue )
{
	// Sampling a single channel texture in D3D9 gives: (G,G,G)
	// Sampling a single channel texture in D3D11 gives: (G,0,0)
	// This replication reproduces the D3D9 behavior in all cases.
	return TextureValue.rrrr;
}
uint BitFieldExtractU32( uint Data, uint Size, uint Offset )
{
	Size &= 31u;
	Offset &= 31u;

	if( Size == 0u )
		return 0u;
	else if( Offset + Size < 32u )
		return ( Data << ( 32u - Size - Offset ) ) >> ( 32u - Size );
	else
		return Data >> Offset;
}
float2 Hammersley( uint Index, uint NumSamples, uint2 Random )
{
	float E1 = frac( (float)Index / NumSamples + float( Random.x & 0xffff ) / ( 1 << 16 ) );
	float E2 = float( reversebits( Index ) ^ Random.y ) * 2.3283064365386963e-10;
	return float2( E1, E2 );
}
float GetPerInstanceCustomData(FMaterialPixelParameters Parameters, int Index, float DefaultValue)
{
#if IS_NANITE_PASS && USES_PER_INSTANCE_CUSTOM_DATA
	//...
#endif

	return DefaultValue;
}
MaterialFloat3 GetPerInstanceCustomData3Vector( FMaterialPixelParameters Parameters, int Index, MaterialFloat3 DefaultValue )
{
#if IS_NANITE_PASS && USES_PER_INSTANCE_CUSTOM_DATA
	//...
#endif

	return DefaultValue;
}
MaterialFloat3 GetPerInstanceCustomData3Vector( FMaterialVertexParameters Parameters, int Index, MaterialFloat3 DefaultValue )
{
#if USE_INSTANCE_CULLING && USES_PER_INSTANCE_CUSTOM_DATA
	//...
#endif

	return DefaultValue;
}
float4 MaterialExpressionDBufferTextureLookup( float2 BufferUV, int DBufferTextureIndex )
{
	//...

	return float4( 0, 0, 0, 1 );
}
float4 MaterialExpressionDBufferTextureLookup( FMaterialPixelParameters Parameters, float2 BufferUV, int DBufferTextureIndex )
{
	//...

	return float4( 0, 0, 0, 1 );
}

float2 ViewportUVToBufferUV( float2 ViewportUV )
{
	float2 PixelPos = ViewportUV * View.ViewSizeAndInvSize.xy;
	return ( PixelPos + View.ViewRectMin.xy ) * View_BufferSizeAndInvSize.zw;
}

//Virtual Textures
#define VTADDRESSMODE_CLAMP  0
#define VTADDRESSMODE_WRAP	 1
#define VTADDRESSMODE_MIRROR 2
#define LIGHTMAP_VT_ENABLED 0
#define VTUniform float

struct VTPageTableResult
{
	float2 Texcoord;
};
float VTPageTableUniform_Unpack( float x, float y )
{
	return 0;
}
VTUniform VTUniform_Unpack( float PackedUniform )
{
	return 0;
}
VTPageTableResult TextureLoadVirtualPageTable(
	/*Texture2D<uint4>*/ float PageTable0,
	/*VTPageTableUniform*/ float PageTableUniform,
	float2 UV, uint AddressU, uint AddressV,
	MaterialFloat MipBias, float2 SvPositionXY,
	uint SampleIndex,
	/*in out FVirtualTextureFeedbackParams*/ float Feedback )
{
	VTPageTableResult Result;
	Result.Texcoord = UV;

	return Result;
}
VTPageTableResult TextureLoadVirtualPageTableLevel(
	float PageTable0,
	float PageTableUniform,
	float2 UV, uint AddressU, uint AddressV,
	MaterialFloat Level )
{
	VTPageTableResult Result;
	Result.Texcoord = UV;

	return Result;
}
VTPageTableResult TextureLoadVirtualPageTableLevel(
	float PageTable0,
	float PageTableUniform,
	float2 UV, uint AddressU, uint AddressV,
	MaterialFloat Level,
	uint SampleIndex,
	float Feedback )
{
	VTPageTableResult Result;
	Result.Texcoord = UV;

	return Result;
}
VTPageTableResult TextureLoadVirtualPageTableGrad(
	float PageTable0,
	float PageTableUniform,
	float2 UV, uint AddressU, uint AddressV,
	MaterialFloat2 dUVdx, MaterialFloat2 dUVdy, float2 SvPositionXY,
	uint SampleIndex,
	float Feedback )
{
	VTPageTableResult Result;
	Result.Texcoord = UV;

	return Result;
}
MaterialFloat4 ProcessMaterialVirtualColorTextureLookup( MaterialFloat4 TextureValue )
{
	return TextureValue;
}
MaterialFloat4 TextureVirtualSample(
	Texture2D PhysicalTexture, SamplerState PhysicalSampler,
	VTPageTableResult PageTableResult, uint LayerIndex,
	VTUniform Uniform )
{
	return Texture2DSample( PhysicalTexture, PhysicalSampler, PageTableResult.Texcoord );
}
MaterialFloat4 TextureVirtualSampleLevel(
	Texture2D Physical, SamplerState PhysicalSampler,
	VTPageTableResult PageTableResult, uint LayerIndex,
	VTUniform Uniform )
{
	return Texture2DSampleLevel( Physical, PhysicalSampler, PageTableResult.Texcoord, 0.0f );
}


float4 GetDynamicParameter( FMaterialParticleParameters Parameters, float4 Default, int ParameterIndex = 0 )
{
#if (NIAGARA_PARTICLE_FACTORY)
	switch( ParameterIndex )
	{
	#if (DYNAMIC_PARAMETERS_MASK & 1)
		case 0:	return ( Parameters.DynamicParameterValidMask & 1 ) != 0 ? Parameters.DynamicParameter : Default;
		#endif
		#if (DYNAMIC_PARAMETERS_MASK & 2)
		case 1:	return ( Parameters.DynamicParameterValidMask & 2 ) != 0 ? Parameters.DynamicParameter1 : Default;
		#endif
		#if (DYNAMIC_PARAMETERS_MASK & 4)
		case 2:	return ( Parameters.DynamicParameterValidMask & 4 ) != 0 ? Parameters.DynamicParameter2 : Default;
		#endif	
		#if (DYNAMIC_PARAMETERS_MASK & 8)
		case 3:	return ( Parameters.DynamicParameterValidMask & 8 ) != 0 ? Parameters.DynamicParameter3 : Default;
		#endif
		default: return Default;
	}
#elif (PARTICLE_FACTORY)
	if( ParameterIndex == 0 )
	{
		return Parameters.DynamicParameter;
	}
#endif
	return Default;

}
float3 GetSpeedTreeVertexOffset( FMaterialVertexParameters Parameters, int GeometryType, int WindType, int LODType, float BillboardThreshold, bool bUsePreviousFrame, bool bExtraBend, float3 ExtraBend )
{
#if VF_SUPPORTS_SPEEDTREE_WIND
	if( bUsePreviousFrame )
	{
		return GetSpeedTreeVertexOffsetInner( Parameters, GeometryType, WindType, LODType, BillboardThreshold, bExtraBend, ExtraBend, GetPreviousSpeedTreeData() );
	}
	return GetSpeedTreeVertexOffsetInner( Parameters, GeometryType, WindType, LODType, BillboardThreshold, bExtraBend, ExtraBend, GetCurrentSpeedTreeData() );
#else
	return 0;
#endif
}
float3 GetDistanceFieldGradientGlobal( float3 WorldPosition )
{
	float3 Gradient = float3( 0, 0, 0.001f );
	//...
	return Gradient;
}
float acosFast( float inX )
{
	float x = abs( inX );
	float res = -0.156583f * x + ( 0.5 * PI );
	res *= sqrt( 1.0f - x );
	return ( inX >= 0 ) ? res : PI - res;
}

float2 acosFast( float2 x )
{
	return float2( acosFast( x.x ), acosFast( x.y ) );
}

float3 acosFast( float3 x )
{
	return float3( acosFast( x.x ), acosFast( x.y ), acosFast( x.z ) );
}

float4 acosFast( float4 x )
{
	return float4( acosFast( x.x ), acosFast( x.y ), acosFast( x.z ), acosFast( x.w ) );
}
float asinFast( float x )
{
	return ( 0.5 * PI ) - acosFast( x );
}

float2 asinFast( float2 x )
{
	return float2( asinFast( x.x ), asinFast( x.y ) );
}

float3 asinFast( float3 x )
{
	return float3( asinFast( x.x ), asinFast( x.y ), asinFast( x.z ) );
}

float4 asinFast( float4 x )
{
	return float4( asinFast( x.x ), asinFast( x.y ), asinFast( x.z ), asinFast( x.w ) );
}
float atan2Fast( float y, float x )
{
	float t0 = max( abs( x ), abs( y ) );
	float t1 = min( abs( x ), abs( y ) );
	float t3 = t1 / t0;
	float t4 = t3 * t3;

	// Same polynomial as atanFastPos
	t0 = +0.0872929;
	t0 = t0 * t4 - 0.301895;
	t0 = t0 * t4 + 1.0;
	t3 = t0 * t3;

	t3 = abs( y ) > abs( x ) ? ( 0.5 * PI ) - t3 : t3;
	t3 = x < 0 ? PI - t3 : t3;
	t3 = y < 0 ? -t3 : t3;

	return t3;
}

float2 atan2Fast( float2 y, float2 x )
{
	return float2( atan2Fast( y.x, x.x ), atan2Fast( y.y, x.y ) );
}

float3 atan2Fast( float3 y, float3 x )
{
	return float3( atan2Fast( y.x, x.x ), atan2Fast( y.y, x.y ), atan2Fast( y.z, x.z ) );
}

float4 atan2Fast( float4 y, float4 x )
{
	return float4( atan2Fast( y.x, x.x ), atan2Fast( y.y, x.y ), atan2Fast( y.z, x.z ), atan2Fast( y.w, x.w ) );
}
float atanFastPos( float x )
{
	float t0 = ( x < 1.0f ) ? x : 1.0f / x;
	float t1 = t0 * t0;
	float poly = 0.0872929f;
	poly = -0.301895f + poly * t1;
	poly = 1.0f + poly * t1;
	poly = poly * t0;
	return ( x < 1.0f ) ? poly : ( 0.5 * PI ) - poly;
}

// 4 VGPR, 16 FR (12 FR, 1 QR), 2 scalar
// input [-infinity, infinity] and output [-PI/2, PI/2]
float atanFast( float x )
{
	float t0 = atanFastPos( abs( x ) );
	return ( x < 0 ) ? -t0 : t0;
}

float2 atanFast( float2 x )
{
	return float2( atanFast( x.x ), atanFast( x.y ) );
}

float3 atanFast( float3 x )
{
	return float3( atanFast( x.x ), atanFast( x.y ), atanFast( x.z ) );
}

float4 atanFast( float4 x )
{
	return float4( atanFast( x.x ), atanFast( x.y ), atanFast( x.z ), atanFast( x.w ) );
}

float MaterialExpressionCloudSampleAltitudeInLayer( FMaterialPixelParameters Parameters )
{
#if CLOUD_LAYER_PIXEL_SHADER
	return Parameters.CloudSampleAltitudeInLayer;
#else
	return 0.0f;
#endif
}
float MaterialExpressionCloudSampleNormAltitudeInLayer( FMaterialPixelParameters Parameters )
{
#if CLOUD_LAYER_PIXEL_SHADER
	return Parameters.CloudSampleNormAltitudeInLayer;
#else
	return 0.0f;
#endif
}
MaterialFloat GetDistanceCullFade()
{
//#if PIXELSHADER
//	return saturate( ResolvedView.RealTime * PrimitiveFade.FadeTimeScaleBias.x + PrimitiveFade.FadeTimeScaleBias.y );
//#else
	return 1.0f;
//#endif
}
float EyeAdaptationLookup()
{
	return 0.5f;
}
float3 EyeAdaptationInverseLookup( float3 LightValue, float Alpha )
{
	float Adaptation = EyeAdaptationLookup();
	float LerpLogScale = -Alpha * log( Adaptation );
	float Scale = exp( LerpLogScale );
	return LightValue * Scale;
}
float EyeAdaptationInverseLookup( float Alpha )
{
	float Adaptation = EyeAdaptationLookup();
	float LerpLogScale = -Alpha * log(Adaptation);
	float Scale = exp(LerpLogScale);
	return Scale;
}
MaterialFloat2 GetLightmapUVs( FMaterialPixelParameters Parameters )
{
	return MaterialFloat2( 0, 0 );
}
MaterialFloat2 GetParticleMacroUV( FMaterialPixelParameters Parameters )
{
	//return ( Parameters.ScreenPosition.xy / Parameters.ScreenPosition.w - Parameters.Particle.MacroUV.xy ) * Parameters.Particle.MacroUV.zw + MaterialFloat2( .5, .5 );
	return float2( 0, 0 );
}
float3 MatPhysicsField_SamplePhysicsVectorField( float3 WorldPosition, int VectorTarget )
{
	return float3( 0, 0, 0 );
}

float MatPhysicsField_SamplePhysicsScalarField( float3 WorldPosition, int ScalarTarget )
{
	return float3( 0, 0, 0 );
}

int MatPhysicsField_SamplePhysicsIntegerField( float3 WorldPosition, int IntegerTarget )
{
	return float3( 0, 0, 0 );
}
uint2 SobolPixel( uint2 Pixel )
{
	//Needs View.SobolSamplingTexture
	return uint2( 0, 0 );
}
uint2 SobolIndex( uint2 Base, int Index, int Bits = 10 )
{
	//BuiltIn Errors out on these
	//uint2 SobolNumbers[ 10 ] = {
	//	uint2( 0x8680u, 0x4c80u ), uint2( 0xf240u, 0x9240u ), uint2( 0x8220u, 0x0e20u ), uint2( 0x4110u, 0x1610u ), uint2( 0xa608u, 0x7608u ),
	//	uint2( 0x8a02u, 0x280au ), uint2( 0xe204u, 0x9e04u ), uint2( 0xa400u, 0x4682u ), uint2( 0xe300u, 0xa74du ), uint2( 0xb700u, 0x9817u ),
	//};
	//
	//uint2 Result = Base;
	//UNROLL for( int b = 0; b < 10 && b < Bits; ++b )
	//{
	//	Result ^= ( Index & ( 1u << b ) ) ? SobolNumbers[ b ] : 0;
	//}
	//return Result;
	return uint2( 0, 0 );
}
float GetSphericalParticleOpacity( FMaterialPixelParameters Parameters, float Density )
{
	float Opacity = 0;
	//...
	return Opacity;
}
float4 MaterialExpressionVolumeSampleConservativeDensity( FMaterialPixelParameters Parameters )
{
	return float4( 0.0f, 0.0f, 0.0f, 0.0f );
}
float2 ComputeDecalDDX( FMaterialPixelParameters Parameters )
{
	return 0.0f;
}
float2 ComputeDecalDDY( FMaterialPixelParameters Parameters )
{
	return 0.0f;
}
float DecalLifetimeOpacity()
{
	return 0.0f;
}
float ComputeDecalMipmapLevel( FMaterialPixelParameters Parameters, float2 TextureSize )
{
	return 0.0f;
}



FStrataData PromoteParameterBlendedBSDFToOperator( FStrataData StrataData, inout FStrataTree StrataTree, int OperatorIndex, int BSDFIndex, int LayerDepth, int bIsBottom )
{
	return StrataData;
}

FStrataData StrataConvertLegacyMaterialStatic(
	FStrataPixelFootprint PixelFootprint,
	float3 BaseColor, float Specular, float Metallic,
	float Roughness, float Anisotropy,
	float3 SubSurfaceColor, float SubSurfaceProfileId,
	float ClearCoat, float ClearCoatRoughness,
	float3 Emissive,
	float Opacity,
	float3 TransmittanceColor,
	float3 WaterScatteringCoefficients, float3 WaterAbsorptionCoefficients, float WaterPhaseG, float3 ColorScaleBehindWater,
	uint ShadingModel,
	float3 RawNormal,
	float3 RawTangent,
	float3 RawClearCoatNormal,
	float3 RawCustomTangent,
	uint SharedLocalBasisIndex,
	uint ClearCoatBottomNormal_SharedLocalBasisIndex,
	inout uint SharedLocalBasisTypes,
	inout FStrataTree StrataTree )
{
	FStrataData Out;
	Out.EmissiveColor = Emissive;
	Out.Opacity = Opacity;
	Out.BaseColor = BaseColor;
	Out.Metallic = Metallic;
	Out.Roughness = Roughness;
	Out.Normal = RawNormal;	//For some reason RawNormal is in world space

	return Out;
}
FStrataData StrataConvertLegacyMaterialStatic(
	FStrataPixelFootprint PixelFootprint,
	float3 BaseColor, float Specular, float Metallic,
	float Roughness, float Anisotropy,
	float3 SubSurfaceColor, float SubSurfaceProfileId,
	float ClearCoat, float ClearCoatRoughness,
	float3 Emissive,
	float Opacity,
	float3 TransmittanceColor,
	float3 WaterScatteringCoefficients, float3 WaterAbsorptionCoefficients, float WaterPhaseG, float3 ColorScaleBehindWater,
	uint ShadingModel,
	float3 RawNormal,
	float3 RawTangent,
	float3 RawClearCoatNormal,
	float3 RawCustomTangent,
	uint SharedLocalBasisIndex,
	uint ClearCoatBottomNormal_SharedLocalBasisIndex,
	inout uint SharedLocalBasisTypes )
{
	FStrataData Out;
	Out.EmissiveColor = Emissive;
	Out.Opacity = Opacity;
	Out.BaseColor = BaseColor;
	Out.Metallic = Metallic;
	Out.Roughness = Roughness;
	Out.Normal = RawNormal;	//For some reason RawNormal is in world space

	return Out;
}
FStrataData StrataConvertLegacyMaterialDynamic(
	FStrataPixelFootprint PixelFootprint,
	float3 BaseColor, float Specular, float Metallic,
	float Roughness, float Anisotropy,
	float3 SubSurfaceColor, float SubSurfaceProfileId,
	float ClearCoat, float ClearCoatRoughness,
	float3 Emissive,
	float Opacity,
	float3 TransmittanceColor,
	float3 WaterScatteringCoefficients, float3 WaterAbsorptionCoefficients, float WaterPhaseG, float3 ColorScaleBehindWater,
	uint ShadingModel,
	float3 RawNormal,
	float3 RawTangent,
	float3 RawClearCoatNormal,
	float3 RawCustomTangent,
	uint SharedLocalBasisIndex,
	uint ClearCoatBottomNormal_SharedLocalBasisIndex,
	inout uint SharedLocalBasisTypes,
	inout FStrataTree StrataTree )
{
	FStrataData Out;
	Out.EmissiveColor = Emissive;
	Out.Opacity = Opacity;
	Out.BaseColor = BaseColor;
	Out.Metallic = Metallic;
	Out.Roughness = Roughness;
	Out.Normal = RawNormal;

	return Out;
}
FStrataData GetStrataSlabBSDF(
	FStrataPixelFootprint InPixelFootprint,
	float3 DiffuseAlbedo, float3 F0, float3 F90,
	float Roughness, float Anisotropy,
	float SSSProfileID, float3 SSSMFP, float SSSMFPScale, float SSSPhaseAnisotropy, float UseSSSDiffusion,
	float3 Emissive,
	float SecondRoughness, float SecondRoughnessWeight, float SecondRoughnessAsSimpleClearCoat,
	float FuzzAmount, float3 FuzzColor, float FuzzRoughness,
	float ThicknessCm,
	bool bIsThinSurface,
	uint SharedLocalBasisIndex, inout uint SharedLocalBasisTypes )
{
	FStrataData Out;
	Out.EmissiveColor = Emissive;
	Out.Opacity = 1;
	Out.BaseColor = DiffuseAlbedo;
	Out.Metallic = 0;
	Out.Roughness = Roughness;
	Out.Normal = float3( 0, 0, 1 );

	return Out;
}
FStrataData GetStrataSingleLayerWaterBSDF(
	float3 BaseColor, float Metallic, float Specular, float Roughness, float3 Emissive, float TopMaterialOpacity,
	float3 WaterAlbedo, float3 WaterExtinction, float WaterPhaseG, float3 ColorScaleBehindWater, uint SharedLocalBasisIndex )
{
	FStrataData Out;
	Out.EmissiveColor = Emissive;
	Out.Opacity = TopMaterialOpacity;
	Out.BaseColor = BaseColor;
	Out.Metallic = Metallic;
	Out.Roughness = Roughness;
	Out.Normal = float3( 0, 0, 1 );

	return Out;
}
FStrataData GetStrataUnlitBSDF( float3 Emissive, float3 TransmittanceColor )
{
	FStrataData Out;
	Out.EmissiveColor = Emissive;	
	Out.Opacity = 1;
	Out.BaseColor = float3(1,1,1);
	Out.Metallic = 0;
	Out.Roughness = 1;
	Out.Normal = float3(0,0,1);
	return Out;
}

void StrataToMaterialInputs( inout FPixelMaterialInputs Inputs, FStrataData Data )
{
	Inputs.EmissiveColor = Data.EmissiveColor;
	Inputs.Opacity = Data.Opacity;
	Inputs.BaseColor = Data.BaseColor;
	Inputs.Metallic = Data.Metallic;
	Inputs.Roughness = Data.Roughness;
	Inputs.Normal = Data.Normal;
}
float2 GetCotanHalfFieldOfView()
{
	//return float2( View.ViewToClip[ 0 ][ 0 ], View.ViewToClip[ 1 ][ 1 ] );
	return 1 / tan( View.FieldOfViewWideAngles * .5 );
}
float4 CosineSampleHemisphere( float2 E )
{
	float Phi = 2 * PI * E.x;
	float CosTheta = sqrt( E.y );
	float SinTheta = sqrt( 1 - CosTheta * CosTheta );

	float3 H;
	H.x = SinTheta * cos( Phi );
	H.y = SinTheta * sin( Phi );
	H.z = CosTheta;

	float PDF = CosTheta * ( 1.0 / PI );

	return float4( H, PDF );
}
//5.2 requirements
bool ShouldEnableWorldPositionOffset( FMaterialVertexParameters Parameters )
{
	//Always enable WPO for our shaders
	return true;
}
float3 ClampWorldPositionOffset( FMaterialVertexParameters Parameters, float3 InOffset )
{
	return float3( 0, 0, 0 );
}

// https://devblogs.microsoft.com/directx/announcing-hlsl-2021/
// HLSL 2021 supports Logical Operator Short Circuiting. To do vector bool operations, need to use and() or() select()
// Sadly the HLSL2021 standard does not overload select() very well...
#define select(cond,a,b) select_internal(cond,a,b)
#define DEFINE_SELECT(TYPE) \
	TYPE    select_internal(bool    c, TYPE    a, TYPE    b) { return TYPE   (c   ? a.x : b.x); } \
	\
	TYPE##2 select_internal(bool    c, TYPE    a, TYPE##2 b) { return TYPE##2(c   ? a   : b.x, c   ? a   : b.y); } \
	TYPE##2 select_internal(bool    c, TYPE##2 a, TYPE    b) { return TYPE##2(c   ? a.x : b  , c   ? a.y : b  ); } \
	TYPE##2 select_internal(bool    c, TYPE##2 a, TYPE##2 b) { return TYPE##2(c   ? a.x : b.x, c   ? a.y : b.y); } \
	TYPE##2 select_internal(bool2   c, TYPE    a, TYPE    b) { return TYPE##2(c.x ? a   : b  , c.y ? a   : b  ); } \
	TYPE##2 select_internal(bool2   c, TYPE    a, TYPE##2 b) { return TYPE##2(c.x ? a   : b.x, c.y ? a   : b.y); } \
	TYPE##2 select_internal(bool2   c, TYPE##2 a, TYPE    b) { return TYPE##2(c.x ? a.x : b  , c.y ? a.y : b  ); } \
	TYPE##2 select_internal(bool2   c, TYPE##2 a, TYPE##2 b) { return TYPE##2(c.x ? a.x : b.x, c.y ? a.y : b.y); } \
	\
	TYPE##3 select_internal(bool    c, TYPE    a, TYPE##3 b) { return TYPE##3(c   ? a   : b.x, c   ? a   : b.y, c   ? a   : b.z); } \
	TYPE##3 select_internal(bool    c, TYPE##3 a, TYPE    b) { return TYPE##3(c   ? a.x : b  , c   ? a.y : b  , c   ? a.z : b  ); } \
	TYPE##3 select_internal(bool    c, TYPE##3 a, TYPE##3 b) { return TYPE##3(c   ? a.x : b.x, c   ? a.y : b.y, c   ? a.z : b.z); } \
	TYPE##3 select_internal(bool3   c, TYPE    a, TYPE    b) { return TYPE##3(c.x ? a   : b  , c.y ? a   : b  , c.z ? a   : b  ); } \
	TYPE##3 select_internal(bool3   c, TYPE    a, TYPE##3 b) { return TYPE##3(c.x ? a   : b.x, c.y ? a   : b.y, c.z ? a   : b.z); } \
	TYPE##3 select_internal(bool3   c, TYPE##3 a, TYPE    b) { return TYPE##3(c.x ? a.x : b  , c.y ? a.y : b  , c.z ? a.z : b  ); } \
	TYPE##3 select_internal(bool3   c, TYPE##3 a, TYPE##3 b) { return TYPE##3(c.x ? a.x : b.x, c.y ? a.y : b.y, c.z ? a.z : b.z); } \
	\
	TYPE##4 select_internal(bool    c, TYPE    a, TYPE##4 b) { return TYPE##4(c   ? a   : b.x, c   ? a   : b.y, c   ? a   : b.z, c   ? a   : b.w); } \
	TYPE##4 select_internal(bool    c, TYPE##4 a, TYPE    b) { return TYPE##4(c   ? a.x : b  , c   ? a.y : b  , c   ? a.z : b  , c   ? a.w : b  ); } \
	TYPE##4 select_internal(bool    c, TYPE##4 a, TYPE##4 b) { return TYPE##4(c   ? a.x : b.x, c   ? a.y : b.y, c   ? a.z : b.z, c   ? a.w : b.w); } \
	TYPE##4 select_internal(bool4   c, TYPE    a, TYPE    b) { return TYPE##4(c.x ? a   : b  , c.y ? a   : b  , c.z ? a   : b  , c.w ? a   : b  ); } \
	TYPE##4 select_internal(bool4   c, TYPE    a, TYPE##4 b) { return TYPE##4(c.x ? a   : b.x, c.y ? a   : b.y, c.z ? a   : b.z, c.w ? a   : b.w); } \
	TYPE##4 select_internal(bool4   c, TYPE##4 a, TYPE    b) { return TYPE##4(c.x ? a.x : b  , c.y ? a.y : b  , c.z ? a.z : b  , c.w ? a.w : b  ); } \
	TYPE##4 select_internal(bool4   c, TYPE##4 a, TYPE##4 b) { return TYPE##4(c.x ? a.x : b.x, c.y ? a.y : b.y, c.z ? a.z : b.z, c.w ? a.w : b.w); } \

DEFINE_SELECT(bool)
#if !(COMPILER_HLSL || COMPILER_DXC || COMPILER_HLSLCC) || COMPILER_SWITCH
// @todo-lh: Ambiguous for DXC and HLSLcc if no suffix is provided for integer literals
DEFINE_SELECT(uint)
#endif
DEFINE_SELECT(int)
DEFINE_SELECT(float)
#if PLATFORM_SUPPORTS_REAL_TYPES
DEFINE_SELECT(half)
#if !(COMPILER_HLSL || COMPILER_DXC || COMPILER_HLSLCC)
// @todo-lh: Ambiguous for DXC and HLSLcc if no suffix is provided for integer literals
DEFINE_SELECT(uint16_t)
#endif
DEFINE_SELECT(int16_t)
#endif
#undef DEFINE_SELECT

float CalculateDistanceFieldApproxAO( float3 WorldPosition, float3 WorldNormal, uint NumSteps, float StepDistance, float StepScale, float DistanceBias, float MaxDistance = 0.0 )
{
	//...
	return 1.0;
}
bool GetLightmassReplaceState()
{
	//...
	return false;
}
float GetInitialisedSubstrateData()
{
	return 0;
}
bool GetPathTracingIsShadow()
{
	return false;
}
bool GetPathTracingIsIndirectDiffuse()
{
	return false;
}
bool GetPathTracingIsIndirectSpecular()
{
	return false;
}
bool GetPathTracingIsIndirectVolume()
{
	return false;
}

//5.4
//For convenience assuming WSVECTOR_IS_TILEOFFSET

#define FWSScalar FLWCScalar
#define FWSVector2 FLWCVector2
#define FWSVector3 FLWCVector3
#define FWSVector4 FLWCVector4

#define FWSScalarDeriv FLWCScalarDeriv
#define FWSVector2Deriv FLWCVector2Deriv
#define FWSVector3Deriv FLWCVector3Deriv
#define FWSVector4Deriv FLWCVector4Deriv
#define FWSMatrix FLWCMatrix
#define FWSInverseMatrix FLWCInverseMatrix

#define MakeWSScalar MakeLWCScalar
#define MakeWSVector2 MakeLWCVector2
#define MakeWSVector3 MakeLWCVector3
#define MakeWSVector4 MakeLWCVector4
#define MakeWSVector MakeLWCVector
#define MakeWSMatrix MakeLWCMatrix
#define MakeWSMatrix4x3 MakeLWCMatrix4x3
#define MakeWSInverseMatrix MakeLWCInverseMatrix
#define MakeWSInverseMatrix4x3 MakeLWCInverseMatrix4x3

#define WSGetComponent LWCGetComponent
#define WSGetX LWCGetX
#define WSGetY LWCGetY
#define WSGetZ LWCGetZ
#define WSGetW LWCGetW
#define WSSwizzle LWCSwizzle
#define WSDemote LWCToFloat
#define WSToFloat3x3 LWCToFloat3x3
#define WSPromote LWCPromote
#define WSPromoteInverse LWCPromoteInverse
#define WSMultiply LWCMultiply
#define WSMultiplyByPow2 LWCMultiply
#define WSMultiplyDemote LWCMultiply
#define WSMultiplyVector LWCMultiplyVector
#define WSMultiplyTranslation LWCMultiplyTranslation
#define WSGetOrigin LWCGetOrigin
#define WSSetOrigin LWCSetOrigin

#define WSSqrtDemote LWCSqrt
#define WSRsqrtDemote LWCRsqrt
#define WSRcpDemote LWCRcp

#define WSGreater LWCGreater
#define WSGreaterEqual LWCGreaterEqual
#define WSLess LWCLess
#define WSLessEqual LWCLessEqual
#define WSEquals LWCEquals
#define WSEqualsApprox LWCEqualsApprox

#define WSDdxDemote LWCDdx
#define WSDdyDemote LWCDdy

#define WSAdd LWCAdd
#define WSAddDemote(A, B) LWCToFloat(LWCAdd(A, B))
#define WSSubtract LWCSubtract
#define WSSubtractDemote(A, B) LWCToFloat(LWCSubtract(A, B))

#define WSSelect LWCSelect
#define WSNegate LWCNegate
#define WSFracDemote LWCFrac
#define WSFloor LWCFloor
#define WSCeil LWCCeil
#define WSRound LWCRound
#define WSTrunc LWCTrunc
#define WSSign LWCSign
#define WSSaturateDemote LWCSaturate
#define WSDivide LWCDivide
#define WSDivideByPow2 LWCDivide
#define WSLerp LWCLerp
#define WSFmodDemote LWCFmod

#define WSSin LWCSin
#define WSCos LWCCos
#define WSTan LWCTan
#define WSASin LWCASin
#define WSACos LWCACos
#define WSATan LWCATan

#define WSSmoothStepDemote LWCSmoothStep

#define WSMin LWCMin
#define WSMax LWCMax
#define WSAbs LWCAbs
#define WSStep LWCStep

#define WSVectorSum LWCVectorSum
#define WSDot LWCDot
#define WSLength LWCLength
#define WSRcpLengthDemote LWCRcpLength
#define WSNormalizeDemote LWCNormalize

#define GetTileOffsetUV( uv, Tex_ST ) uv * Tex_ST.xy + Tex_ST.zw

#if 1//WSVECTOR_IS_TILEOFFSET
	#define WSApplyAddressMode LWCApplyAddressMode
#elif WSVECTOR_IS_DOUBLEFLOAT
	#define WSApplyAddressMode DFApplyAddressMode
#endif

float AntialiasedTextureMask( Texture2D Tex, SamplerState Sampler, float2 UV, float ThresholdConst, int Channel )
{
	MaterialFloat4 MaskConst = MaterialFloat4( Channel == 0, Channel == 1, Channel == 2, Channel == 3 );

	const float WidthConst = 1.0f;
	float InvWidthConst = 1 / WidthConst;
	float Result;
	{
		// optimized, ddx/ddy only for every 2x2 block (bad for distant stuff)
		float Sample1 = dot( MaskConst, Texture2DSample( Tex, Sampler, UV ) );

		// compute the derivatives of the texture content
		float2 TexDD = float2( DDX( Sample1 ), DDY( Sample1 ) );

		float TexDDLength = max( abs( TexDD.x ), abs( TexDD.y ) );
		float Top = InvWidthConst * ( Sample1 - ThresholdConst );
		Result = Top / TexDDLength + ThresholdConst;
	}

	Result = saturate( Result );	// no always needed (e.g. DX9 framebuffer blending)

	return Result;
}
float MaterialExpressionCloudSampleAltitude( FMaterialPixelParameters Parameters )
{
	return 0.0f;
}
float MaterialExpressionDepthOfFieldFunction( float SceneDepth, int FunctionValueIndex )
{
	return 0;
}
float2 MaterialExpressionGetHairUV( FMaterialPixelParameters Parameters )
{
	return float2( 0, 0 );
}
float3 MaterialExpressionSkyAtmosphereLightIlluminanceOnGround( FMaterialPixelParameters Parameters, uint LightIndex )
{
	return float3( 0.0f, 0.0f, 0.0f );
}
float3 MaterialExpressionCloudEmptySpaceSkippingSphereCenterWorldPosition( FMaterialPixelParameters Parameters )
{
	return 0.0f;
}

//5.5
#define SHADER_PUSH_WARNINGS_STATE
#define SHADER_DISABLE_WARNINGS
#define SHADER_POP_WARNINGS_STATE

//5.6
float3 GetActorTranslatedWorldPosition(FMaterialVertexParameters Parameters)
{
	return transpose( UNITY_MATRIX_M)[ 3 ];
}
float3 GetActorTranslatedWorldPosition(FMaterialPixelParameters Parameters)
{
	return transpose( UNITY_MATRIX_M)[ 3 ];
}