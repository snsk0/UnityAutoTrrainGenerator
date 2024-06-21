using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AutoTerrainGenerator.HeightMapGenerators {
    internal class GeneratorByUnityPerlin : IHeightMapGenerator
    {
        private const float PerlinNoiseFrequency = 256f;

        public float[,] Generate(int seed, int resolutionExp, float frequency, float amplitude, int octaves)
        {
            Random.InitState(seed);
            float xSeed = Random.Range(0f, PerlinNoiseFrequency);
            float ySeed = Random.Range(0f, PerlinNoiseFrequency);

            int resolution = ATGMathf.GetResolution(resolutionExp);

            float[,] heightMap = new float[resolution, resolution];

            for (int i = 0; i <= octaves; i++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    for (int y = 0; y < resolution; y++)
                    {
                        var xvalue = (float)x / resolution * frequency + xSeed;
                        var yvalue = (float)y / resolution * frequency + ySeed;
                        heightMap[x, y] += Mathf.PerlinNoise(xvalue, yvalue) * amplitude;
                    }
                }

                frequency *= ATGMathf.FBmFrequencyRate;
                amplitude *= ATGMathf.FBmPersistence;
            }

            return heightMap;
        }

        public float[,] Generate(int seed, int resolutionExp, float frequency, float minLinearScale, float maxLinearScale, int octaves)
        {
            float[,] heightMap = Generate(seed, resolutionExp, frequency, ATGMathf.MaxTerrainHeight, octaves);

            IEnumerable<float> heightEnum = heightMap.Cast<float>();
            float minHeight = heightEnum.Min();
            float maxHeight = heightEnum.Max();

            for (int x = 0; x < heightMap.GetLength(0); x++)
            {
                for (int y = 0; y < heightMap.GetLength(1); y++)
                {
                    heightMap[x, y] = ATGMathf.LinearScaling(heightMap[x,y], minHeight, maxHeight, minLinearScale, maxLinearScale);
                }
            }

            return heightMap;
        }
    }
}