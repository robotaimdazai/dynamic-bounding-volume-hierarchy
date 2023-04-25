using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DBVHCanvas : DBVHBase
{
   private Canvas _canvas;
   private RectTransform _rectTransform;
   private AABB _aabb = new();
   private Vector3[] _corners = new Vector3[4];
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
      
      SetAABB();
      Tree.Remove(_index);
      Tree.InsertLeaf(_index,_aabb);
      _cachedPos = _cachedTransform.position;
      _cachedRot = _cachedTransform.rotation.eulerAngles;
      _cachedScale = _cachedTransform.localScale;
      
   }

   private void SetAABB()
   {
      _rectTransform.GetWorldCorners(_corners);
      _aabb.Min = _corners[0];
      _aabb.Max = _corners[2];
   }
}
