float Time;

sampler2D textTexture : register(s0);

float4 hsv2rgb(float4 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return float4(c.z * lerp(K.xxx, saturate(p - K.xxx), c.y), 1.0);
}

float4 main(float2 texCoord : TEXCOORD0) : COLOR0
{
    float2 pos = texCoord.xy;
    float4 color = float4(0, 0, 0, 0);

    float hue = (pos.x + Time * 0.1) * 0.15f;
    float4 rainbowColor = hsv2rgb(float4(hue, 1.0, 1.0, 1.0));

    // Sample the alpha channel of the text texture
    float textAlpha = tex2D(textTexture, texCoord).a;

    // Use the alpha channel as a mask for the rainbow color
    color.rgb = rainbowColor.rgb * textAlpha;
    color.a = textAlpha;

    return color;
}

technique RainbowTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 main();
    }
}