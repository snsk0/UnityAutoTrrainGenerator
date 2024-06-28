using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AutoTerrainGenerator.Parameters;

namespace AutoTerrainGenerator.HeightMapGenerators {
    internal class GeneratorByUnityPerlin : HeightMapGeneratorBase
    {
        private const float PerlinNoiseFrequency = 256f;

        [SerializeField]
        private HeightMapGeneratorParam _param;

        private void Awake()
        {
            _param = CreateInstance<HeightMapGeneratorParam>();
        }

        public override float[,] Generate()
        {
            Random.InitState(_param.seed);
            float xSeed = Random.Range(0f, PerlinNoiseFrequency);
            float ySeed = Random.Range(0f, PerlinNoiseFrequency);

            int resolution = ATGMathf.GetResolution(_param.resolutionExp);

            float[,] heightMap = new float[resolution, resolution];

            float frequency = _param.frequency;
            float amplitude = _param.amplitude;

            System.Func<float, float, float> noiseFunc = null;
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
            noiseFunc = Mathf.PerlinNoise;

            if (_param.isLinearScaling)
            {
                amplitude = ATGMathf.MaxTerrainHeight;
            }

            for (int i = 0; i <= _param.octaves; i++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    for (int y = 0; y < resolution; y++)
                    {
                        var xvalue = (float)x / resolution * frequency + xSeed;
                        var yvalue = (float)y / resolution * frequency + ySeed;
                        heightMap[x, y] += noiseFunc.Invoke(xvalue, yvalue) * amplitude;
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