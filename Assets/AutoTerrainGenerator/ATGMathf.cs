using UnityEngine;

namespace AutoTerrainGenerator
{
    internal struct ATGMathf
    {
        internal const int ResolutionBase = 2;
        internal const int MinResolutionExp = 5;
        internal const int MaxResolutionExp = 12;

        internal const float MinTerrainHeight = 0f;
        internal const float MaxTerrainHeight = 1.0f;

        internal const float FBmPersistence = 0.5f;
        internal const float FBmFrequencyRate = 2.0f;

        internal const float RidgeOffset = 0.9f;

        internal static int GetResolution(int resolutionExp)
        {
            return (int)Mathf.Pow(ResolutionBase, resolutionExp) + 1;
        }

        internal static float LinearScaling(float value, float fromMinValue, float fromMaxValue, float toMinValue, float toMaxValue)
        {
            return toMinValue + ((value - fromMinValue) / (fromMaxValue - fromMinValue)) * (toMaxValue - toMinValue);
        }
    }
}
