float4x4 ScreenMatrix;
texture ScreenTexture;

float2 Resolution;
float2 ShadowOffset = float2(-0.5, -0.5); // Example offset (top-left)
float4 ShadowColor = float4(0.0, 0.0, 0.0, 0.65); // Semi-transparent black

float FXAAQualitySubpix = 0.69;
float FXAAQualityEdgeThreshold = 0.166;
float FXAAQualityEdgeThresholdMin = 0.0833;

sampler2D ScreenSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

float4 FXAA(float2 texCoord : TEXCOORD0) : COLOR0
{
    float2 inverseResolution = float2(1.0 / 800, 1.0 / 600);
    
    // Compute the shadow
    float2 shadowTexCoord = texCoord + ShadowOffset;
    float4 shadow = tex2D(ScreenSampler, shadowTexCoord) * ShadowColor;

    float4 color = tex2D(ScreenSampler, texCoord) * 0.5;

    float3 rgbNW = tex2D(ScreenSampler, texCoord + float2(-1.0, -1.0) * inverseResolution).xyz;
    float3 rgbNE = tex2D(ScreenSampler, texCoord + float2(1.0, -1.0) * inverseResolution).xyz;
    float3 rgbSW = tex2D(ScreenSampler, texCoord + float2(-1.0, 1.0) * inverseResolution).xyz;
    float3 rgbSE = tex2D(ScreenSampler, texCoord + float2(1.0, 1.0) * inverseResolution).xyz;
    float3 rgbM = tex2D(ScreenSampler, texCoord).xyz;

    float3 luma = float3(0.299, 0.587, 0.114);

    float lumaNW = dot(rgbNW, luma);
    float lumaNE = dot(rgbNE, luma);
    float lumaSW = dot(rgbSW, luma);
    float lumaSE = dot(rgbSE, luma);
    float lumaM = dot(rgbM, luma);

    float lumaMin = min(lumaM, min(min(lumaNW, lumaNE), min(lumaSW, lumaSE)));
    float lumaMax = max(lumaM, max(max(lumaNW, lumaNE), max(lumaSW, lumaSE)));

    float2 dir;
    dir.x = -((lumaNW + lumaNE) - (lumaSW + lumaSE));
    dir.y = ((lumaNW + lumaSW) - (lumaNE + lumaSE));

    float edgeThreshold = max(FXAAQualityEdgeThresholdMin, lumaMax * FXAAQualityEdgeThreshold);
    float isEdge = saturate((lumaMax - lumaMin) * (1.0 / edgeThreshold));

    if (isEdge < 0.066)
    {
        return float4(0.0, 0.0, 0.0, 0.0);
    }

    float2 dirReduce = max(float2(-2.0, -2.0) * inverseResolution, dir * (1.0 / (min(isEdge, 1.0) * 4.0)));
    float2 dirFinal = inverseResolution * dirReduce * (1.0 / 2.0);

    float3 rgbA = (1.0 / 2.0) * (tex2D(ScreenSampler, texCoord + dirFinal * (-1.0 / 3.0 - 0.5)).xyz + tex2D(ScreenSampler, texCoord + dirFinal * (1.0 / 3.0 - 0.5)).xyz);
    float3 rgbB = rgbA * (1.0 / 2.0) + (1.0 / 4.0) * (tex2D(ScreenSampler, texCoord + dirFinal * (-0.5)).xyz + tex2D(ScreenSampler, texCoord + dirFinal * (0.5)).xyz);
    float3 rgbC = rgbB * (3.0 / 4.0) + (1.0 / 4.0) * (1.0 / 2.0) * (tex2D(ScreenSampler, texCoord + dirFinal * (0.0 / 3.0 - 0.5)).xyz + tex2D(ScreenSampler, texCoord + dirFinal * (2.0 / 3.0 - 0.5)).xyz);
    
    float lumaA = dot(rgbA, luma);
    float lumaB = dot(rgbB, luma);
    float lumaC = dot(rgbC, luma);
    
    float dirBlend = saturate((lumaMin - lumaA) / (lumaMax - lumaA));
    dirBlend = smoothstep(0.0, 1.0, dirBlend);
    
    float3 rgbFinal = lerp(rgbA, rgbC, dirBlend);
    float subpix = clamp(1.0 / 8.0, 1.0 / 16.0, FXAAQualitySubpix);
    float aa = clamp((lumaMax - lumaMin) / subpix, 0.0, 1.0);
    aa = smoothstep(edgeThreshold, edgeThreshold + subpix, aa);
    // Blend the shadow with the final color
        float4 finalColor = lerp(color, float4(rgbFinal, color.a), aa);
        //finalColor = finalColor + shadow * (1.0 - finalColor.a);
        
        return finalColor;
}

technique FXAATechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 FXAA();
    }
}
