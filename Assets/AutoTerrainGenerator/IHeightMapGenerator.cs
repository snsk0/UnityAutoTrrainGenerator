namespace AutoTerrainGenerator
{
    internal interface IHeightMapGenerator
    {
        float[,] Generate(int seed, int resolutionExp, float frequency, float amplitude, int octaves);
        float[,] Generate(int seed, int resolutionExp, float frequency, float minLinearScale, float maxLinearScale, int octaves);
    }
}

