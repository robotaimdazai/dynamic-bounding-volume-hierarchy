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
         UpdateTree();
      }

      private void SetAABB()
      {
         AABB = DBVHUtils.GetAABBFromRectTransform(_rectTransform);
         if (DBVHUtils.AlmostEqual(AABB.Max, AABB.Min, _tolerance))
         {
            AABB = DBVHUtils.GetAABBFromTransform(CachedTransform);
         }
      }

      private void UpdateTree()
      {
         bool samePos = DBVHUtils.AlmostEqual(CachedPos, CachedTransform.position, _tolerance);
         bool sameRot = DBVHUtils.AlmostEqual(CachedRot, CachedTransform.rotation.eulerAngles, _tolerance);
         bool sameScale = DBVHUtils.AlmostEqual(CachedScale, CachedTransform.localScale, _tolerance);
      
         if (samePos && sameRot && sameScale)
         {
            return;
         }
         SetAABB();
         BinaryTree.Remove(Index);
         BinaryTree.InsertLeaf(Index,AABB,this);
         CachedPos = CachedTransform.position;
         CachedRot = CachedTransform.rotation.eulerAngles;
         CachedScale = CachedTransform.localScale;
      }
   }
}
