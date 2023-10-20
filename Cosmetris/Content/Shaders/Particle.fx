float Time;

Texture2D ParticleTexture;
sampler2D ParticleSampler = sampler_state
{
    Texture = <ParticleTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Wrap;
    AddressV = Wrap;
};

float4 main(float4 position : SV_Position, float4 color : COLOR0, float2 texCoord : TEXCOORD0) : COLOR0
{
    float4 texColor = tex2D(ParticleSampler, texCoord);

    // Modify the alpha value based on the time
    float alpha = sin(Time * 10.0) * 0.5 + 0.5;
    texColor.a *= alpha;

    return texColor;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 main();
    }
}