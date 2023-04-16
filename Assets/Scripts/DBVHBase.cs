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

public struct  AABB
{
    public Vector3 Min;
    public Vector3 Max;

    public AABB Union(AABB a, AABB b)
    {
        AABB c;
        c.Min = Vector3.Min(a.Min, b.Min);
        c.Max = Vector3.Max(a.Max, b.Max);
        return c;
    }

    public float Area(AABB a)
    {
        Vector3 d = a.Max - a.Min;
        return 2.0f * (d.x * d.y + d.y * d.z + d.z * d.x);

    }
    
    public  bool Intersects(Ray ray)
    {
        float tmin = float.MinValue;
        float tmax = float.MaxValue;

        for (int i = 0; i < 3; i++)
        {
            if (Math.Abs(ray.direction[i]) < float.Epsilon)
            {
                if (ray.origin[i] < Min[i] || ray.origin[i] > Max[i])
                {
                    return false;
                }
            }
            else
            {
                float t1 = (Min[i] - ray.origin[i]) / ray.direction[i];
                float t2 = (Max[i] - ray.origin[i]) / ray.direction[i];

                tmin = Math.Max(tmin, Math.Min(t1, t2));
                tmax = Math.Min(tmax, Math.Max(t1, t2));

                if (tmax < 0 || tmin > tmax)
                {
                    return false;
                }
            }
        }

        return true;
    }
}

public struct Node
{
    public AABB Box;
    public int ObjectIndex;
    public int ParentIndex;
    public int Child1;
    public int Child2;
    public bool IsLeaf;
}

public struct Tree
{
    public Node Nodes;
    public int NodeCount;
    public int RootIndex;
}
