using UnityEngine;

namespace AutoTerrainGenerator.HeightMapGenerators
{
    public class GeneratorRidge : DefaultGeneratorBase
    {
        protected override float CalculateHeight(INoiseReader noiseReader, float currentAmplitude, float x, float y)
        {
            float height = Mathf.Abs(noiseReader.ReadSignedNoise(x, y)) * currentAmplitude;
            height = ATGMathf.RidgeOffset - height;
            height *= height;
            return height;
        }
    }
}
