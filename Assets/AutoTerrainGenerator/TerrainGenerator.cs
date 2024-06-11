using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AutoTerrainGenerator
{
    public static class TerrainGenerator
    {
        public static TerrainData Generate(Vector3 scale, int resolution)
        {
            TerrainData terrainData = new TerrainData();
            terrainData.size = scale;
            terrainData.heightmapResolution = resolution;

            float[,] heightMap = new float[resolution, resolution];

            for(int x = 0; x < resolution; x++)
            {
                for(int y = 0; y < resolution; y++)
                {
                    heightMap[x, y] = Mathf.PerlinNoise((float)x / resolution, (float)y / resolution);
                }
            }
            terrainData.SetHeights(0, 0, heightMap);

            Terrain.CreateTerrainGameObject(terrainData);

            return terrainData;
        }
    }
}