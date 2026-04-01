// Procedural Terrain Painter by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

Shader "Hidden/TerrainPainter/Modifier"
{
    Properties
    {
		_MainTex("Input", 2D) = "black" {}
    	_BlendOp("Blend Operation", float) = 0
    	_Opacity("Opacity / 100", float) = 1
        _Heightmap ("_Heightmap", 2D) = "black" {}
		_NormalMap("NormalMap", 2D) = "bump" {}
		_MaskTexture("Mask", 2D) = "white" {}

    	//Filters
		_MinMaxHeight("MinMaxHeight", Vector) = (0,1,0,0)
		_MinMaxCurvature("_MinMaxCurvature", Vector) = (0,1,0,0)
		_MinMaxSlope("_MinMaxSlope", Vector) = (0,1,0,0)

		_HeightmapScale("Height scale", float) = 0
    }

	HLSLINCLUDE

	#include "UnityCG.cginc"
	#include "Filters.hlsl"

	float _BlendOp;
	float _Opacity;
	
	sampler2D _MainTex;
	sampler2D _MaskTexture;
	
	sampler2D _Heightmap;
	float4 _Heightmap_ST;
	float4 _Heightmap_TexelSize;
	
	sampler2D _NormalMap;
	float4 _NormalMap_ST;
	float4 _NormalMap_TexelSize;
	
	float4 _MinMaxHeight;
	float4 _MinMaxCurvature;
	float4 _MinMaxSlope;

	float _HeightmapScale;

	#define BLEND_OP_ADD 0
	#define BLEND_OP_SUB 2 //Actually ReverseSubtract
	#define BLEND_OP_MIN 3
	#define BLEND_OP_MAX 4
	#define BLEND_OP_MUL 21

	//Base values that's lerped from with _Opacity
	//Note: Subtraction base is 1, because ReverseSubtract is used
	#define BASE _BlendOp == BLEND_OP_ADD || _BlendOp == BLEND_OP_SUB || _BlendOp == BLEND_OP_MAX ? 0.0 : 1.0

	struct Varyings
	{
		float2 uv : TEXCOORD0;
		float4 vertex : SV_POSITION;
	};
	
	Varyings vert(float4 vertex : POSITION, float2 uv : TEXCOORD0)
	{
		Varyings o;
		o.vertex = UnityObjectToClipPos(vertex);
		o.uv = uv;
		return o;
	}

	float SampleSource(Varyings i)
	{
		return tex2D(_MainTex, i.uv).r;
	}

	float SampleHeightmap(Varyings i)
	{
		return tex2D(_Heightmap, i.uv).r * _HeightmapScale * 4;
	}

	float LinearHeight(float height)
	{
		return height / (_HeightmapScale * 4);
	}
	
	ENDHLSL

    SubShader
    {
    	Tags{ "RenderType" = "Transparent" "Queue" = "Transparent" }
    	Blend [_SrcFactor][_DstFactor]
		BlendOp [_BlendOp]
    	ColorMask RGBA
    	AlphaToMask Off
    	
    	
		Pass
		{
			Name "Height"
			HLSLPROGRAM
			
			#pragma vertex vert
			#pragma fragment frag

			fixed4 frag(Varyings i) : SV_Target
			{
				//return i.uv.y;
				//float source = SampleSource(i);
				const float heightmap = SampleHeightmap(i);

				const float mask = HeightMask(heightmap, i.uv, _MinMaxHeight);

				return lerp(BASE, mask, _Opacity);
			}
			ENDHLSL
		}
		Pass
		{		
			Name "Slope"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			fixed4 frag(Varyings i) : SV_Target
			{			
				const float mask = SlopeMask(_Heightmap, i.uv, _MinMaxSlope, _Heightmap_TexelSize.x);

				return lerp(BASE, mask, _Opacity);
			}
			ENDHLSL
		}
    	Pass
		{	
			Name "Curvature"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			float _CurvatureRadius;
			uint _CurvatureSolver;

			fixed4 frag(Varyings i) : SV_Target
			{
				float normal = RemapNormals(tex2Dlod(_NormalMap, float4(i.uv.x, i.uv.y, 0, 0))).rgb ;

				float3 dx = fwidth(normal);

				//return float4(dx, 1.0);
				const float mask = CurvatureMask(_NormalMap, _Heightmap, i.uv, _MinMaxCurvature, _NormalMap_TexelSize.x * _CurvatureRadius, _CurvatureSolver);

				return lerp(BASE, mask, _Opacity);
			}
			ENDHLSL
		}
    	Pass
		{		
			Name "Texture Mask"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float _Channel;
			float2 _TilingParams;
			//X: Tiling
			//Y: (bool) Span terrains

			float4 _TerrainPosScale;

			fixed4 frag(Varyings i) : SV_Target
			{
				float2 boundsUV = (_TerrainPosScale.zw * i.uv) + _TerrainPosScale.xy;
				float2 uv = lerp(i.uv * _TilingParams.x, boundsUV, _TilingParams.y);
				
				return lerp(BASE, tex2D(_MaskTexture, uv)[_Channel], _Opacity);
			}
			ENDHLSL
		}
    	Pass
		{		
			Name "Noice"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			//XY: Corner
			//ZW: Size
			float4 _TerrainPosScale;
			
			//X: Min corner
			//Y: Max corner
			//Z: Size X
			//W: Size Z
			float4 _TerrainBounds;
			
			float4 _NoiseScaleOffset;
			float4 _Levels;
			uint _NoiseType;

			fixed4 frag(float2 uv : TEXCOORD0) : SV_Target
			{
				float2 boundsUV = (_TerrainPosScale.zw * uv) + _TerrainPosScale.xy;

				float2 coords = (boundsUV.xy + _NoiseScaleOffset.zw) * _NoiseScaleOffset.xy * _TerrainBounds.zw;

				float mask = 0;

				if(_NoiseType == 1)
				{
					mask = GradientNoise(coords) * 0.5 + 0.5;

				}
				if(_NoiseType == 0)
				{
					mask = SimplexNoise(coords);
				}

				mask = smoothstep(_Levels.x, _Levels.y, mask);

				return lerp(BASE, mask, _Opacity);
			}
			ENDHLSL
		}
    	Pass
		{		
			Name "Direction"
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			float3 _Direction;
			float4 _DirectionLevels;

			fixed4 frag(Varyings i) : SV_Target
			{
				const float3 normal = RemapNormals(tex2Dlod(_NormalMap, float4(i.uv.x, i.uv.y, 0, 0))).xyz;

				const float aspect = dot(_Direction, -normal);
				
				float dirMask = smoothstep(_DirectionLevels.x, _DirectionLevels.y, aspect);
				
				return lerp(BASE, dirMask, _Opacity);
			}
			ENDHLSL
		}
    }
}
