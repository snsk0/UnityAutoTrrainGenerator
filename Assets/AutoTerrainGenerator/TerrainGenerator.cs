using UnityEngine;

namespace AutoTerrainGenerator
{
    public static class TerrainGenerator
    {
        public static TerrainData Generate(Vector3 scale, int resolutionEx, float noiseScale, int seed)
        {
            int resolution = (int)Mathf.Pow(2, resolutionEx) + 1;

            Random.InitState(seed);
            float xInitial = Random.Range(0f, 256f);
            float yInitial = Random.Range(0f, 256f);

            TerrainData terrainData = new TerrainData();

            //‰ğ‘œ“x‚ğÅ—Dæ‚Åİ’è‚·‚é
            terrainData.heightmapResolution = resolution;

            terrainData.size = scale;

            float[,] heightMap = new float[resolution, resolution];

            for(int x = 0; x < resolution; x++)
            {
                for(int y = 0; y < resolution; y++)
                {
                    heightMap[x, y] = Mathf.PerlinNoise(((float)x / resolution) * noiseScale + xInitial, ((float)y / resolution) * noiseScale + yInitial);
                }
            }
            terrainData.SetHeights(0, 0, heightMap);

            Terrain.CreateTerrainGameObject(terrainData);

            return terrainData;
        }
    }
}