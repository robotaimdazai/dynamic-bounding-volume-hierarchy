using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DBVHBase : MonoBehaviour
{
    public static Tree Tree = new();
    
}
public static class DBVHUtils
{
    public static bool AlmostEqual(Vector3 v1, Vector3 v2, Vector3 precision)
    {
        bool equal = true;
        if (Mathf.Abs (v1.x - v2.x) > precision.x) equal = false;
        if (Mathf.Abs (v1.y - v2.y) > precision.y) equal = false;
        if (Mathf.Abs (v1.z - v2.z) > precision.z) equal = false;
        return equal;
    }
}

