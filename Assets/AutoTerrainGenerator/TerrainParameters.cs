using System;
using UnityEngine;

namespace AutoTerrainGenerator
{
    [Serializable]
    public class TerrainParameters
    {
        public int resolutionExp = (ATGMathf.MinResolutionExp + ATGMathf.MaxResolutionExp) / 2;
        public Vector3 scale = new Vector3(1000, 600, 1000);

        public int resolution => ATGMathf.GetResolution(resolutionExp);
    }
}
