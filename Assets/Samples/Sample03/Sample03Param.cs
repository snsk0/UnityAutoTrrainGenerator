using System;
using UnityEngine;

[Serializable]
public class Sample03Param
{
    [SerializeField] private float _amplitude;
    [SerializeField] private float _frequency;
    [SerializeField] private float _loadThreshold;

    public float amplitude => _amplitude;
    public float frequency => _frequency;
    public float loadThreshold => _loadThreshold;
}
