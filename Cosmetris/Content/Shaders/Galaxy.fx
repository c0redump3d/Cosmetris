/*
 * Galaxy.fx is part of Cosmetris.
 *
 * Copyright (c) 2023 CKProductions, https://ckproductions.dev/
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */
sampler2D NoiseTexture : register(s0);
sampler2D CloudTexture : register(s1);

float Time;
float LinesCompleted = 0.0;
float3 GalaxyColorOne = float3(0.05, 0.01, 0.15);
float3 GalaxyColorTwo = float3(0.1, 0.05, 0.3);
float3 GalaxyColorThree = float3(0.902, 0.706, 0.012);
float3 NebulaColor = float3(0.875, 0.11, 0.255);
float3 CloudColor = float3(1.0, 0.9, 0.7);

struct Asteroid
{
    float2 position;
    float size;
    float speed;
};

float2 mod289(float2 x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
float mod289(float x) { return x - floor(x * (1.0 / 289.0)) * 289.0; }
float permute(float x) { return mod289((34.0 * x + 1.0) * x); }

float4 taylorInvSqrt(float4 r)
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

float taylorInvSqrt(float r)
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

float rand(float2 input)
{
    return frac(sin(dot(input, float2(12.9898, 78.233))) * 43758.5453);
}

float rand(float x, float y)
{
    float dotProduct = dot(float2(x, y), float2(12.9898, 78.233));
    return frac(sin(dotProduct) * 43758.5453);
}

float mix(float x, float y, float a)
{
    return x * (1.0 - a) + y * a;
}

float3 rgb2hsv(float3 c)
{
    float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
    float4 p = mix(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
    float4 q = mix(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

    float d = q.x - min(q.w, q.y);
    float e = 1.0e-10;
    return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}

float3 hsv2rgb(float3 c)
{
    float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
    return c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
}

float snoise(float2 v)
{
    const float4 C = float4(0.211324865405187, 0.366025403784439, -0.577350269189626, 0.024390243902439);
    float2 i = floor(v + dot(v, C.yy));
    float2 x0 = v - i + dot(i, C.xx);
    float2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    float4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;
    i = mod289(i);
    float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));
    float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
    m = m * m;
    m = m * m;
    float3 x = 2.0 * frac(p * C.www) - 1.0;
    float3 h = abs(x) - 0.5;
    float3 ox = floor(x + 0.5);
    float3 a0 = x - ox;
    m *= taylorInvSqrt(a0 * a0 + h * h);
    float3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}

float bigStarGlow(float2 texCoord)
{
    float2 starPos = texCoord * float2(300.0, 400.0);
    float star = snoise(starPos * 0.5 + Time * 0.025);
    return smoothstep(0.85, 0.95, star);
}

float perlin(float2 input)
{
    float2 i = floor(input);
    float2 j = frac(input);
    float2 coord = smoothstep(0.0, 1.0, j);

    float a = rand(i);
    float b = rand(i + float2(1.0, 0.0));
    float c = rand(i + float2(0.0, 1.0));
    float d = rand(i + float2(1.0, 1.0));

    return mix(mix(a, b, coord.x), mix(c, d, coord.x), coord.y);
}

float blendingFunction(float2 texCoord)
{
    float blend_factor = 0.005;
    float2 blend_vector = abs(frac(texCoord) - 0.5) / blend_factor;
    float perlin_blend = perlin(texCoord * 0.65);
    return min(blend_vector.x, blend_vector.y) * (1.0 - perlin_blend);
}

float nebula(float2 texCoord, float time, sampler2D noiseTexture)
{
    // Constants
    float2 offset1 = float2(0.4, 0.4);

    // Generate a random movement vector based on time
    // This uses sin/cos functions for pseudo-randomness, which can be adjusted as needed
    float2 randomMove = float2(sin(time * 0.3) * 0.5 + 0.5, cos(time * 0.47) * 0.5 + 0.5);

    // Apply the random movement to the texture coordinates
    // Since the texture is in tiling mode, it will wrap around
    texCoord += randomMove;

    // Sample the noise texture
    float neb_intensity1 = tex2D(noiseTexture, texCoord + time * 0.34).r;
    float neb_intensity2 = tex2D(noiseTexture, texCoord + offset1 + time * 0.68).r;

    float warpIntensity = 15.5;
    float perlinValue1 = tex2D(noiseTexture, texCoord).r;
    float perlinValue2 = tex2D(noiseTexture, texCoord + offset1).r;
    float2 warp = warpIntensity * float2(perlinValue1 * neb_intensity1, perlinValue2 * neb_intensity2);
    float2 warpedTexCoord = texCoord + warp;

    float noise1 = tex2D(noiseTexture, warpedTexCoord).r;
    float blend = blendingFunction(warpedTexCoord);

    float neb_intensity = lerp(neb_intensity1, neb_intensity2, blend);

    // Convert nebula color from RGB to HSV
    float4 nebColorRGB = float4(NebulaColor.x, NebulaColor.y, NebulaColor.z, 1.0);
    float3 nebColorHSV = rgb2hsv(nebColorRGB.rgb);
    nebColorHSV.x += noise1 * 0.92;

    // Convert back to RGB
    float3 nebColorRGBAdjusted = hsv2rgb(nebColorHSV);
    float4 nebColor = float4(nebColorRGBAdjusted, 1.0);

    float nebSize = 0.12;
    float nebDist = distance(texCoord, float2(0.15, 0.5));
    float nebFade = smoothstep(0.4, 0.6, nebDist);

    return mix(neb_intensity, nebFade, nebSize) * nebColor.a;
}

float starGlow(float2 texCoord)
{
    float2 starPos = texCoord * float2(100.0, 200.0);
    float star = snoise(starPos + Time * 0.025);
    return smoothstep(0.88, 0.92, star);
}

float dustCloud(float2 texCoord, float time)
{
    // Define some scales for the layers.
    float2 scales[] = {
        float2(0.05, 0.05),
        float2(0.3, 0.3),
        float2(0.1, 0.1),
        float2(0.2, 0.2)
    };

    // Define some scroll speeds for the layers.
    float2 speeds[] = {
        float2(0.001, 0.0),
        float2(-0.002, 0.001),
        float2(0.003, -0.0015),
        float2(-0.005, 0.001)
    };

    float combinedCloudValue = 0.0;
    for (int i = 0; i < 4; i++)
    {
        float2 cloudUV = texCoord * scales[i] + speeds[i] * time;
        cloudUV = mod289(cloudUV); // Ensure coordinates wrap.
        float cloudValue = tex2D(CloudTexture, cloudUV).r;
        combinedCloudValue = max(combinedCloudValue, cloudValue); // Layer the values.
    }

    return combinedCloudValue;
}

float lensFlare(float2 texCoord, float bigStar)
{
    float2 dir = (texCoord - float2(0.5, 0.5)) * 2.0;
    float2 flarePos = texCoord - dir * bigStar * 0.5;
    float flare = snoise(flarePos * 100.0);
    return smoothstep(0.9, 0.98, flare);
}

float2 lightSourcePosition(float time, float speed, float amplitude)
{
    float x = 0.35 + amplitude * sin(time * speed);
    float y = 0.65 + amplitude * cos(time * speed);
    return float2(x, y);
}

float parallaxStarfield(float2 texCoord, float time)
{
    float t = time * 0.02;
    float2 offset = float2(cos(t), sin(t)) * 0.025;
    float2 starfieldCoords = (texCoord + offset);
    float star = snoise(starfieldCoords * 75.0);
    return smoothstep(0.85, 0.95, star);
}

float volumetricLight(float2 uv, float2 lightSource, float intensity)
{
    float d = distance(uv, lightSource);
    float illumination = smoothstep(1.0, 0.0, d) * intensity;

    return illumination;
}

float4 main(float2 texCoord : TEXCOORD0) : COLOR0
{
    float2 pos = texCoord.xy;
    float2 center = float2(0.5, 0.5);
    float4 color = float4(0, 0, 0, 1);

    // Modify rotation speed based on LinesCompleted
    float modifiedTime = Time + LinesCompleted * 0.2; // increase speed based on lines completed
    float rotationAngle = modifiedTime * 0.05;

    // Calculate the offset from the center of the texture coordinates
    float2 offset = pos - center;

    // Add some rotation to the offset based on time
    float2 rotatedOffset = float2(
        offset.x * cos(rotationAngle) - offset.y * sin(rotationAngle),
        offset.x * sin(rotationAngle) + offset.y * cos(rotationAngle)
    );

    // Add the rotated offset to the center to get the rotated texture coordinates
    float2 rotatedTexCoord = center + rotatedOffset;
    float3 volumetricLightColor = GalaxyColorThree;

    // Create a gradient of dark blue to dark purple
    color.rgb = smoothstep(0.0, 1.0, rotatedTexCoord.y) * GalaxyColorOne + smoothstep(
        0.7, 1.0, rotatedTexCoord.y) * GalaxyColorTwo;
    // Add some color variation at the bottom based on time
    color.r += smoothstep(0.3, 1.0, rotatedTexCoord.y) * sin((modifiedTime * 0.025) * 0.5) * 0.2;
    color.g += smoothstep(0.3, 1.0, rotatedTexCoord.y) * cos((modifiedTime * 0.035) * 0.7) * 0.2;
    color.b += smoothstep(0.3, 1.0, rotatedTexCoord.y) * sin((modifiedTime * 0.015) * 0.3) * 0.2;

    float neb = nebula(texCoord.xy, modifiedTime, NoiseTexture);
    color.rgb = lerp(color.rgb, NebulaColor, neb * 0.75);

    //float asteroidShadow = asteroidBelt(texCoord, Time);
    //color.rgb *= 1.0 - asteroidShadow * 0.5;

    float bigStar = bigStarGlow(texCoord.xy);
    color.rgb += smoothstep(0.9, 1.0, bigStar) * float3(1.0, 0.9, 0.7);

    float parallaxStarIntensity = parallaxStarfield(texCoord, Time);
    color.rgb += parallaxStarIntensity * float3(0.8, 0.8, 1.0);

    // Add some stars
    float4 starColor = float4(1.0, 1.0, 1.0, 1.0);
    color.rgb += starGlow(texCoord) * starColor.rgb;

    //float lensflare = lensFlare(texCoord.xy, bigStar);
    //color.rgb += smoothstep(0.9, 1.0, lensflare) * float3(1.0, 0.9, 0.7);

    float2 movingLightSource = lightSourcePosition(modifiedTime * 0.018, 1.0, 0.25);
    float volumetricLightIntensity = volumetricLight(texCoord, movingLightSource, 0.05);

    float3 volumetricLightEffect = volumetricLightIntensity * volumetricLightColor;
    color.rgb += volumetricLightEffect;

    float dust = dustCloud(texCoord.xy, Time);
    color.rgb = lerp(color.rgb, CloudColor, dust * 0.70);

    // Clamp the color values to prevent them from extending beyond 0 and 1
    color.rgb = clamp(color.rgb, 0.0, 1.0);

    return color;
}

technique RainbowTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 main();
    
    
    }
}
