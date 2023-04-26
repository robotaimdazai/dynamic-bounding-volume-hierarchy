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
      SetAABB();
      Tree.InsertLeaf(_index,_aabb,this);
   }

   private void Update()
   {
      UpdateTree();
   }

   [ContextMenu("UpdateTree")]
   private void UpdateTree()
   {
      bool samePos = DBVHUtils.AlmostEqual(_cachedPos, _cachedTransform.position, _tolerance);
      bool sameRot = DBVHUtils.AlmostEqual(_cachedRot, _cachedTransform.rotation.eulerAngles, _tolerance);
      bool sameScale = DBVHUtils.AlmostEqual(_cachedScale, _cachedTransform.localScale, _tolerance);
      
      if (samePos && sameRot && sameScale)
      {
         return;
      }

      SetAABB();
      Tree.Remove(_index);
      Tree.InsertLeaf(_index,_aabb,this);
      _cachedPos = _cachedTransform.position;
      _cachedRot = _cachedTransform.rotation.eulerAngles;
      _cachedScale = _cachedTransform.localScale;
      
   }

   private void SetAABB()
   {
      _rectTransform.GetWorldCorners(_corners);
      float xMin = Mathf.Infinity;
      float xMax = Mathf.NegativeInfinity;
      float yMin = Mathf.Infinity;
      float yMax = Mathf.NegativeInfinity;
      float zMin = Mathf.Infinity;
      float zMax = Mathf.NegativeInfinity;
      for (int i = 0; i < 4; i++)
      {
         if (_corners[i].x < xMin)
         {
            xMin = _corners[i].x;
         }
         if (_corners[i].x > xMax)
         {
            xMax = _corners[i].x;
         }
         if (_corners[i].y < yMin)
         {
            yMin = _corners[i].y;
         }
         if (_corners[i].y > yMax)
         {
            yMax = _corners[i].y;
         }
         if (_corners[i].z < zMin)
         {
            zMin = _corners[i].z;
         }
         if (_corners[i].z > zMax)
         {
            zMax = _corners[i].z;
         }
         
      }
      var min = new Vector3(xMin, yMin, zMin);
      var max = new Vector3(xMax, yMax, zMax);
      _aabb.Min = min;
      _aabb.Max = max;
   }
   
}
