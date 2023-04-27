using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DBVH
{
   public class DBVHBase : MonoBehaviour
   {
      public static bool Debug = true; // set this to false in production
      public static BinaryTree BinaryTree = new();
      protected AABB AABB = new();
      protected int Index = -1;
      protected Transform CachedTransform;
      protected Vector3 CachedPos; 
      protected Vector3 CachedRot; 
      protected Vector3 CachedScale;
      protected virtual void Start()
      {
         Index = gameObject.GetInstanceID();
         CachedTransform = transform;
         CachedPos = CachedTransform.position;
         CachedRot = CachedTransform.rotation.eulerAngles;
         CachedScale = CachedTransform.localScale;
      }
   }
   public static class DBVHUtils
   {
      public static bool AlmostEqual(Vector3 v1, Vector3 v2, Vector3 precision)
      {
         bool equal = true;
         if (Mathf.Abs (v1.x - v2.x) > precision.x) equal = false;
         if (Mathf.Abs (v1.y - v2.y) > precision.y) equal = false;
         if (Mathf.Abs (v1.z - v2.z) > precision.z) equal = false;
         return equal;
      }
      public static AABB GetAABBFromRectTransform(RectTransform rectTransform)
      {
         Vector3[] corners = new Vector3[4];
         rectTransform.GetWorldCorners(corners);
         float xMin = Mathf.Infinity;
         float xMax = Mathf.NegativeInfinity;
         float yMin = Mathf.Infinity;
         float yMax = Mathf.NegativeInfinity;
         float zMin = Mathf.Infinity;
         float zMax = Mathf.NegativeInfinity;
         for (int i = 0; i < 4; i++)
         {
            if (corners[i].x < xMin)
            {
               xMin = corners[i].x;
            }
            if (corners[i].x > xMax)
            {
               xMax = corners[i].x;
            }
            if (corners[i].y < yMin)
            {
               yMin = corners[i].y;
            }
            if (corners[i].y > yMax)
            {
               yMax = corners[i].y;
            }
            if (corners[i].z < zMin)
            {
               zMin = corners[i].z;
            }
            if (corners[i].z > zMax)
            {
               zMax = corners[i].z;
            }
         }
         var min = new Vector3(xMin, yMin, zMin);
         var max = new Vector3(xMax, yMax, zMax);
         AABB aabb = new AABB();
         aabb.Min = min;
         aabb.Max = max;
         return aabb;
      }
      private static Bounds CalculateBounds(Transform transform)
      {
         Bounds bounds = new Bounds(transform.position, Vector3.zero);
         foreach (Transform child in transform)
         {
            Bounds childBounds = CalculateBounds(child);
            bounds.Encapsulate(childBounds);
         }
         return bounds;
      }
      public static AABB GetAABBFromTransform(Transform transform)
      {
         var bounds =CalculateBounds(transform);
         AABB aabb = new AABB();
         aabb.Min = bounds.min;
         aabb.Max = bounds.max;
         return aabb;
      }
   }
}