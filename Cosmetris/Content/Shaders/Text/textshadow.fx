float2 ShadowOffset = float2(-1,-1);
float4 ShadowColor = float4(0,0,0,0.5);

texture2D SpriteTexture;

sampler2D SpriteSampler = sampler_state
{
    Texture = (SpriteTexture);
};

float4 main(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : SV_Target0
{
    float4 texColor = tex2D(SpriteSampler, texCoord);
    float4 shadow = tex2D(SpriteSampler, texCoord + ShadowOffset) * ShadowColor;

    return shadow + texColor * color;
}

technique SimpleTextShadowTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 main();
    }
}