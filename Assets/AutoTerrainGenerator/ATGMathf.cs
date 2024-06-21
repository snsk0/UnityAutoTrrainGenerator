using UnityEngine;

namespace AutoTerrainGenerator
{
    internal struct ATGMathf
    {
        internal const int ResolutionBase = 2;
        internal const int MinResolutionExp = 5;
        internal const int MaxResolutionExp = 12;
        internal const int ResolutionExpRange = MaxResolutionExp - MinResolutionExp + 1;

        internal const float MinTerrainHeight = 0f;
        internal const float MaxTerrainHeight = 1.0f;

        internal const float FBmPersistence = 0.5f;
        internal const float FBmFrequencyRate = 2.0f;

        internal static int GetResolution(int resolutionExp)
        {
            return (int)Mathf.Pow(ResolutionBase, resolutionExp) + 1;
        }

        internal static float Scaling(float value, float minValue, float maxValue, float toMinValue, float toMaxValue)
        {
            return toMinValue + ((value - minValue) / (maxValue - minValue)) * (toMaxValue - toMinValue);
        }

        internal static float SignedPerlinNoise(float x, float y)
        {
            return (Mathf.PerlinNoise(x, y) - 0.5f) * 2;
        }
    }
}
