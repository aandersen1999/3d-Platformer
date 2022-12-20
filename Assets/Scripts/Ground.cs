using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ground
{
    public Vector3 normal;
    public GroundType type;
    public Ground()
    {

    }
}

public enum GroundType
{
    Standard,
}