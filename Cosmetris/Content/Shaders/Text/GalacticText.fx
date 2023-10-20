texture SpriteTexture;
float4 GlowColor = float4(1,1,1,1);
float GlowRadius = 48.0;
float2 Resolution;
float2 Padding = float2(24,24); // The amount of padding on each side of the texture, in pixels
float Time;

sampler2D SpriteSampler = sampler_state
{
    Texture = <SpriteTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float4 main(float2 texCoord : TEXCOORD0) : COLOR0
{
    float2 pixelSize = 1.0 / Resolution;
    float2 paddedSize = (Resolution - Padding * 2.0) / Resolution;
    float2 paddedTexCoord = texCoord * paddedSize + Padding / Resolution;

    float4 color = tex2D(SpriteSampler, paddedTexCoord);
    
    float glowStrength = 0.0;
    int sampleCount = 32;
    for (int i = 0; i < sampleCount; i++)
    {
        float angle = 2.0 * 3.14159265 * (i / float(sampleCount));
        float2 direction = float2(cos(angle), sin(angle));
        float2 offset = direction * pixelSize * GlowRadius;
        float4 neighborColor = tex2D(SpriteSampler, paddedTexCoord + offset);
        glowStrength += neighborColor.a;
    }

    glowStrength = glowStrength / float(sampleCount);
    glowStrength *= (1.0 + sin(Time * 2.0 * 3.14159) * 0.1);

    float4 glow = GlowColor * glowStrength * GlowColor.a;
    color.rgb += glow.rgb;

    // Time-based colored outline
    float outlineStrength = 1.0 - glowStrength;
    float3 outlineColor = lerp(color.rgb, GlowColor.rgb, outlineStrength);
    color.rgb = lerp(outlineColor, color.rgb, color.a);

    return color;
}

technique GalacticTextTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 main();
    }
}