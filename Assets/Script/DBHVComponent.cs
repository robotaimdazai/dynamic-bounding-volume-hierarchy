using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DBHVComponent : DBVHBase
{
   private Canvas _canvas;
   private RectTransform _rectTransform;
   private void OnDrawGizmos()
   {
      AABB aabb = new AABB();
      _canvas = GetComponent<Canvas>();
      _rectTransform = GetComponent<RectTransform>();
      Vector3[] corners = new Vector3[4];
      _rectTransform.GetWorldCorners(corners);
      aabb.LowerBound = corners[0];
      aabb.UpperBound = corners[2];
      Vector3 center = new Vector3((aabb.LowerBound.x + aabb.UpperBound.x) / 2, (aabb.LowerBound.y + aabb.UpperBound .y) / 2, 0);
      float xSize = (center.x - aabb.LowerBound.x)*2;
      float ySize = (aabb.UpperBound.y - center.y)*2;
      Gizmos.color = Color.red;
      Gizmos.DrawCube(center,new Vector3(xSize,ySize));
   }
}
