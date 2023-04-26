using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DBVHCanvas : DBVHBase
{
   private Canvas _canvas;
   private RectTransform _rectTransform;
   private AABB _aabb = new();
   private Vector3[] _corners = new Vector3[8];
   private int _index = -1;
   private Transform _cachedTransform;
   private Vector3 _cachedPos;
   private Vector3 _cachedRot;
   private Vector3 _cachedScale;
   private Vector3 _tolerance = new Vector3(0.1f,0.1f,0.1f);

   private void Start()
   {
      _canvas = GetComponent<Canvas>();
      _rectTransform = GetComponent<RectTransform>();
      _index = gameObject.GetInstanceID();
      _cachedTransform = transform;
      _cachedPos = _cachedTransform.position;
      _cachedRot = _cachedTransform.rotation.eulerAngles;
      _cachedScale = _cachedTransform.localScale;
      SetAABV2();
      Tree.InsertLeaf(_index,_aabb);
   }

   private void Update()
   {
      UpdateTree();
   }

   [ContextMenu("UpdateTree")]
   private void UpdateTree()
   {
      bool samePos = AlmostEqual(_cachedPos, _cachedTransform.position, _tolerance);
      bool sameRot = AlmostEqual(_cachedRot, _cachedTransform.rotation.eulerAngles, _tolerance);
      bool sameScale = AlmostEqual(_cachedScale, _cachedTransform.localScale, _tolerance);
      
      if (samePos && sameRot && sameScale)
      {
         return;
      }

      SetAABV2();
      Tree.Remove(_index);
      Tree.InsertLeaf(_index,_aabb);
      _cachedPos = _cachedTransform.position;
      _cachedRot = _cachedTransform.rotation.eulerAngles;
      _cachedScale = _cachedTransform.localScale;
      
   }

   private void SetAABB()
   {
      _rectTransform.GetWorldCorners(_corners);
      float width = _corners[3].x - _corners[0].x;
      float height = _corners[1].y - _corners[1].y;
      float depth = Mathf.Sqrt(width * width + height * height);
      var boxMax = _corners[2] + Vector3.forward * depth;
      _aabb.Min = _corners[0];
      _aabb.Max = boxMax;
   }

   private void SetAABV2()
   {
      var rect = _rectTransform.rect;
      float canvasWidth = rect.width;
      float canvasHeight = rect.height;
      float depth = Mathf.Sqrt(canvasWidth * canvasWidth + canvasHeight * canvasHeight);
      Vector3 canvasPosition = _rectTransform.localPosition;
      Bounds canvasBounds = new Bounds(canvasPosition, Vector3.zero);
      Vector3 corner1 = canvasPosition + new Vector3(-depth / 2f, depth / 2f, -depth/2f);
      Vector3 corner2 = canvasPosition + new Vector3(depth / 2f, depth / 2f, -depth/2f);
      Vector3 corner3 = canvasPosition + new Vector3(-depth / 2f, -depth / 2f, -depth/2f);
      Vector3 corner4 = canvasPosition + new Vector3(depth / 2f, -depth / 2f, -depth/2f);
      Vector3 corner5 = corner1 + new Vector3(0f, 0f, depth );
      Vector3 corner6 = corner2 + new Vector3(0f, 0f, depth );
      Vector3 corner7 = corner3 + new Vector3(0f, 0f, depth );
      Vector3 corner8 = corner4 + new Vector3(0f, 0f, depth );
      canvasBounds.Encapsulate(corner1);
      canvasBounds.Encapsulate(corner2);
      canvasBounds.Encapsulate(corner3);
      canvasBounds.Encapsulate(corner4);
      canvasBounds.Encapsulate(corner5);
      canvasBounds.Encapsulate(corner6);
      canvasBounds.Encapsulate(corner7);
      canvasBounds.Encapsulate(corner8);

      _aabb.Min = canvasBounds.min;
      _aabb.Max = canvasBounds.max;
   }
}
