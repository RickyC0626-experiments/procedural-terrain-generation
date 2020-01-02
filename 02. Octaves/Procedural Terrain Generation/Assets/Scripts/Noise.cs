using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    // Octave: individual layer of noise
    // Persistence: controls the decrease in amplitude of each octave
    // Lacunarity: controls the increase in frequency of each octave
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset)
    {
        // Create a two-dimensional array within the bounds of the map
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Pseudo-Random Number Generator using the seed
        System.Random prng = new System.Random(seed);
        // Offsets to allow each octave to be sampled from a different location
        Vector2[] octaveOffsets = new Vector2[octaves];

        for(int i = 0; i < octaves; i++)
        {
            // Mathf.PerlinNoise(x, y) should not be given a coordinate that is too high otherwise it will just return the same value repeatedly
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Prevent division by 0
        if(scale <= 0) scale = 0.0001f;

        float maxNoiseHeight = float.MinValue;
        float minNoiseheight = float.MaxValue;

        // Scale from the middle of the map instead of the upper right corner
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        // For each point on the map, assign a perlin noise value that is the sum of all octaves
        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for(int i = 0; i < octaves; i++)
                {
                    // The higher the frequency, the further apart the sample points will be, which will cause height values to change more rapidly
                    float sampleX = (((x - halfWidth) / scale) * frequency) + octaveOffsets[i].x;
                    float sampleY = (((y - halfHeight) / scale) * frequency) + octaveOffsets[i].y;

                    // Perlin noise values are created between 0 and 1
                    // Multiplying that result by 2 and then subtracting 1 will create more interesting noise values between -1 and 1
                    // This will allow noiseHeight to decrease
                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    // Amplitude decreases each octave
                    amplitude *= persistence;
                    // Frequency increases each octave
                    frequency *= lacunarity;
                }

                // Find the maximum and minimum noiseHeight values
                if(noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if(noiseHeight < minNoiseheight) minNoiseheight = noiseHeight;

                // Assign noiseHeight value to the noiseMap
                noiseMap[x, y] = noiseHeight;
            }
        }

        // Iterate through all the noiseMap values again to normalize them
        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                // Returns a value between 0 and 1 within the range of min and max noiseHeight
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseheight, maxNoiseHeight, noiseMap[x, y]);
            }
        }
        return noiseMap;
    }
}
