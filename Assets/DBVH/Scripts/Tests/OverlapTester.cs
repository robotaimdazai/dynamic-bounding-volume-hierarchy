using UnityEngine;

namespace DBVH
{
#if UNITY_EDITOR
    public class OverlapTester : MonoBehaviour
    {
        public float range = 10f;
        private Ray _ray;
        private void OnDrawGizmos()
        {
            if(!DBVHBase.Debug) return;
            _ray= new Ray(transform.position, transform.forward);
            BinaryTree.Raycast(DBVHBase.BinaryTree, _ray);
            Gizmos.color = Color.white;
            Gizmos.DrawLine(_ray.origin, _ray.origin + _ray.direction * range);
        }
    }
#endif
}
