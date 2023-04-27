using UnityEditor;
using UnityEngine;

namespace DBVH
{
    #if UNITY_EDITOR
    public class DebugDraw : MonoBehaviour
    {
        private void OnDrawGizmos()
        {
            if(!DBVHBase.Debug) return;
            foreach (var node in DBVHBase.BinaryTree.Nodes.Values)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.white;
                Gizmos.color =Color.blue;
                Vector3 center = new Vector3(
                    (node.Box.Min.x + node.Box.Max.x) / 2,
                    (node.Box.Min.y + node.Box.Max .y) / 2,
                    (node.Box.Min.z + node.Box.Max.z) / 2);
                float multiplier = 2f;
                if (node.IsLeaf)
                {
                    multiplier = 1.9f;
                    Gizmos.color =Color.green;
                }
                if (node.IsHit)
                {
                    Gizmos.color =Color.red;
                    style.normal.textColor = Color.red;
                }
                if (node.ObjectIndex == DBVHBase.BinaryTree.RootIndex)
                {
                    multiplier = 2.1f;
                    Gizmos.color =Color.black;
                }
                float xSize = (center.x - node.Box.Min.x)*multiplier;
                float ySize = (node.Box.Max.y - center.y)*multiplier;
                float zSize = (node.Box.Max.z - center.z)*multiplier;
                Handles.Label(center,node.ObjectIndex.ToString(),style);
                Gizmos.DrawWireCube(center,new Vector3(xSize,ySize,zSize));
            }
        }
    }
    #endif
}
