namespace AutoTerrainGenerator
{
    internal interface IHeightMapGenerator
    {
        float[,] Generate(HeightMapGeneratorData data);
    }
}

