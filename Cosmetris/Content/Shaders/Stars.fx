sampler s0 : register(s0);
float Time;
float2 Resolution;
texture2D StarTextures[3];

float rand(float2 co)
{
    return frac(sin(dot(co.xy, float2(12.9898, 78.233))) * 43758.5453);
}

float4 main(float2 texCoord : TEXCOORD0) : COLOR0
{
    float2 pos = texCoord.xy * Resolution;
    float4 color = float4(0, 0, 0, 0);

    // Generate random values for star position, size, and texture
    float randPos = rand(floor(pos * 0.05) + Time * 0.01);
    float randSize = rand(floor(pos * 0.1) + Time * 0.005);
    float randTex = rand(floor(pos * 0.2) + Time * 0.0025);

    // Determine star position
    float2 starPos = frac(float2(randPos, randPos * 0.5));

    // Calculate distance to the star
    float distance = length(pos - starPos * Resolution);

    // Calculate star size and select a star texture
    float starSize = lerp(0.1, 3.0, randSize);
    int starIndex = int(floor(randTex * 3.0));

    // Sample the selected star texture
    float4 starTexture = StarTextures[starIndex].Sample(sampler_state, texCoord);

    // Fade in and fade out
    float fadeInOut = sin(Time * rand(floor(pos * 0.1)) * 0.5 + 1.5);

    // If distance is within the star size, draw the star and apply fading
    if (distance < starSize)
    {
        color = starTexture;
        color.a *= fadeInOut;
    }

    return color;
}



technique StarTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 main();
    }
}
