using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Master : MonoBehaviour
{
    private static Master instance;
    public static Master Instance { get { return instance; } }
    private CameraPOI cameraPOI;
    public CameraPOI Camera
    { 
        get { return cameraPOI; } 
        set
        {
            if (value.Equals(typeof(CameraPOI)))
            {
                cameraPOI = value;
            }
        }
    }

    private void Awake()
    {
        instance = this;
    }
}
