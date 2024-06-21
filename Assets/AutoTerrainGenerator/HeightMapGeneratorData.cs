using UnityEngine;

internal class HeightMapGeneratorData : ScriptableObject
{
    //ノイズ変数
    public int noiseTypeIndex;
    public int seed;
    public float frequency;
    public bool isLinearScaling;
    public float amplitude;
    public float minLinearScale;
    public float maxLinearScale;
    public int octaves;

    //ハイトマップ
    public int resolutionExp;

    //テレイン
    public Vector3 scale;
}
