using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap)
    {
        // Get the length of the first dimension array 
        int width = heightMap.GetLength(0);
        // Get the length of the second dimension array 
        int height = heightMap.GetLength(1);

        /*
            x   x   x
            -1  0   1

            Left most point: x = (w - 1) / -2 = -1
            Top most point: z = (h - 1) / 2 = 1

            z 1
            z 0
            z -1
        */
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        // Create a new meshData for the heightMap
        MeshData meshData = new MeshData(width, height);

        int vertexIndex = 0;

        // Iterate through each coordinate on the heightMap
        for(int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Assign the height value to each vertex starting from the top left corner
                // topLeftX + x --> move rightward
                // topLeftZ - y --> move downward
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightMap[x, y], topLeftZ - y);
                // Let each vertex know where the uv is in relation to the rest of the map as a percentage
                meshData.uvs[vertexIndex] = new Vector2(x / (float) width, y / (float) height);

                /*
                    i       i + 1 ...

                    i + w   i + w + 1 ...

                    Triangle 1: i,          i + w + 1,  i + w
                    Triangle 2: i + w + 1,  i,          i + 1
                */
                // Right and bottom edges of the map do not have triangles to set
                if(x < width - 1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
                    meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
                }
                vertexIndex++;
            }
        }
        // Return the meshData instead of mesh to allow implementation of threading so that the game doesn't freeze
        // when different chunks of the mesh are generated
        return meshData;
    }
}

// A mesh consists of triangles arranged in a 3D space to create the impression of a solid object
public class MeshData
{
    // Contains the positions of vertices
    public Vector3[] vertices;
    // Contains indices to the vertex array
    public int[] triangles;
    // UV: texture coordinates of the mesh
    public Vector2[] uvs;

    int triangleIndex;

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight];
        uvs = new Vector2[meshWidth * meshHeight];
        // (width - 1)(height - 1) gives number of squares
        // Each square consists of 2 triangles with 3 vertices each
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex + 1] = b;
        triangles[triangleIndex + 2] = c;
        triangleIndex += 3;
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        // Fix the lighting for a smoother transition
        mesh.RecalculateNormals();
        return mesh;
    }
}
