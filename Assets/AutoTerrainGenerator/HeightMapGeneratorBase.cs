using UnityEngine;

namespace AutoTerrainGenerator
{
    public abstract class HeightMapGeneratorBase : ScriptableObject
    {
        public abstract float[,] Generate(INoiseReader noiseReader, int size);
    }
}

