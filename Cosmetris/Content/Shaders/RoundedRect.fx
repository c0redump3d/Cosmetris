/*
 * RoundedRect.fx is part of Cosmetris.
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
float4 Color;
float4 OutlineColor;
float2 Size;
float Radius;
float OutlineThickness;

struct PixelInput
{
    float4 Position : SV_POSITION;
    float2 UV : TEXCOORD0;
};

// Signed Distance Field function
// Returns how far a pixel is from the edge of the rounded rectangle shape
float roundedRectSDF(float2 pixelPos, float2 size, float radius)
{
    return length(max(abs(pixelPos) - size / 2.0f + radius, 0.0f)) - radius;
}

// Function to generate a pseudo-random value between 0 and 1, based on a 2D position
float noise(float2 position)
{
    float2 i = floor(position);
    float2 f = frac(position);

    float a = dot(i, float2(12.9898, 78.233));
    float b = dot(i + float2(1.0, 0.0), float2(12.9898, 78.233));
    float c = dot(i + float2(0.0, 1.0), float2(12.9898, 78.233));
    float d = dot(i + float2(1.0, 1.0), float2(12.9898, 78.233));

    a = frac(sin(a) * 43758.5453);
    b = frac(sin(b) * 43758.5453);
    c = frac(sin(c) * 43758.5453);
    d = frac(sin(d) * 43758.5453);

    float u = f.x * f.x * (3.0 - 2.0 * f.x);
    float v = f.y * f.y * (3.0 - 2.0 * f.y);

    return lerp(a, b, u) + (c - a) * v * (1.0 - u) + (d - b) * u * v;
}

// Pixel shader
float4 PS_RoundedRect(PixelInput input) : SV_TARGET
{
    // Convert our UV position (that goes from 0 - 1) to pixel positions relative to the rectangle
    float2 pixelPos = float2(input.UV.x * Size.x, input.UV.y * Size.y);

    // Calculate distance to edge
    float distance = roundedRectSDF(pixelPos - (Size / 2.0f), Size, Radius);

    // Determine if the pixel is within the outline band
    float outlineDistance = distance + OutlineThickness;
    bool isInOutline = outlineDistance > 0.0f && outlineDistance < OutlineThickness;

    // Determine if the pixel is inside the rectangle
    bool isInsideRectangle = distance <= 0.0f;

    // Base color for the pixel, defaulting to transparent
    float4 color = float4(0.0f, 0.0f, 0.0f, 0.0f);

    // Apply color and noise texture if inside the rectangle or the outline
    if (isInsideRectangle)
    {
        // Generate noise based on the pixel position
        float noiseValue = noise(pixelPos / Size);

        // Scale the noise value so it's subtle
        float noiseScale = 0.1f; // Adjust this as needed to make the noise more or less subtle
        noiseValue *= noiseScale;

        // Create a color from the noise value
        float4 noiseColor = float4(noiseValue, noiseValue, noiseValue, 0.0f);

        // Set the base color to the appropriate color, adding the noise color
        color = isInOutline ? OutlineColor : Color + noiseColor * Color.a;
    }

    return color;
}

technique RoundedRectangle
{
    pass Pass1
    {
        PixelShader = compile ps_3_0 PS_RoundedRect();
    }
};
