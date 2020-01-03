using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    // Renderer of the plane we want to display our map on
    public Renderer textureRenderer;

    public void DrawTexture(Texture2D texture)
    {
        // Apply the texture to the texture renderer
        // Use sharedMaterial instead of material to generate the maps within the editor, since material is instantiated at runtime
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }
}
