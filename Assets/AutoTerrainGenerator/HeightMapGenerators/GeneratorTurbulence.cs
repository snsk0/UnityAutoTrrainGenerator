using UnityEngine;

namespace AutoTerrainGenerator.HeightMapGenerators
{
    public class GeneratorTurbulence : DefaultGeneratorBase
    {
        protected override float CalculateHeight(INoiseReader noiseReader, float currentAmplitude, float x, float y)
        {
            return Mathf.Abs(noiseReader.ReadSignedNoise(x, y)) * currentAmplitude;
        }
    }
}
