void CTI_ShadowFade_half(
    half alpha,
    out half o_alpha
)
{
    #if (SHADERPASS == SHADERPASS_SHADOWS)
        o_alpha = 1.0;
    #else 
        o_alpha = alpha; 
    #endif
}

void CTI_ColorVariation_half(
    half3       Albedo,
    half4       ColorVariation,
    half        ColorVariationStrength,

    out half3   o_Albedo
)
{
    o_Albedo = lerp(Albedo, (Albedo + ColorVariation.rgb) * 0.5, (ColorVariationStrength * ColorVariation.a).xxx );
}

void CTI_UnpackFixNormal_half(
    half4       normalSample,
    half        normalScale,
    float       isFrontFace, 
    out half3   o_normalTS  
)
{
    o_normalTS = UnpackNormalAG(normalSample, normalScale);
    o_normalTS.z *= isFrontFace ? 1 : -1;
}

void CTI_UnpackNormal_half(
    half4       normalSample,
    half        normalScale,
    out half3   o_normalTS  
)
{
    o_normalTS = UnpackNormalAG(normalSample, normalScale);
}

void CTI_UnpackNormalBillboard_half(
    half4       normalSample,
    half        normalScale,
    out half3   o_normalTS  
)
{
    // Up is flipped!
    normalSample.g = 1 - normalSample.g;
    o_normalTS = UnpackNormalAG(normalSample, normalScale);
}

void CTI_AlphaLeak_half(
    half        alphaSample,
    half        leak,
    out half    o_Occlusion 
)
{
    o_Occlusion = (alphaSample <= leak) ? 1 : alphaSample; // Eliminate alpha leaking into ao
}