using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlapTester : MonoBehaviour
{
    public float range = 10f;
    private Ray _ray;
    private void Update()
    {
        
        /*
        foreach (DBHVComponent component in DBVHBase.AllComponents)
        {
            if (component.AABB.Intersects(_ray,range))
            {
                Debug.Log("Hit: "+component.name);
            }
        }
        */
       

    }

    private void OnDrawGizmos()
    {
        _ray= new Ray(transform.position, transform.forward);
        Tree.Raycast(DBVHBase.Tree, _ray, range);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_ray.origin, _ray.origin + _ray.direction * range);
    }
}
