float4 _CTI_SRP_Wind;
float _CTI_SRP_Turbulence;

float3x3 GetRotationMatrix(float3 axis, float angle)
{
    //axis = normalize(axis); // moved to calling function
    float s = sin(angle);
    float c = cos(angle);
    float oc = 1.0 - c;

    return float3x3 (oc * axis.x * axis.x + c, oc * axis.x * axis.y - axis.z * s, oc * axis.z * axis.x + axis.y * s,
        oc * axis.x * axis.y + axis.z * s, oc * axis.y * axis.y + c, oc * axis.y * axis.z - axis.x * s,
        oc * axis.z * axis.x - axis.y * s, oc * axis.y * axis.z + axis.x * s, oc * axis.z * axis.z + c);
}

float4 SmoothCurve( float4 x ) {   
    return x * x *( 3.0 - 2.0 * x );   
}
float4 TriangleWave( float4 x ) {   
    return abs( frac( x + 0.5 ) * 2.0 - 1.0 );   
}
float4 SmoothTriangleWave( float4 x ) {   
    return SmoothCurve( TriangleWave( x ) );   
}
// Overloads for single float 
float SmoothCurve( float x ) {   
    return x * x *( 3.0 - 2.0 * x );   
}
float TriangleWave( float x ) {   
    return abs( frac( x + 0.5 ) * 2.0 - 1.0 );   
}
float SmoothTriangleWave( float x ) {   
    return SmoothCurve( TriangleWave( x ) );   
}

half3 CTI_UnpackScaleNormal(half4 packednormal, half bumpScale)
{
    half3 normal;
    normal.xy = (packednormal.wy * 2 - 1);
    #if (SHADER_TARGET >= 30)
        // SM2.0: instruction count limitation
        // SM2.0: normal scaler is not supported
        normal.xy *= bumpScale;
    #endif
    normal.z = sqrt(1.0f - saturate(dot(normal.xy, normal.xy)));
    return normal;
}

float4 AfsSmoothTriangleWave( float4 x ) {   
    return (SmoothCurve( TriangleWave( x )) - 0.5f) * 2.0f;   
}

