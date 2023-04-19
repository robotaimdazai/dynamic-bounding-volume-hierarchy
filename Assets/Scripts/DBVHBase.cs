using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DBVHBase : MonoBehaviour
{
    private static List<DBVHBase> _allDbhvComponents = new List<DBVHBase>();
    public static List<DBVHBase>  AllComponents => _allDbhvComponents;

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
}

