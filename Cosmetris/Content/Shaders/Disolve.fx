/*
 * Dissolve.fx is part of Cosmetris.
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
sampler TextureSampler : register(s0);
sampler DissolveMask : register(s1);

float Threshold;
float WaveAmplitude = 0.25; // Amplitude of the wave (how "high" or "low" the wave goes)
float WaveFrequency = 10.0; // Frequency of the wave (how many waves across the texture)

float4 PixelShaderFunction(float2 textureCoordinate : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(TextureSampler, textureCoordinate);
    float dissolveValue = tex2D(DissolveMask, textureCoordinate).r;

    // Adjust the threshold using a sine wave to create a wavy effect
    float wave = WaveAmplitude * sin(textureCoordinate.x * WaveFrequency * 2.0f * 3.14159265f);
    float adjustedThreshold = Threshold + wave;

    if (dissolveValue > adjustedThreshold)
    {
        color.a = 0.0f;
    }

    return color;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PixelShaderFunction();
    
    
    }
}
