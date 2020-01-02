using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    // Renderer of the plane we want to display our map on
    public Renderer textureRenderer;

    public void DrawNoiseMap(float[,] noiseMap)
    {
        // Get the length of the first dimension array 
        int width = noiseMap.GetLength(0);
        // Get the length of the second dimension array
        int height = noiseMap.GetLength(1);

        // Create a new 2D texture
        Texture2D texture = new Texture2D(width, height);

        // Create an array to hold all colors on the map
        Color[] colorMap = new Color[width * height];

        // Iterate through each value in the noiseMap
        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                // Converting the index of two-dimensional array to one-dimensional
                // y * width will give us the row, and adding x will give us the column

                // Generate a color between black and white with the percentage
                colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, noiseMap[x, y]);
            }
        }
        // Apply the colors to the texture
        texture.SetPixels(colorMap);
        texture.Apply();

        // Apply the texture to the texture renderer
        // Use sharedMaterial instead of material to generate the maps within the editor, since material is instantiated at runtime
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(width, 1, height);
    }
}
