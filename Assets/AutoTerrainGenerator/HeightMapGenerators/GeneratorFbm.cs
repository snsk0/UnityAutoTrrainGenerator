namespace AutoTerrainGenerator.HeightMapGenerators 
{
    public class GeneratorFbm : DefaultGeneratorBase
    {
        protected override float CalculateHeight(INoiseReader noiseReader, float currentAmplitude, float xvalue, float yvalue)
        {
            return noiseReader.ReadNoise(xvalue, yvalue) * currentAmplitude;
        }
    }
}