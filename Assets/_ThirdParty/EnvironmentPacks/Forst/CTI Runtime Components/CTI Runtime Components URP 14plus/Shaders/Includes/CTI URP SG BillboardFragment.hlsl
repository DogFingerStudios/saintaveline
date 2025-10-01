// URP uses _half

void CTIBillboardFragment_half (
	in float4  		UV,
	in real2        BlendCv,
	in float4  		ScreenPosRaw,

	out real3 		o_Albedo,
	out real  		o_Alpha,
	out real3 		o_NormalTS,
	out real  		o_Smoothness,
	out real 		o_Occlusion,
	out real  		o_Thickness
) {


	real4 sampleA = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, UV.xy);
	real4 n_sampleA = SAMPLE_TEXTURE2D(_BumpSpecMap, sampler_BumpSpecMap, UV.xy);

	real blend = saturate(1.0 - BlendCv.x);

	if (_BlendBB)
	{
		real4 sampleB = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, UV.zw);
		real4 n_sampleB = SAMPLE_TEXTURE2D(_BumpSpecMap, sampler_BumpSpecMap, UV.zw);	
	
	//	Dither
		if (_BlendBBDithering)
		{
		    float2 screenPos = floor( (ScreenPosRaw.xy / ScreenPosRaw.w) * _ScreenParams.xy);
		    real alphaClip = InterleavedGradientNoise(float4(screenPos, ScreenPosRaw.zw), (real)0.0);
			// real alphaClip = GenerateHashedRandomFloat( asuint((int2)screenPos.xy) );

			real blend = BlendCv.x;     
			blend = saturate( ((real)1.0 - blend - alphaClip) * (real)1000.0);
		}

		sampleA.a = sampleA.a * blend;
		sampleB.a = sampleB.a * (1.0 - blend);

		sampleA.rgb = sampleA.rgb * blend;
		sampleA.rgb += sampleB.rgb * (1.0 - blend);
	
		//n_sampleA = n_sampleA * blend + n_sampleB * (1.0 - blend); // Needs dithering!
		n_sampleA = lerp(n_sampleA, n_sampleB, sampleB.aaaa);

		sampleA.a += sampleB.a;
	}

	// Up is flipped!
    n_sampleA.g = 1.0 - n_sampleA.g;
	real3 normalTS = UnpackNormalAG(n_sampleA, _BumpScale);

//	Alpha Leak
	o_Occlusion = (sampleA.a <= _AlphaLeak) ? (real)1.0 : sampleA.a; // Eliminate alpha leaking into ao

//	Add Color Variation
	o_Albedo = lerp(sampleA.rgb, (sampleA.rgb + _HueVariation.rgb) * (real)0.5, (BlendCv.y * _HueVariation.a).xxx );
	o_Alpha = sampleA.a;
	o_NormalTS = normalTS;
	o_Smoothness = n_sampleA.b * _Smoothness;
    o_Thickness = n_sampleA.r * _ThicknessRemap;
}