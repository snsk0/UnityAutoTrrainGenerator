using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AutoTerrainGenerator.Parameters;

namespace AutoTerrainGenerator.HeightMapGenerators {
    internal class GeneratorFbm : HeightMapGeneratorBase
    {
        private const float PerlinNoiseFrequency = 256f;

        [SerializeField]
        private HeightMapGeneratorParam _param;

        public override float[,] Generate(INoiseReader noiseReader, int size)
        {
            Random.InitState(_param.seed);
            float xSeed = Random.Range(0f, PerlinNoiseFrequency);
            float ySeed = Random.Range(0f, PerlinNoiseFrequency);

            float[,] heightMap = new float[size, size];

            float frequency = _param.frequency;
            float amplitude = _param.amplitude;
            /*
            switch (data.generateType)
            {
                case GenerateType.fBm:
                    noiseFunc = Mathf.PerlinNoise;
                    break;
                case GenerateType.turbulence:
                    noiseFunc = (x, y) =>
                    {
                        float value = ATGMathf.SignedPerlinNoise(x, y);
                        return Mathf.Abs(value);
                    };
                    break;
                case GenerateType.ridge:
                    noiseFunc = (x, y) =>
                    {
                        float value = ATGMathf.SignedPerlinNoise(x, y);
                        value = Mathf.Abs(value);
                        value = ATGMathf.RidgeOffset - value;
                        return value *= value;
                    };
                    break;
            }*/
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
                        heightMap[x, y] += noiseReader.ReadNoise(xvalue, yvalue) * amplitude;
                    }
                }

                frequency *= ATGMathf.FBmFrequencyRate;
                amplitude *= ATGMathf.FBmPersistence;
            }

            //スケーリング
            if(_param.isLinearScaling)
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