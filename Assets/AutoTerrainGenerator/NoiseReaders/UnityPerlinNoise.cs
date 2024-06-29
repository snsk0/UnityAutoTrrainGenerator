using UnityEngine;

namespace AutoTerrainGenerator.NoiseReaders
{
    public class UnityPerlinNoise : INoiseReader
    {
        public float GetNoiseFrequency()
        {
            return 256f;
        }

        public float ReadNoise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }

        public float ReadSignedNoise(float x, float y)
        {
            return (Mathf.PerlinNoise(x, y) - 0.5f) * 2.0f;
        }
    }
}
