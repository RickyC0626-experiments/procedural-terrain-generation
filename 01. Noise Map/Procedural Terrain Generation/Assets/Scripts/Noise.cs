using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
    {
        // Create a two-dimensional array within the bounds of the map
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Prevent division by 0
        if(scale <= 0) scale = 0.0001f;

        // For each point on the map, assign a perlin noise value between 0 and 1
        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float sampleX = x / scale;
                float sampleY = y / scale;

                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = perlinValue;
            }
        }
        return noiseMap;
    }
}
