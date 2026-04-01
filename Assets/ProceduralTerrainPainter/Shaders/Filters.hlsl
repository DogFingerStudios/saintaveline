// Procedural Terrain Painter by Staggart Creations http://staggart.xyz
// Copyright protected under Unity Asset Store EULA

#define RAD2DEGREE 57.29578
#define CurvStrength 1
//SamplerState sampler_LinearClamp;
#define Clamp sampler_LinearClamp

#include "Noise.hlsl"

//X: Min height
//Y: Max height
//Z: Min height falloff
//W: Max height falloff
float HeightMask(float heightmap, float2 uv, float4 params)
{
	#define MIN_HEIGHT params.x
	#define MIN_HEIGHT_FALLOFF params.z
	#define MAX_HEIGHT params.y
	#define MAX_HEIGHT_FALLOFF params.w
	
	float minEnd = (MIN_HEIGHT - MIN_HEIGHT_FALLOFF);
	float minWeight = saturate((minEnd - (heightmap - MIN_HEIGHT) ) / (minEnd-MIN_HEIGHT));
	float maxEnd = MAX_HEIGHT + MAX_HEIGHT_FALLOFF;
	float maxWeight = saturate((maxEnd - (heightmap - MAX_HEIGHT) ) / (maxEnd-MAX_HEIGHT));

	//maxWeight = GradientNoise(uv, 50) * maxWeight;
	return saturate(maxWeight * minWeight);
}

float4 RemapNormals(float4 normals)
{
	normals.xyz = normals.xyz * 2.0 - 1.0;

	return normals;
}

float4 NormalFromHeight(sampler2D heightmap, float2 uv, float texelSize, float heightScale) {

	float strength = heightScale;

	float radius = texelSize;

	//Flatten border
	//if (uv.x < radius + 0.001 || uv.x > 1 - radius || uv.y < radius + 0.001 || uv.y > 1 - radius - 0.001) strength = 0;

	float xLeft = tex2D(heightmap, uv - float2(radius, 0.0)) * strength;
	float xRight = tex2D(heightmap, uv + float2(radius, 0.0)) * strength;

	float yUp = tex2D(heightmap, uv - float2(0.0, radius)) * strength;
	float yDown = tex2D(heightmap, uv + float2(0.0, radius)) * strength;

	float xDelta = ((xLeft - xRight) + 1.0) * 0.5f;
	float yDelta = ((yUp - yDown) + 1.0) * 0.5f;

	/*
#if UNITY_COLORSPACE_GAMMA
	xLeft = GammaToLinearSpace(xLeft);
	xRight = GammaToLinearSpace(xRight);
	yUp = GammaToLinearSpace(yUp);
	yDown = GammaToLinearSpace(yDown);
	xDelta = GammaToLinearSpace(xDelta);
	yDelta = GammaToLinearSpace(yDelta);
#endif
*/

	float4 normals = float4(xDelta, yDelta, 1.0, yDelta);

	return normals;
}

#define SOFT_CURVATURE 1

//https://blender.stackexchange.com/questions/89278/how-to-get-a-smooth-curvature-map-from-a-normal-map
float CurvatureFromNormal(sampler2D normals, sampler2D _Heightmap, float2 uv, float2 texelSize, uint mode)
{
	uint mip = 0;
	float curvature = 0;
	
	if(mode == 1) //Hard
	{
		float3 normal = (tex2Dlod(normals, float4(uv.x, uv.y, 0, mip))).rgb;
		normal = normalize(normal);

		const float3 right = (tex2Dlod(normals, float4(uv.x + texelSize.x, uv.y, 0, mip))).xyz * CurvStrength;
		const float3 left = (tex2Dlod(normals, float4(uv.x - texelSize.x, uv.y, 0, mip))).xyz * CurvStrength;
		const float3 up = (tex2Dlod(normals, float4(uv.x, uv.y + texelSize.x, 0, mip))).xyz * CurvStrength;
		const float3 down = (tex2Dlod(normals, float4(uv.x, uv.y - texelSize.x, 0, mip))).xyz * CurvStrength;
	
		float3 xpos = normal + right;
		float3 xneg = normal - left;
		float3 ypos = normal + up;
		float3 yneg = normal - down;

		float depth = UnpackHeightmap(tex2Dlod(_Heightmap, float4(uv.x, uv.y, 0, mip)).r);
		curvature = (cross(xneg, xpos).x - cross(yneg, ypos).y) * 4.0 / 1;
		curvature = 1-(curvature * 0.5 + 0.5);
	}
	else
	{
		float right = RemapNormals(tex2Dlod(normals, float4(uv.x + texelSize.x, uv.y, 0, mip))).x * CurvStrength;
		float left = RemapNormals(tex2Dlod(normals, float4(uv.x - texelSize.x, uv.y, 0, mip))).x * CurvStrength;

		float x = (right - left) + 0.5;

		float up = RemapNormals(tex2Dlod(normals, float4(uv.x, uv.y + texelSize.x, 0, mip))).y * CurvStrength;
		float down = RemapNormals(tex2Dlod(normals, float4(uv.x, uv.y - texelSize.x, 0, mip))).y * CurvStrength;

		float y = (up - down) + 0.5;

		//Overlay blending
		curvature = (y < 0.5) ? 2.0 * x * y : 1.0 - 2.0 * (1.0 - x) * (1.0 - y);
	}

	return curvature;
}

