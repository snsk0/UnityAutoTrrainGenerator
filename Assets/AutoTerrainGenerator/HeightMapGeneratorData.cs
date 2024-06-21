using AutoTerrainGenerator;
using UnityEngine;

internal class HeightMapGeneratorData : ScriptableObject
{
    //ノイズ変数
    public int noiseTypeIndex = 0;
    public int seed = 0;
    public float frequency = 0;
    public bool isLinearScaling = false;
    public float amplitude = ATGMathf.MaxTerrainHeight;
    public float minLinearScale = (ATGMathf.MinTerrainHeight + ATGMathf.MaxTerrainHeight) / 2;
    public float maxLinearScale = (ATGMathf.MinTerrainHeight + ATGMathf.MaxTerrainHeight) / 2;
    public int octaves = 0;

    //ハイトマップ
    public int resolutionExp = (ATGMathf.MinResolutionExp + ATGMathf.MaxResolutionExp) / 2;

    //テレイン
    public Vector3 scale = new Vector3(1000, 600, 1000);
}
