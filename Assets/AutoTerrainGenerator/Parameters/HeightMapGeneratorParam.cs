using UnityEngine;

namespace AutoTerrainGenerator.Parameters
{
    internal class HeightMapGeneratorParam : ScriptableObject
    {
        //ƒmƒCƒY•Ï”
        public int noiseTypeIndex = 0;
        public int seed = 0;
        public float frequency = 0;
        public bool isLinearScaling = false;
        public float amplitude = ATGMathf.MaxTerrainHeight;
        public float minLinearScale = (ATGMathf.MinTerrainHeight + ATGMathf.MaxTerrainHeight) / 2;
        public float maxLinearScale = (ATGMathf.MinTerrainHeight + ATGMathf.MaxTerrainHeight) / 2;
        public int octaves = 0;
    }
}
