namespace AutoTerrainGenerator
{
    public interface INoiseReader
    {
        float ReadNoise(float x, float y);
        float ReadSignedNoise(float x, float y);
        float GetNoiseFrequency();
    }
}
