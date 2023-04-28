using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DBVH
{
   public class DBVHBase : MonoBehaviour
   {
      public static bool DebugMode = true; // set this to false in production
      public static BinaryTree BinaryTree = new();
      protected AABB AABB = new();
      protected int Index = -1;
      protected Transform CachedTransform;
      protected virtual void Start()
      {
         Index = gameObject.GetInstanceID();
         CachedTransform = transform;
         SetTransformHasChangedForAll(false);
      }
      protected virtual void SetTransformHasChangedForAll(bool value)
      {
         CachedTransform.hasChanged = false;
         foreach (Transform child in CachedTransform)
         {
            child.hasChanged = value;
         }
      }
      protected virtual bool IfAnyTransformHasChanged()
      {
         if (CachedTransform.hasChanged) return true;
         foreach (Transform child in CachedTransform)
         {
            if (child.hasChanged) return true;
         }
         return false;
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

         Vector3 childMin = min;
         Vector3 childMax = max;
         foreach (RectTransform child in rectTransform)
         {
            AABB thisAABB = new AABB();
            thisAABB =GetAABBFromRectTransform(child);
            if (thisAABB.Min.x < childMin.x)
            {
               childMin.x = thisAABB.Min.x;
            }
            if (thisAABB.Min.y < childMin.y)
            {
               childMin.y = thisAABB.Min.y;
            }
            if (thisAABB.Min.z < childMin.z)
            {
               childMin.z = thisAABB.Min.z;
            }
            //max
            if (thisAABB.Max.x > childMax.x)
            {
               childMax.x = thisAABB.Max.x;
            }
            if (thisAABB.Max.y > childMax.y)
            {
               childMax.y = thisAABB.Max.y;
            }
            if (thisAABB.Max.z > childMax.z)
            {
               childMax.z = thisAABB.Max.z;
            }
         }

         aabb.Min = childMin;
         aabb.Max = childMax;
         return aabb;
      }
      public static Bounds CalculateBounds(Transform transform)
      {
         Bounds bounds = new Bounds(transform.position, Vector3.zero);
         foreach (Transform child in transform)
         {
            Bounds childBounds = CalculateBounds(child);
            bounds.Encapsulate(childBounds);
         }
         return bounds;
      }
      
      public static Bounds GetBounds(Transform transform)
      {
         // Get the local corners of the cube
         Vector3[] corners = new Vector3[8];
         corners[0] = new Vector3(-0.5f, -0.5f, -0.5f);
         corners[1] = new Vector3(-0.5f, -0.5f, 0.5f);
         corners[2] = new Vector3(-0.5f, 0.5f, -0.5f);
         corners[3] = new Vector3(-0.5f, 0.5f, 0.5f);
         corners[4] = new Vector3(0.5f, -0.5f, -0.5f);
         corners[5] = new Vector3(0.5f, -0.5f, 0.5f);
         corners[6] = new Vector3(0.5f, 0.5f, -0.5f);
         corners[7] = new Vector3(0.5f, 0.5f, 0.5f);

         // Transform the corners to world space
         for (int i = 0; i < 8; i++)
         {
            corners[i] = transform.TransformPoint(corners[i]);
         }

         // Calculate the bounds based on the transformed corners
         Bounds bounds = new Bounds(corners[0], Vector3.zero);
         for (int i = 1; i < 8; i++)
         {
            bounds.Encapsulate(corners[i]);
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