//Returns 0-90 degrees slope value
float SlopeFromNormal(float3 normal)
{
    return acos(dot(normal, float3(0.0, 0.0, 1.0))) * RAD2DEGREE;
}

//X: Min (in degrees)
//Y: Max (in degrees)
float SlopeMask(sampler2D _Heightmap, float2 uv, float4 params, float texelSize) {

	float width = texelSize;

	uint mip = 0;

	float centerHeight = UnpackHeightmap(tex2D(_Heightmap, uv).r);
	float posX = UnpackHeightmap(tex2Dlod(_Heightmap, float4(uv.x + width, uv.y, 0, mip)).r) - centerHeight;
	float negX = UnpackHeightmap(tex2D(_Heightmap, float4(uv.x - width, uv.y, 0, mip)).r) - centerHeight;
	float posY = UnpackHeightmap(tex2D(_Heightmap, float4(uv.x, uv.y + width, 0, mip)).r) - centerHeight;
	float negY = UnpackHeightmap(tex2D(_Heightmap, float4(uv.x, uv.y - width, 0, mip)).r) - centerHeight;

	float slope = sqrt((posX * posX) + (posY * posY) + (negX * negX) + (negY * negY)) * 90;

	//Edges of heightmap aren't interpolated with neighboring heightmaps, workaround is to simply flatten them
	//if (uv.x < width || uv.x > 1 - width || uv.y < width || uv.y > 1 - width) return 0;

	#define MIN_SLOPE params.x / 90
	#define MIN_SLOPE_FALLOFF params.z / 90
	#define MAX_SLOPE params.y/ 90
	#define MAX_SLOPE_FALLOFF params.w/ 90

	float minEnd = MIN_SLOPE - (MIN_SLOPE_FALLOFF);
	float minWeight = saturate((minEnd - (slope - MIN_SLOPE) ) / (minEnd-MIN_SLOPE));
	float maxEnd = MAX_SLOPE + MAX_SLOPE_FALLOFF;
	float maxWeight = saturate((maxEnd - (slope - MAX_SLOPE) ) / (maxEnd-MAX_SLOPE));

	return saturate(maxWeight * minWeight);
}

float CurvatureMask(sampler2D _NormalMap, sampler2D _Heightmap, float2 uv, float4 params, float texelSize, uint mode)
{
	float convexity = CurvatureFromNormal(_NormalMap, _Heightmap, uv, texelSize, mode);
	//return convexity;
	
	float curvature = (convexity - (1.0 - convexity)) * 0.5 + 0.5;
				
	#define MIN_CURV params.x
	#define MIN_CURV_FALLOFF params.z
	#define MAX_CURV params.y
	#define MAX_CURV_FALLOFF params.w

	float minEnd = (MIN_CURV - MIN_CURV_FALLOFF);
	float minWeight = saturate((minEnd - (curvature - MIN_CURV) ) / (minEnd-MIN_CURV));
	float maxEnd = MAX_CURV + MAX_CURV_FALLOFF;
	float maxWeight = saturate((maxEnd - (curvature - MAX_CURV) ) / (maxEnd-MAX_CURV));

	return saturate(maxWeight * minWeight);

}