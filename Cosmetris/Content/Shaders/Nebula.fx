/*
 * Nebula.fx is part of Cosmetris.
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
sampler2D TextureSampler = sampler_state
{
    Texture = <SpriteTexture>;
};

struct VertexShaderInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoords: TEXCOORD0; // added
};

struct VertexShaderOutput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR0;
    float2 TexCoords: TEXCOORD0; // added
}; 

float Time;
float2 Resolution;
float fbm(float2 position, float time)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 0.0;
    for (int i = 0; i < 12; ++i)
    {
        value += amplitude * snoise(position * frequency + time * 0.001);
        position *= 2.0;
        amplitude *= 0.5;
        frequency += 0.02;
    }
    return value;
}

float fbm2(float2 position, float time)
{
    float value = 0.0;
    float amplitude = 0.5;
    float frequency = 0.0;
    for (int i = 0; i < 12; ++i)
    {
        value += amplitude * snoise(position * frequency + time * 0.005);
        position *= 1.75;
        amplitude *= 0.55;
        frequency += 0.015;
    }
    return value;
}



float2 domainWarp(float2 position, float time)
{
    float warp_intensity = 10.0;
    float2 warp_offset = snoise(position * 0.1 + time * 0.001) * float2(warp_intensity, warp_intensity);
    return position + warp_offset;
}

float nebula1(float2 texCoord, float time)
{
    float scale = 2.2;
    float2 adjustedTexCoord = texCoord * 3.0 + float2(0.5, 0.5);
    float2 warpedTexCoord = domainWarp(adjustedTexCoord, time);

    float neb_intensity1 = fbm(warpedTexCoord * scale, time) * 0.5 + 0.5;
    float neb_intensity2 = fbm(warpedTexCoord * scale * 2.0, time) * 0.5 + 0.5;
    float neb_intensity = lerp(neb_intensity1, neb_intensity2, 0.5);

    float blend_factor = fbm2(warpedTexCoord * 0.75, time) * 0.5 + 0.5;
    float blended_intensity = mix(neb_intensity, blend_factor, 0.1);

    return smoothstep(0.25, 0.65, blended_intensity);
}

float4 main(float2 texCoord : TEXCOORD0) : COLOR0
{
    float2 pos = texCoord.xy;
    float4 color = float4(0, 0, 0, 1);

    // Create a gradient of dark blue to dark purple
    color.rgb = smoothstep(0.0, 1.0, pos.y) * float3(0.05, 0.01, 0.15) + smoothstep(0.7, 1.0, pos.y) * float3(0.1, 0.05, 0.3);
    
    float neb = nebula1(texCoord.xy, Time);
    color.rgb = lerp(color.rgb, float3(0.4, 0.2, 0.6), neb * 0.5);

    return color;
}

technique NebulaEffect
{
    pass P0
    {
        PixelShader = compile ps_3_0 main();
    }
}