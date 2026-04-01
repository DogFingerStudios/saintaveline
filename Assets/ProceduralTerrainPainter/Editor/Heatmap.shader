Shader "Hidden/TerrainPainter/Heatmap"
{
    SubShader
    {
        Cull Back ZWrite On
        Blend SrcAlpha OneMinusSrcAlpha

        HLSLINCLUDE

        #include "UnityCG.cginc"
        #include "TerrainPreview.cginc"

        sampler2D _LayerMaskTex;
        sampler2D _NormalMap;
        float4 _Heightmap_TexelSize;
        float4 _LayerTiling; //XY: Tling //ZW: Resolution
        float _ContourOn;
        float _TilingOn;

        ENDHLSL

        Pass
        {
            Name "Terrain layer heatmap"

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            struct Varyings {
                float4 positionCS : SV_POSITION;
                float3 positionWorld : TEXCOORD0;
                float3 positionWorldOrig : TEXCOORD1;
                float2 pcPixels : TEXCOORD2;
                float2 uv : TEXCOORD3;
            };

            Varyings vert(uint vid : SV_VertexID)
            {
                Varyings o;
                
                // build a quad mesh, with one vertex per paint context pixel (pcPixel)
                float2 pcPixels = BuildProceduralQuadMeshVertex(vid);

                // compute heightmap UV and sample heightmap
                float2 heightmapUV = PaintContextPixelsToHeightmapUV(pcPixels);
                float heightmapSample = UnpackHeightmap(tex2Dlod(_Heightmap, float4(heightmapUV, 0, 0)));

                // compute brush UV
                float2 brushUV = PaintContextPixelsToBrushUV(pcPixels);

                // compute object position (in terrain space) and world position
                float3 positionObject = PaintContextPixelsToObjectPosition(pcPixels, heightmapSample);
                float3 positionWorld = TerrainObjectToWorldPosition(positionObject);

                o.pcPixels = pcPixels;
                o.positionWorld = positionWorld;
                o.positionWorldOrig = positionWorld;
                o.positionCS = UnityWorldToClipPos(positionWorld);
                o.uv = brushUV;
                return o;
            }

            float Contours(sampler2D heightmap, float2 uv)
            {
				float contourLineFactor = 512;
				float majorLineSteps = contourLineFactor / (contourLineFactor/8);
				float thickness = _Heightmap_TexelSize.x * 0.5;
				uint mip = 0;

				//White base
				float lines = 1;

				//Cross-kernel sampling
				float corner0 = tex2Dlod(heightmap, float4(uv.x - thickness, uv.y, 0, mip)).r;
				float corner1 = tex2Dlod(heightmap, float4(uv.x + thickness, uv.y, 0, mip)).r;
				float corner2 = tex2Dlod(heightmap, float4(uv.x, uv.y + thickness, 0, mip)).r;
				float corner3 = tex2Dlod(heightmap, float4(uv.x + thickness, uv.y + thickness, 0, mip)).r;

				//Elevation range of the pixel's area
				float minHeight = min(min(corner0, corner1), min(corner2, corner3));
				float maxHeight = max(max(corner0, corner1), max(corner2, corner3));

				//Check if the pixel's area crosses at least one contour line, creating a secondary line
				if (floor(minHeight * contourLineFactor) != floor(maxHeight * contourLineFactor))
				{
					lines = 0.75;
				}

				//Major lines
				if (floor(minHeight * contourLineFactor / majorLineSteps) != floor(maxHeight * contourLineFactor / majorLineSteps))
				{
					lines = 0.5;
				}

				return 1-lines;
			}

            inline float mod(float a, float b) { return a - (b * floor(a / b)); }

            float Checker(float2 uv)
            {
				float fmodResult = mod(floor(uv.x) + floor(uv.y), 2.0);
				return max(sign(fmodResult), 0.0);
            }


            float4 frag(Varyings i) : SV_Target
            {
                float layerAlpha = UnpackHeightmap(tex2D(_LayerMaskTex, i.uv)).r;

            	float lines = lerp(0, Contours(_Heightmap, i.uv), _ContourOn);
            	float texelChecker = Checker(i.positionWorld.xz * _LayerTiling.xy * 2.0);
            	texelChecker = lerp(1, saturate(texelChecker + 0.75), _TilingOn);
            	//return float4(texelChecker.xxx, 0.7);

            	float3 normal = tex2D(_NormalMap, i.uv);
            	
                float3 lightDir = UnityWorldSpaceLightDir(i.positionWorld.xyz);
            	//lightDir = normalize(_WorldSpaceCameraPos - i.positionWorld);

            	//Add some diffuse shading to visualize the contour of the terrain
            	float diffuse = saturate(dot(lightDir, normal) + 0.66);

            	float3 color = float3(layerAlpha, 0.0, 0.0) - lines;
            	
                return float4(color * diffuse * texelChecker, layerAlpha);
            }
            ENDHLSL
        }
    }
    Fallback Off
}