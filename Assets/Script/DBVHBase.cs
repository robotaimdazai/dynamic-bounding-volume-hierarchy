using System;
using System.Collections;
using System.Collections.Generic;
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
    public Vector3 LowerBound;
    public Vector3 UpperBound;

    public AABB Union(AABB a, AABB b)
    {
        AABB c;
        c.LowerBound = Vector3.Min(a.LowerBound, b.LowerBound);
        c.UpperBound = Vector3.Max(a.UpperBound, b.UpperBound);
        return c;
    }

    public float Area(AABB a)
    {
        Vector3 d = a.UpperBound - a.LowerBound;
        return 2.0f * (d.x * d.y + d.y * d.z + d.z * d.x);

    }
    
    public bool TestOverlap(AABB aabb,Vector3 start, Vector3 end)
    {
        Vector3 dir = (end - start).normalized;
        Vector3 dirFrac;
        dirFrac.x = 1f / dir.x;
        dirFrac.y = 1f / dir.y;
        dirFrac.z = 1f / dir.z;
        var lb = aabb.LowerBound;
        var rt = aabb.UpperBound;
        float t1 = (lb.x - start.x)*dirFrac.x;
        float t2 = (rt.x - start.x)*dirFrac.x;
        float t3 = (lb.y - start.y)*dirFrac.y;
        float t4 = (rt.y - start.y)*dirFrac.y;
        float t5 = (lb.z - start.z)*dirFrac.z;
        float t6 = (rt.z - start.z)*dirFrac.z;
        float t;
        float tmin = Mathf.Max(Mathf.Max(Mathf.Min(t1, t2), Mathf.Min(t3, t4)), Mathf.Min(t5, t6));
        float tmax = Mathf.Min(Mathf.Min(Mathf.Max(t1, t2), Mathf.Max(t3, t4)), Mathf.Max(t5, t6));

// if tmax < 0, ray (line) is intersecting AABB, but the whole AABB is behind us
        if (tmax < 0)
        {
            t = tmax;
            return false;
        }

// if tmin > tmax, ray doesn't intersect AABB
        if (tmin > tmax)
        {
            t = tmax;
            return false;
        }

        t = tmin;
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
