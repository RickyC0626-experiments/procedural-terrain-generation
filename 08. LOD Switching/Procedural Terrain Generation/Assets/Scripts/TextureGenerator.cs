using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        // Create a new 2D texture
        Texture2D texture = new Texture2D(width, height);

        // Fix blurriness of the texture
        texture.filterMode = FilterMode.Point;

        // Prevent texture from wrapping
        texture.wrapMode = TextureWrapMode.Clamp;

        // Apply the colors to the texture
        texture.SetPixels(colorMap);
        texture.Apply();

        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
        // Get the length of the first dimension array 
        int width = heightMap.GetLength(0);
        // Get the length of the second dimension array
        int height = heightMap.GetLength(1);

        // Create an array to hold all colors on the map
        Color[] colorMap = new Color[width * height];

        // Iterate through each value in the heightMap
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                // Converting the index of two-dimensional array to one-dimensional
                // y * width will give us the row, and adding x will give us the column

                // Generate a color between black and white with the percentage
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
            }
        }
        return TextureFromColorMap(colorMap, width, height);
    }
}
