using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DBHVComponent : DBVHBase
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
      if (_cachedPos == _cachedTransform.position &&
          _cachedRot == _cachedTransform.rotation.eulerAngles &&
          _cachedScale == _cachedTransform.localScale)
      {
         return;
      }
      SetAABB();
      Tree.Remove(_index);
      Tree.InsertLeaf(_index,_aabb);
      _cachedPos = _cachedTransform.position;
      _cachedRot = _cachedTransform.rotation.eulerAngles;
      _cachedScale = _cachedTransform.localScale;
      
      Debug.Log("yo");
   }

   private void SetAABB()
   {
      _rectTransform.GetWorldCorners(_corners);
      _aabb.Min = _corners[0];
      _aabb.Max = _corners[2];
   }
}
