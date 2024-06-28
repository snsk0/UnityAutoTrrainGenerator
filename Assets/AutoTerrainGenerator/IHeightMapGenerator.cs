using AutoTerrainGenerator.Parameters;

namespace AutoTerrainGenerator
{
    internal interface IHeightMapGenerator
    {
        float[,] Generate(HeightMapGeneratorParam data);
    }
}

