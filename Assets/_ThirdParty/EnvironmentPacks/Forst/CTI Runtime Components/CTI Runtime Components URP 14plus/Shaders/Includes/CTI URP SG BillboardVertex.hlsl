// Not needed as we use our own calculation for eyevec!

// CBUFFER_START(UnityBillboardPerCamera)
// 	float3 unity_BillboardNormal;
// 	float3 unity_BillboardTangent;
// 	float4 unity_BillboardCameraParams;
// 	#define unity_BillboardCameraPosition (unity_BillboardCameraParams.xyz)
// 	#define unity_BillboardCameraXZAngle (unity_BillboardCameraParams.w)
// CBUFFER_END

CBUFFER_START(UnityBillboardPerBatch)
	float3 unity_BillboardSize;
CBUFFER_END
	
	float4 _CTI_SRP_Wind;
	float _CTI_SRP_Turbulence;

#if defined(_PARALLAXMAP)
	float2 _CTI_TransFade;
#endif


float4 SmoothCurve(float4 x) {
	return x * x * (3.0 - 2.0 * x);
}

float4 TriangleWave(float4 x) {
	return abs(frac(x + 0.5) * 2.0 - 1.0);
}

float4 SmoothTriangleWave(float4 x) {
	return (SmoothCurve(TriangleWave(x)) - 0.5) * 2.0;
}

// Billboard Vertex Function
void CTI_BillboardVert_float (
	float3  	positionOS,
	float2  	texcoord,
	float3 		texcoord1,

	// float3 	lightDir,

	out float3 	o_positionOS,
	out real3 	o_normalOS,
	out real3 	o_tangentOS,
	out float4  o_uv,
	out real2 	o_cv
)
{

	float3 position = positionOS;
	float3 positionWS = positionOS + UNITY_MATRIX_M._m03_m13_m23;

//	Store Color Variation
	//float3 TreeWorldPos = abs(positionWS.xyz * 0.125f);
	o_cv.y = saturate((frac(positionWS.x + positionWS.y + positionWS.z) + frac((positionWS.x + positionWS.y + positionWS.z) * 3.3)) * 0.5);

// 	////////////////////////////////////
//	Set vertex position
	// #if (SHADERPASS == SHADERPASS_SHADOWCASTER)
	// 	float3 eyeVec = -lightDir; //normalize(GetCurrentViewPosition() - positionWS);
	// #else
	// 	float3 eyeVec = normalize(_WorldSpaceCameraPos - positionWS);
	// #endif
//	Shadows go nuts:
	// float3 eyeVec = normalize(unity_BillboardCameraPosition - positionWS);

//	So we do it manually
//	We do not have access to _LightDirection or _LightPosition as these are defined later in code...
	#if (SHADERPASS == SHADERPASS_SHADOWCASTER)
		#define cameraForward UNITY_MATRIX_V[2].xyz 
		#if _CASTING_PUNCTUAL_LIGHT_SHADOW
		//	Matches HDRP GetCurrentViewPosition()
    		float3 eyeVec = normalize(UNITY_MATRIX_I_V._14_24_34 - positionWS); 	// normalize(_LightPosition - worldPos);
		#else
		    float3 eyeVec = cameraForward; 											// _LightDirection;
		#endif
	#else
		float3 eyeVec = GetWorldSpaceNormalizeViewDir(positionWS);
	#endif

//	NOTE: We have incorrect triangle winding...
	float3 billboardTangent = normalize(float3(-eyeVec.z, 0, eyeVec.x));
	real3 billboardNormal = real3(billboardTangent.z, 0, -billboardTangent.x);
	float2 percent = texcoord.xy;
	float3 billboardPos = (percent.x - 0.5) * unity_BillboardSize.x * texcoord1.x * billboardTangent;
	billboardPos.y += (percent.y * unity_BillboardSize.y + unity_BillboardSize.z) * texcoord1.y;
 
//	Wind - make sure we apply it in "object space" (use billboardPos)
	if (_WindStrength > 0)
	{
		float origLength = length(billboardPos);
		float sinuswave = _SinTime.z;
		float4 vOscillations = SmoothTriangleWave(float4(positionWS.x + sinuswave, positionWS.z + sinuswave * 0.8, 0.0, 0.0));
		float fOsc = vOscillations.x + (vOscillations.y * vOscillations.y);
		fOsc = 0.75 + (fOsc + 3.33) * 0.33;
	//	saturate added to stop warning on dx11...
        float percentage = pow(saturate(percent.y), _WindPower); // pow(y,1.5) matches the wind baked to the mesh trees
		billboardPos.xyz += _CTI_SRP_Wind.xyz * ( _CTI_SRP_Wind.w * _WindStrength * fOsc * percentage );	
		billboardPos = normalize(billboardPos) * origLength;
	}
	
//	Now bring it to the proper position
	position.xyz += billboardPos;
	o_positionOS.xyz = position.xyz;

// 	////////////////////////////////////
//	Get billboard texture coords
	o_uv = 0.0;
	o_cv.x = 0.0;

	float angle = atan2(billboardNormal.z, billboardNormal.x);	// signed angle between billboardNormal to {0,0,1}
	angle += angle < 0 ? 2 * PI : 0;										
//	Set Rotation
	angle += texcoord1.z;
//	Write final billboard texture coords
	const float invDelta = 1.0 / (45.0 * ((PI * 2.0) / 360.0));
	float imageIndex = fmod(floor(angle * invDelta + 0.5f), 8.0);
	float2 column_row;
	column_row.x = imageIndex * 0.25;
	column_row.y = saturate(4.0 - imageIndex) * 0.5;
	o_uv.xy = column_row + texcoord.xy * float2(0.25, 0.5);

//	/////////////////////////
	if (_BlendBB)
	{
		float percentage = frac(angle / (PI / 4));
	//	So percentage 0 - 1: 0 - 0.5 = next texture / 0.5 - 1 prev texture
		imageIndex = (percentage < 0.5) ? imageIndex + 1.0 : imageIndex - 1.0;	
	//	Needed!
		imageIndex = (imageIndex < 0.0) ? 7.0 : imageIndex;
		imageIndex = (imageIndex > 7.0) ? 0.0 : imageIndex;

		column_row.x = imageIndex * 0.25; // we do not care about the horizontal coord that much as our billboard texture tiles
		column_row.y = saturate(4.0 - imageIndex) * 0.5;
		o_uv.zw = column_row + texcoord.xy * float2(0.25, 0.5);

	//	Set Blend value
		o_cv.x = (percentage < 0.5) ? (percentage * 2.0) : (1.0 - percentage * 2.0);
		//o_cv.x = smoothstep (0, 1, o_cv.x); // Nope
	}

// 	////////////////////////////////////
//	Set Normal and Tangent
	o_normalOS = billboardNormal.xyz;
//	We have to fix normalTS in pixel shader as up is flipped!?
	o_tangentOS = billboardTangent.xyz;
}