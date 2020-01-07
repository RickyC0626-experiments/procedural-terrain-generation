using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MeshGenerator
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve _heightCurve, int levelOfDetail)
    {
        // Allows each thread to have its own heightCurve object, rather than accessing the same one and creating tall spikes
        AnimationCurve heightCurve = new AnimationCurve(_heightCurve.keys);

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

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1 : levelOfDetail * 2;
        /*
            w = 9
            i = 1   0   1   2   3   4   5   6   7   8       (9 - 1) / 1 + 1 = 8 + 1 = 9 vertices
            i = 2   0       2       4       6       8       (9 - 1) / 2 + 1 = 4 + 1 = 5 vertices
            i = 4   0               4               8       (9 - 1) / 4 + 1 = 2 + 1 = 3 vertices

            i = factor of (w - 1)
            i = 1, 2, 4, 8

            (w - 1) / i + 1
        */
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        // Create a new meshData for the heightMap
        MeshData meshData = new MeshData(width, height);

        int vertexIndex = 0;

        // Iterate through each coordinate on the heightMap based on level of detail
        for(int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {
                // Assign the height value to each vertex starting from the top left corner
                // topLeftX + x --> move rightward
                // topLeftZ - y --> move downward
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightCurve.Evaluate(heightMap[x, y]) * heightMultiplier, topLeftZ - y);
                // Let each vertex know where the uv is in relation to the rest of the map as a percentage
                meshData.uvs[vertexIndex] = new Vector2(x / (float) width, y / (float) height);

                // Right and bottom edges of the map do not have triangles to set
                if(x < width - 1 && y < height - 1)
                {
                    /*
                        i       i + 1 ...

                        i + w   i + w + 1 ...

                        Triangle 1: i,          i + w + 1,  i + w
                        Triangle 2: i + w + 1,  i,          i + 1
                    */
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
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
