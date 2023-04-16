using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class OverlapTester : MonoBehaviour
{
    public float range = 10f;
    private Ray _ray;
    private void Update()
    {
        _ray= new Ray(transform.position, transform.forward);
        var p1 = _ray.origin;
        var p2 = _ray.origin + _ray.direction * range;
        foreach (DBHVComponent component in DBVHBase.AllComponents)
        {
            if (component.AABB.Intersects(_ray))
            {
                Debug.Log("Hit");
            }
            else
            {
                Debug.Log("No hit");
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(_ray.origin, _ray.origin + _ray.direction * range);
    }
}
