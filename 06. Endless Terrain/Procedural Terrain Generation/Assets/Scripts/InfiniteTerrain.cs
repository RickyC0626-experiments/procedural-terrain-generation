using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public const float maxViewDistance = 450;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunkSize;
    int visibleChunksInViewDist;

    // Coordinates, corresponding terrain chunk
    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        // Dimensions of the actual mesh will be 1 less than mapChunkSize (241)
        chunkSize = MapGenerator.mapChunkSize - 1;
        visibleChunksInViewDist = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        // Clear chunks from last update to prevent chunks beyond maxViewDistance to remain loaded
        for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();

        // Get the coordinate of the chunk the viewer is standing on
        // Current chunk coord (0, 0)
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        // 240 vertices to 1 chunk coord
        // Chunk to the right of current chunk: (240, 0) --> (1, 0)
        for(int yOffset = -visibleChunksInViewDist; yOffset <= visibleChunksInViewDist; yOffset++)
        {
            for (int xOffset = -visibleChunksInViewDist; xOffset <= visibleChunksInViewDist; xOffset++)
            {
                // Only instantiate a new terrain chunk at this coordinate if one is not created already
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                // Maintain a dictionary of all the coordinates and terrain chunks to prevent duplicates
                if(terrainChunkDict.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();

                    if(terrainChunkDict[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDict[viewedChunkCoord]);
                    }
                }
                else terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionVector3 = new Vector3(position.x, 0, position.y);

            // Instantiate a plane as visual placeholder
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionVector3;
            meshObject.transform.localScale = (Vector3.one * size) / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false);
        }

        // Find the point on the chunk's perimeter that is closest to the viewer's position and find the distance between that point and the viewer
        // If the distance is less than maxViewDistance, then meshObject is enabled
        // If the distance exceeds maxViewDistance, then meshObject is disabled
        public void UpdateTerrainChunk()
        {
            float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}
