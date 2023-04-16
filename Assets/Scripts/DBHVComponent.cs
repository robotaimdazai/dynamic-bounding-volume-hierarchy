using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteInEditMode]
public class DBHVComponent : DBVHBase
{
   private Canvas _canvas;
   private RectTransform _rectTransform;
   private AABB _aabb = new AABB();
   private Vector3[] _corners = new Vector3[4];

   public AABB AABB => _aabb;

   private void Start()
   {
      _canvas = GetComponent<Canvas>();
      _rectTransform = GetComponent<RectTransform>();
   }
   
   private void Update()
   {
      _rectTransform.GetWorldCorners(_corners);
      _aabb.Min = _corners[0];
      _aabb.Max = _corners[2];
   }

   private void OnDrawGizmos()
   {
      Vector3 center = new Vector3((_aabb.Min.x + _aabb.Max.x) / 2, (_aabb.Min.y + _aabb.Max .y) / 2, 0);
      float xSize = (center.x - _aabb.Min.x)*2;
      float ySize = (_aabb.Max.y - center.y)*2;
      Gizmos.color = Color.red;
      Gizmos.DrawCube(center,new Vector3(xSize,ySize));
   }
}
