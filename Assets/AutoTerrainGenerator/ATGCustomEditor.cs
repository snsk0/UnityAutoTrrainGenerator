using System;
using UnityEngine;

namespace AutoTerrainGenerator
{
    public class ATGCustomEditor : Attribute
    {
        private Type _inspectedType;
        internal Type inspectedType => _inspectedType;

        public ATGCustomEditor(Type inspectedType)
        {
            if (inspectedType == null)
            {
                Debug.LogError("Failed to load ATGCustomEditor inspected type");
            }

            _inspectedType = inspectedType;
        }
    }
}
