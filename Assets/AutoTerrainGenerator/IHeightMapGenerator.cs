namespace AutoTerrainGenerator
{
    internal interface IHeightMapGenerator
    {
        float[,] Generate(int seed, int resolutionExp, float frequency, float minAmplitude, float maxAmplitude, int octaves);
    }
}

