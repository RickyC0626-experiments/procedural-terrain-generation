using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public LevelOfDetailInfo[] detailLevels;
    public static float maxViewDistance;

    public Transform viewer;
    public Material mapMaterial;

    public static Vector2 viewerPosition;
    Vector2 viewerPositionOld;
    static MapGenerator mapGenerator;
    int chunkSize;
    int visibleChunksInViewDist;

    // Coordinates, corresponding terrain chunk
    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();

        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistThreshold;

        // Dimensions of the actual mesh will be 1 less than mapChunkSize (241)
        chunkSize = MapGenerator.mapChunkSize - 1;
        visibleChunksInViewDist = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();

    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

        if((viewerPositionOld - viewerPosition).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }
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
                else terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, detailLevels, transform, mapMaterial));
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;

        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LevelOfDetailInfo[] detailLevels;
        LevelOfDetailMesh[] levelOfDetailMeshes;

        MapData mapData;
        bool mapDataReceived;
        int previousLODIndex = -1;

        public TerrainChunk(Vector2 coord, int size, LevelOfDetailInfo[] detailLevels, Transform parent, Material material)
        {
            this.detailLevels = detailLevels;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionVector3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionVector3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            levelOfDetailMeshes = new LevelOfDetailMesh[detailLevels.Length];

            for(int i = 0; i < detailLevels.Length; i++)
            {
                levelOfDetailMeshes[i] = new LevelOfDetailMesh(detailLevels[i].levelOfDetail, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colorMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
        }

        // Find the point on the chunk's perimeter that is closest to the viewer's position and find the distance between that point and the viewer
        // If the distance is less than maxViewDistance, then meshObject is enabled
        // If the distance exceeds maxViewDistance, then meshObject is disabled
        public void UpdateTerrainChunk()
        {
            if(!mapDataReceived) return;

            float viewerDistFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistFromNearestEdge <= maxViewDistance;

            if(visible)
            {
                int lodIndex = 0;

                for(int i = 0; i < detailLevels.Length - 1; i++)
                {
                    if(viewerDistFromNearestEdge > detailLevels[i].visibleDistThreshold) lodIndex = i + 1;
                    else break;
                }

                if(lodIndex != previousLODIndex)
                {
                    LevelOfDetailMesh lodMesh = levelOfDetailMeshes[lodIndex];

                    if(lodMesh.hasMesh)
                    {
                        previousLODIndex = lodIndex;
                        meshFilter.mesh = lodMesh.mesh;
                    } 
                    else if(!lodMesh.hasRequestedMesh) lodMesh.RequestMesh(mapData);
                }
            }
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

    // Responsible for fetching its own mesh from MapGenerator
    class LevelOfDetailMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int levelOfDetail;
        System.Action updateCallback;

        public LevelOfDetailMesh(int levelOfDetail, System.Action updateCallback)
        {
            this.levelOfDetail = levelOfDetail;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestedMesh = true;
            mapGenerator.RequestMeshData(mapData, levelOfDetail, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LevelOfDetailInfo
    {
        public int levelOfDetail;
        // When viewer is outside of threshold, then lower level of detail is shown
        public float visibleDistThreshold;
    }
}
