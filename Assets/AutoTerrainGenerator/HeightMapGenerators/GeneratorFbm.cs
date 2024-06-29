using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using AutoTerrainGenerator.Parameters;

namespace AutoTerrainGenerator.HeightMapGenerators 
{
    public class GeneratorFbm : HeightMapGeneratorBase
    {
        [SerializeField]
        private HeightMapGeneratorParam _param;

        [SerializeField]
        private HeightMapGeneratorParam _inputParam;

        public override float[,] Generate(INoiseReader noiseReader, int size)
        {
            //入力値がある場合はそちらを使用する
            HeightMapGeneratorParam param = _param;
            if (_inputParam != null)
            {
                param = _inputParam;
            }

            Random.InitState(param.seed);
            float xSeed = Random.Range(0f, noiseReader.GetNoiseFrequency());
            float ySeed = Random.Range(0f, noiseReader.GetNoiseFrequency());

            float[,] heightMap = new float[size, size];

            float frequency = param.frequency;
            float amplitude = param.amplitude;

            if (param.isLinearScaling)
            {
                amplitude = ATGMathf.MaxTerrainHeight;
            }

            for (int i = 0; i <= param.octaves; i++)
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
            if(param.isLinearScaling)
            {
                IEnumerable<float> heightEnum = heightMap.Cast<float>();
                float minHeight = heightEnum.Min();
                float maxHeight = heightEnum.Max();

                for (int x = 0; x < heightMap.GetLength(0); x++)
                {
                    for (int y = 0; y < heightMap.GetLength(1); y++)
                    {
                        heightMap[x, y] = ATGMathf.LinearScaling(heightMap[x, y], minHeight, maxHeight, param.minLinearScale, param.maxLinearScale);
                    }
                }
            }

            return heightMap;
        }
    }
}