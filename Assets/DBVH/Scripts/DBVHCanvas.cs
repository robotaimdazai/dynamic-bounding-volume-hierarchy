using Unity.VisualScripting;
using UnityEngine;

namespace DBVH
{
   public class DBVHCanvas : DBVHBase
   {
      private RectTransform _rectTransform;
      private Vector3 _tolerance = new Vector3(0.1f,0.1f,0.1f);
      protected override void Start()
      {
         base.Start();
         _rectTransform = GetComponent<RectTransform>();
         //This is how you should normally init
         //1, Set AABB
         //2, Insert to Binary tree
         SetAABB();
         BinaryTree.InsertLeaf(Index,AABB,this);
      }
      private void Update()
      {
         if (IfAnyTransformHasChanged())
         {
            UpdateTree();
            SetTransformHasChangedForAll(false);
         }
      }
      private void SetAABB()
      {
         AABB = DBVHUtils.GetAABBFromRectTransform(_rectTransform);
      }
      private void UpdateTree()
      {
         SetAABB();
         BinaryTree.Remove(Index);
         BinaryTree.InsertLeaf(Index,AABB,this);
      }
   }
}