void CTI_AnimateVertexSG_float(
    float3      PositionOS,
    half3       NormalOS,
    half4       VertexColor,
    float2      UV2,
    float3      UV3,
    
    float       leafNoise,

    float3      baseWindMultipliers,

    bool        enableNormalRotation,

    bool        EnableAdvancedEdgeBending,
    float2      AdvancedEdgeBending,

    float3      timeParams,

    bool        IsLeaves,

    out float3  o_positionOS,
    out half3   o_normalOS,
    out half2   o_colorVariationAmbient

) {  
    const float fDetailAmp = 0.1f;
    const float fBranchAmp = 0.3f;

    #define Phase animParams.r
    #define Flutter animParams.g
    #define MainBending animParams.z
    #define BranchBending animParams.w

    float4 animParams = float4(VertexColor.rg, UV2.xy);
    animParams.zwy *= baseWindMultipliers.xyz;

//  Init output    
    o_positionOS = PositionOS;
    o_normalOS = NormalOS;
//  Store ambient occlusion
    o_colorVariationAmbient = 0;

    const float3 TreeWorldPos = UNITY_MATRIX_M._m03_m13_m23;
    
    float sinuswave = sin(timeParams.x * 0.5);
    float shiftedsinuswave = (sinuswave + timeParams.y) * 0.5;

    //#if defined (_LEAFTUMBLING)
    //    float shiftedsinuswave = sin(_Time.y * 0.5 + _TimeOffset);
    //    float4 vOscillations = SmoothTriangleWave(float4(TreeWorldPos.x + sinuswave, TreeWorldPos.z + sinuswave * 0.7, TreeWorldPos.x + shiftedsinuswave, TreeWorldPos.z + shiftedsinuswave * 0.8));
    //#else
        float4 vOscillations = SmoothTriangleWave(float4(TreeWorldPos.x + sinuswave, TreeWorldPos.z + sinuswave * 0.7, TreeWorldPos.x + shiftedsinuswave, TreeWorldPos.z + shiftedsinuswave * 0.8));
    //#endif

    // x used for main wind bending / y used for tumbling
    float2 fOsc = vOscillations.xz + (vOscillations.yw * vOscillations.yw);
    fOsc = 0.75 + (fOsc + 3.33) * 0.33;

    // float fObjPhase = abs(frac((TreeWorldPos.x + TreeWorldPos.z) * 0.5) * 2 - 1);
    // NOTE: We have to limit (frac) fObjPhase in case we use fBranchPhase in tumbling or turbulence
    float fObjPhase = dot(TreeWorldPos, 1);
    float fBranchPhase = fObjPhase + Phase;
    float fVtxPhase = dot(o_positionOS.xyz, Flutter + fBranchPhase);
    
    // x is used for edges; y is used for branches
    float2 vWavesIn = timeParams.xx + float2(fVtxPhase, fBranchPhase );
    
    // 1.975, 0.793, 0.375, 0.193 are good frequencies
    float4 vWaves = (frac( vWavesIn.xxyy * float4(1.975, 0.793, 0.375, 0.193) ) * 2.0 - 1.0);
    vWaves = SmoothTriangleWave( vWaves );
    float2 vWavesSum = vWaves.xz + vWaves.yw;

//  Get local Wind
    #define absWindStrength _CTI_SRP_Wind.w

    float turbulence = _CTI_SRP_Turbulence;
    float3 windDir = mul((float3x3)GetWorldToObjectMatrix(), _CTI_SRP_Wind.xyz);
    float4 Wind = float4(windDir, absWindStrength);

//  Leaf specific bending
    if (IsLeaves)
    {
        float3 pivot;

    //  Decode UV3
        // 15bit compression 2 components only, important: sign of y
        pivot.xz = (frac(float2(1.0f, 32768.0f) * UV3.xx) * 2) - 1;
        pivot.y = sqrt(1 - saturate(dot(pivot.xz, pivot.xz)));
        pivot *= UV3.y;

        half tumbleInfluence = frac(VertexColor.b * 2.0);

    //  Move point to 0,0,0
        o_positionOS.xyz -= pivot;

        #if defined(_LEAFTUMBLING) || defined (_LEAFTURBULENCE)

            float3 fracs = frac(pivot * 33.3);
            
            //float offset = fracs.x + fracs.y + fracs.z; // /* this adds a lot of noise, so we use * 0.1 */ + (BranchBending  + Phase) * leafNoise;
            
            float offset = fracs.x + fracs.y + fracs.z  /* this adds a lot of noise, so we use * 0.1 */ + (BranchBending + Phase) * leafNoise;
            float tFrequency = _TumbleFrequency * (timeParams.x + fObjPhase * 10.0 );
            float4 vWaves1 = SmoothTriangleWave( float4( (tFrequency + offset) * (1.0 + offset * 0.25), tFrequency * 0.75 + offset, tFrequency * 0.5 + offset, tFrequency * 1.5 + offset));

            float3 windTangent = float3(-windDir.z, windDir.y, windDir.x);
            float twigPhase = vWaves1.x + vWaves1.y + (vWaves1.z * vWaves1.z);

            #define packedBranchAxis UV3.z
            
            //#if defined (_EMISSION)
                // This was the root of the fern issue: branchAxes slightly varied on different LODs!
                float3 branchAxis = frac( packedBranchAxis * float3(1.0, 256.0, 65536.0) );
                branchAxis = branchAxis * 2.0 - 1.0;
                branchAxis = normalize(branchAxis);
                // we can do better in case we have the baked branch main axis
                float facingWind = (dot(branchAxis, windDir));
            //#else
            //    half facingWind = (dot(normalize(float3(v.positionOS.x, 0, v.positionOS.z)), windDir)); //saturate 
            //#endif

            

            //float localWindStrength = dot(abs(xWind.xyz), 1) * tumbleInfluence * (1.35 - facingWind) * xWind.w + absWindStrength; // Use abs(_Wind)!!!!!!
            half localWindStrength = (1.35h - facingWind) * tumbleInfluence * absWindStrength;

        //  tumbling
            #if defined(_LEAFTUMBLING)
                // float angleTumble = 
                //  (twigPhase + fBranchPhase * 0.25 + BranchBending)
                //   * localWindStrength * tumbleStrength
                //   * fOsc.y
                // ;
            //  Let's keep it simple
                float angleTumble = (twigPhase + BranchBending) * localWindStrength * _TumbleStrength;
                
                float3x3 tumbleRot = GetRotationMatrix( windTangent, angleTumble);
                o_positionOS.xyz = mul(tumbleRot, o_positionOS.xyz);
                if(enableNormalRotation)
                {
                    o_normalOS = mul(tumbleRot, o_normalOS);
                }
            #endif

        //  turbulence
            #if defined (_LEAFTURBULENCE)
                // float angleTurbulence =
                //  // center rotation so the leaves rotate leftwards as well as rightwards according to the incoming waves
                //  ((twigPhase + vWaves1.w) * 0.25 - 0.5)
                //  // make rotation strength depend on absWindStrength and all other inputs
                //  * localWindStrength * leafTurbulence * saturate(lerp(1.0, Flutter * 8.0, _EdgeFlutterInfluence))
                //  * fOsc.x
                // ;
            //  Let's keep it simple
                float angleTurbulence =
                    ((twigPhase + vWaves1.w) * 0.25 - 0.5)
                    * localWindStrength * _LeafTurbulence * saturate(lerp(1.0, Flutter * 8.0, _EdgeFlutterInfluence))
                ;
                
                float3x3 turbulenceRot = GetRotationMatrix( -branchAxis, angleTurbulence);
                o_positionOS.xyz = mul(turbulenceRot, o_positionOS.xyz);
                if(enableNormalRotation)
                {
                    o_normalOS = mul(turbulenceRot, NormalOS);
                }
            #endif

        #endif


    //  fade in/out leave planes
            // float lodfade = ceil(pos.w - 0.51);
    //  asset store
            // float lodfade = (pos.w > 0.5) ? 1 : 0;
    //  latest
        if (unity_LODFade.x < 1.0)
        {
            float lodfade = (VertexColor.b > (1.0f / 255.0f * 126.0f) ) ? 1 : 0; // Make sure that the 1st vertex is taken into account
            o_positionOS.xyz *= 1.0 - unity_LODFade.x * lodfade;
        }
    
    //  Move point back to origin
        o_positionOS.xyz += pivot;

    //  Advanced edge fluttering (has to be outside preserve length)
        if(EnableAdvancedEdgeBending)
        {
            o_positionOS.xyz += o_normalOS.xyz * SmoothTriangleWave( tumbleInfluence * timeParams.x * AdvancedEdgeBending.y + Phase ) * AdvancedEdgeBending.x * Flutter * absWindStrength;
        }
    }

//  Preserve Length
    float origLength = length(o_positionOS.xyz);

//  Primary bending / Displace position
    o_positionOS.xyz += animParams.z * Wind.xyz * fOsc.x * absWindStrength ;

    #if defined(_NORMALIZEBRANCH)
//  Preserve Length - good here but stretches real branches
        o_positionOS.xyz = normalize(o_positionOS.xyz) * origLength;
    #endif

//  Apply secondary bending and edge flutter
    float3 bend = animParams.y * fDetailAmp * abs(o_normalOS.xyz);
    bend.y = animParams.w * fBranchAmp;
    o_positionOS.xyz += ((vWavesSum.xyx * bend) + (Wind.xyz * vWavesSum.y * animParams.w)) * absWindStrength  * _CTI_SRP_Turbulence; 

    #if !defined(_NORMALIZEBRANCH)
//  Preserve Length - good here but stretches real branches
        o_positionOS.xyz = normalize(o_positionOS.xyz) * origLength;
    #endif

    //  Store Variation
    #if !defined(UNITY_PASS_SHADOWCASTER) && !defined(DEPTHONLYPASS)
        o_colorVariationAmbient.x = saturate ( ( frac(TreeWorldPos.x + TreeWorldPos.y + TreeWorldPos.z) + frac( (TreeWorldPos.x + TreeWorldPos.y + TreeWorldPos.z) * 3.3 ) ) * 0.5 );
        o_colorVariationAmbient.y = VertexColor.a;
    #endif

}