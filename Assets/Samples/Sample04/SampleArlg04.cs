using UnityEngine;

namespace AutoTerrainGenerator.Sample
{
    public class SampleArlg04 : HeightMapGeneratorBase
    {
        [SerializeField] public float _amplitude;
        [SerializeField] public float _frequency;
        [SerializeField] public float _loadThreshold;

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
                    float xvalue = (float)x / size * _frequency + xSeed;
                    float yvalue = (float)y / size * _frequency + ySeed;
                    heightMap[x, y] = noiseReader.ReadNoise(xvalue, yvalue) * _amplitude;

                    if (heightMap[x, y] < _loadThreshold)
                    {
                        heightMap[x, y] = _loadThreshold;
                    }
                }
            }
            return heightMap;
        }
    }
}
