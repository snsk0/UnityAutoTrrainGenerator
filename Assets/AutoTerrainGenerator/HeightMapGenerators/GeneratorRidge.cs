using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AutoTerrainGenerator.Parameters;

namespace AutoTerrainGenerator.HeightMapGenerators
{
    public class GeneratorRidge : HeightMapGeneratorBase
    {
        [SerializeField]
        private HeightMapGeneratorParam _param;

        [SerializeField]
        private HeightMapGeneratorBase _inputParam;

        public override float[,] Generate(INoiseReader noiseReader, int size)
        {
            Random.InitState(_param.seed);
            float xSeed = Random.Range(0f, noiseReader.GetNoiseFrequency());
            float ySeed = Random.Range(0f, noiseReader.GetNoiseFrequency());

            float[,] heightMap = new float[size, size];

            float frequency = _param.frequency;
            float amplitude = _param.amplitude;

            if (_param.isLinearScaling)
            {
                amplitude = ATGMathf.MaxTerrainHeight;
            }

            for (int i = 0; i <= _param.octaves; i++)
            {
                for (int x = 0; x < size; x++)
                {
                    for (int y = 0; y < size; y++)
                    {
                        var xvalue = (float)x / size * frequency + xSeed;
                        var yvalue = (float)y / size * frequency + ySeed;
                        float value = Mathf.Abs(noiseReader.ReadSignedNoise(xvalue, yvalue)) * amplitude;
                        value = ATGMathf.RidgeOffset - value;
                        heightMap[x, y] += value * value;
                    }
                }

                frequency *= ATGMathf.FBmFrequencyRate;
                amplitude *= ATGMathf.FBmPersistence;
            }

            //スケーリング
            if (_param.isLinearScaling)
            {
                IEnumerable<float> heightEnum = heightMap.Cast<float>();
                float minHeight = heightEnum.Min();
                float maxHeight = heightEnum.Max();

                for (int x = 0; x < heightMap.GetLength(0); x++)
                {
                    for (int y = 0; y < heightMap.GetLength(1); y++)
                    {
                        heightMap[x, y] = ATGMathf.LinearScaling(heightMap[x, y], minHeight, maxHeight, _param.minLinearScale, _param.maxLinearScale);
                    }
                }
            }
            return heightMap;
        }
    }
}
