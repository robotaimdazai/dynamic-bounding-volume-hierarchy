using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DBVHBase : MonoBehaviour
{
    private static List<DBVHBase> _allDbhvComponents = new List<DBVHBase>();
    public static List<DBVHBase>  AllComponents => _allDbhvComponents;

    public static Tree Tree = new();

    public virtual void OnEnable()
    {
        if(!_allDbhvComponents.Contains(this))
            _allDbhvComponents.Add(this);
        
    }
    public virtual void OnDisable()
    {
        if(_allDbhvComponents.Contains(this))
            _allDbhvComponents.Remove(this);
    }
    
    public static bool AlmostEqual(Vector3 v1, Vector3 v2, Vector3 precision)
    {
        bool equal = true;
        if (Mathf.Abs (v1.x - v2.x) > precision.x) equal = false;
        if (Mathf.Abs (v1.y - v2.y) > precision.y) equal = false;
        if (Mathf.Abs (v1.z - v2.z) > precision.z) equal = false;
        return equal;
    }
}

