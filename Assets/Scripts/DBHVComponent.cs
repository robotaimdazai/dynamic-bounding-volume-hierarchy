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
   public AABB AABB => _aabb;

   private void Start()
   {
      _canvas = GetComponent<Canvas>();
      _rectTransform = GetComponent<RectTransform>();
      _index = gameObject.GetInstanceID();
      SetAABB(); 
      Tree.InsertLeaf(_index,_aabb);
   }

   [ContextMenu("UpdateTree")]
   public void UpdateTree()
   {
      SetAABB();
      Tree.Remove(_index);
      Tree.InsertLeaf(_index,_aabb);
   }

   private void SetAABB()
   {
      _rectTransform.GetWorldCorners(_corners);
      _aabb.Min = _corners[0];
      _aabb.Max = _corners[2];
   }
}
