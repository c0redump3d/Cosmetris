/*
 * Glow.fx is part of Cosmetris.
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
texture SpriteTexture;
float4 GlowColor;
float GlowRadius;
float2 Resolution;
float2 Padding; // The amount of padding on each side of the texture, in pixels

sampler2D SpriteSampler = sampler_state
{
    Texture = <SpriteTexture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
};

float Time;

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

    // Add glistening effect
    glowStrength *= (1.0 + sin(Time * 2.0 * 3.14159) * 0.1);

    color.rgb += GlowColor.rgb * glowStrength * GlowColor.a;
    return color;
}

technique GlowTechnique
{
    pass P0
    {
        PixelShader = compile ps_3_0 main();
    }
}
