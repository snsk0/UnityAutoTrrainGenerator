using UnityEngine;

namespace AutoTerrainGenerator.Sample
{
    public class SampleArlg03 : HeightMapGeneratorBase
    {
        [SerializeField] private Sample03Param param;

        public override float[,] Generate(INoiseReader noiseReader, int size)
        {
            Random.InitState((int)Time.time);
            float xSeed = Random.Range(0f, noiseReader.GetNoiseFrequency());
            float ySeed = Random.Range(0f, noiseReader.GetNoiseFrequency());

            float[,] heightMap = new float[size, size];

            for(int x = 0; x < size; x++)
            {
                for(int y = 0; y < size; y++)
                {
                    float xvalue = (float)x / size * param.frequency + xSeed;
                    float yvalue = (float)y / size * param.frequency + ySeed;
                    heightMap[x, y] = noiseReader.ReadNoise(xvalue, yvalue) * param.amplitude;

                    if (heightMap[x, y] < param.loadThreshold)
                    {
                        heightMap[x, y] = param.loadThreshold;
                    }
                }
            }
            return heightMap;
        }
    }
}
