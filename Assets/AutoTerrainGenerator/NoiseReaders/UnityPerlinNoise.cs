using UnityEngine;

namespace AutoTerrainGenerator.NoiseReaders
{
    internal class UnityPerlinNoise : INoiseReader
    {
        public float ReadNoise(float x, float y)
        {
            return Mathf.PerlinNoise(x, y);
        }
    }
}
