using UnityEngine;

namespace AutoTerrainGenerator.NoiseReaders
{
    public class UnitySignedPerlinNoise : INoiseReader
    {
        public float ReadNoise(float x, float y)
        {
            return (Mathf.PerlinNoise(x, y) - 0.5f) * 2.0f;
        }
    }
}
