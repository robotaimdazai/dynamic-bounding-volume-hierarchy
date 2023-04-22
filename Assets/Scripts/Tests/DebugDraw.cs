using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugDraw : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        foreach (var node in DBVHBase.Tree.Nodes)
        {
            
            Vector3 center = new Vector3((node.Box.Min.x + node.Box.Max.x) / 2, (node.Box.Min.y + node.Box.Max .y) / 2, 0);
            float multiplier = 2f;
            if (node.IsLeaf)
            {
                multiplier = 1.96f;
            }
            float xSize = (center.x - node.Box.Min.x)*multiplier;
            float ySize = (node.Box.Max.y - center.y)*multiplier;
            Gizmos.color =Color.white;
            Gizmos.DrawWireCube(center,new Vector3(xSize,ySize));
            if (node.Hit)
            {
                Gizmos.color =Color.green;
                Gizmos.DrawWireCube(center,new Vector3(xSize,ySize));
            }
        }
    }
}
