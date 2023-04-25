using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class Tree
{
    private  Dictionary<int,Node> _nodes = new();
    private int _rootIndex;
    private readonly int _nullIndex = -1;
    public Dictionary<int,Node> Nodes => _nodes;
    public int RootIndex => _rootIndex;
    
    private int RayCast(Ray ray, float range) 
    {
        if (_rootIndex == _nullIndex) return _nullIndex;
        Stack<int> stack = new();
        stack.Push(_rootIndex);
        
        //TODO: Remove this, only for debug
        foreach (var node in _nodes.Values)
        {
            node.IsHit = false;
        }
        
        while (stack.Count >0)
        {
            var index = stack.Pop();
            if (!AABB.Intersects(_nodes[index].Box, ray, range))
            {
                continue;
            }
            Debug.Log(index);
            _nodes[index].IsHit = true;
            if (_nodes[index].IsLeaf)
            {
                int objectIndex = _nodes[index].ObjectIndex;
                return index;
            }
            else
            {
                stack.Push(_nodes[index].Child1);
                stack.Push(_nodes[index].Child2);
            }
        }

        return _nullIndex; 
    }

    private int AllocateLeafNode(int objectIndex, AABB box)
    {
        if (!_nodes.ContainsKey(objectIndex))
        {
            _nodes[objectIndex] = new Node();
        }
        _nodes[objectIndex].Box = box;
        _nodes[objectIndex].ObjectIndex = objectIndex;
        _nodes[objectIndex].IsLeaf = true;
        return objectIndex;
    }

    private int AllocateInternalNode()
    {
        int objectIndex = Guid.NewGuid().GetHashCode();
        Node node = new Node
        {
            IsLeaf = false,
            ObjectIndex = objectIndex
        };
        _nodes[objectIndex] = node;
        return objectIndex;
    }
    
    private int PickBest(int leaf)
    {
        //this algorithm is working on branch and bound pruning
        var sBest = _rootIndex;
        var cBest = _nodes[_rootIndex].Box.Union(_nodes[leaf].Box).Area(); // SA(1 U L)
        var priorityQueue = new Queue<int>();
        priorityQueue.Enqueue(_rootIndex);
        while (priorityQueue.Count>0)
        {
            var index = priorityQueue.Dequeue();
            if(index == _nullIndex) continue;
            var node = _nodes[index];
            var child1 = node.Child1;
            var child2 = node.Child2;
            //get direct cost for this node
            float directCost = AABB.Area(AABB.Union(_nodes[leaf].Box, _nodes[index].Box));
            //calculate inherited cost for the node
            float inheritedCost = 0f;
            int currentIndex = _nodes[index].ParentIndex;
            while (currentIndex != _nullIndex)
            {
                inheritedCost+= AABB.Area(AABB.Union(_nodes[currentIndex].Box, _nodes[leaf].Box)) - 
                                _nodes[currentIndex].Box.Area();
                currentIndex = _nodes[currentIndex].ParentIndex;
            }
            float finalCost = directCost + inheritedCost;
            if (finalCost < cBest)
            {
                cBest = finalCost;
                sBest = index;
            }

            if (finalCost > cBest)
            {
                continue;
            }

            if (child1 != _nullIndex) priorityQueue.Enqueue(child1);
            if (child2 != _nullIndex) priorityQueue.Enqueue(child2);
        }
        
        return sBest;
    }
    
    private void RefitAncestors(int parentIndex)
    {
        while (parentIndex != _nullIndex)
        {
            int child1 = _nodes[parentIndex].Child1;
            int child2 = _nodes[parentIndex].Child2;
            _nodes[parentIndex].Box = _nodes[child1].Box.Union(_nodes[child2].Box);
            parentIndex = _nodes[parentIndex].ParentIndex;
        }
    }

    public static void InsertLeaf(Tree tree, int objectIndex, AABB box)
    {
        tree.InsertLeaf(objectIndex, box);
    }
    public static void Remove(Tree tree, int index)
    {
        tree.Remove(index);
    }
    
    public static int Raycast(Tree tree, Ray ray, float range )
    {
        return tree.RayCast(ray, range);
    }
    
    public static float ComputeCost(Tree tree)
    {
        return tree.ComputeCost();
    }
    
    public void InsertLeaf(int objectIndex, AABB box)
    {
        int leafIndex = AllocateLeafNode(objectIndex,box);
        if (_nodes.Count == 1)
        {
            _rootIndex = leafIndex;
            return ;
        }

        //1, Find best sibling for new leaf
        //Branch and bound
        int bestSibling = _rootIndex;
        bestSibling = PickBest(leafIndex);
        
        //2, Create a new parent

        int sibling = bestSibling;
        int oldParent = _nodes[sibling].ParentIndex;
        int newParent = AllocateInternalNode();
        _nodes[newParent].ParentIndex = oldParent;
        _nodes[newParent].Box = box.Union(_nodes[sibling].Box);
        if (oldParent != _nullIndex)
        {
            // sibling was not root
            if (_nodes[oldParent].Child1 == sibling)
            {
                _nodes[oldParent].Child1 = newParent;
            }
            else
            {
                _nodes[oldParent].Child2 = newParent;
            }

            _nodes[newParent].Child1 = sibling;
            _nodes[newParent].Child2 = leafIndex;
            _nodes[sibling].ParentIndex = newParent;
            _nodes[leafIndex].ParentIndex = newParent;
        }
        else
        {
            //the sibling was root
            _nodes[newParent].Child1 = sibling;
            _nodes[newParent].Child2 = leafIndex;
            _nodes[sibling].ParentIndex = newParent;
            _nodes[leafIndex].ParentIndex = newParent;
            _rootIndex = newParent;
        }

        //3, Traverse the tree upwards refitting AABBs
        int parentIndex = _nodes[leafIndex].ParentIndex;
        RefitAncestors(parentIndex);
        
    }
   
    public void Remove(int index)
    {
        if (index == _nullIndex || !_nodes.ContainsKey(index) || index == _rootIndex) return;
        int parent = _nodes[index].ParentIndex;
        int child1 = _nodes[parent].Child1;
        int child2 = _nodes[parent].Child2;
        int sibling = child1;
        if (_nodes[parent].Child1 == index)
        {
            //child 2 is remaining
            sibling = child2;

        }
        else if (_nodes[parent].Child2 == index)
        {
            //child 1 is remaining   
            sibling = child1;
        }
        
        //updating grandParent
        //get grandParent 
        var grandParent = _nodes[parent].ParentIndex;
        if (grandParent == _nullIndex)
        {
            //grandParent is root
            _rootIndex = sibling;
        }
        else
        {
            if (parent == _nodes[grandParent].Child1)
            {
                _nodes[grandParent].Child1 = _nodes[sibling].ObjectIndex;
            }
            else if (parent == _nodes[grandParent].Child2)
            {
                _nodes[grandParent].Child2 = _nodes[sibling].ObjectIndex;
            }
        }
        
        _nodes.Remove(parent);
        _nodes.Remove(index);
        
        _nodes[sibling].ParentIndex = grandParent;
        if (grandParent == -1)
        {
            //of the grandParent was root then, parent should be null, as sibling is the new root
            _nodes[sibling].ParentIndex = _nullIndex;
        }

        RefitAncestors(_nodes[sibling].ParentIndex);
        
    }
    
    public float ComputeCost()
    {
        float cost = 0f;
        for (int i = 0; i < _nodes.Count; i++)
        {
            if (_nodes[i].IsLeaf == false)
            {
                cost += _nodes[i].Box.Area();
            }
        }

        return cost;
    }
}

    public class Node
    {
        public AABB Box;
        public int ObjectIndex =-1;
        public int ParentIndex =-1;
        public int Child1 =-1;
        public int Child2 =-1;
        public bool IsLeaf;
        public bool IsHit = false;
    }

    public struct AABB
    {
        public Vector3 Min;
        public Vector3 Max;

        public AABB Union(AABB other)
        {
            AABB c;
            c.Min = Vector3.Min(Min, other.Min);
            c.Max = Vector3.Max(Max, other.Max);
            return c;
        }

        public static AABB Union(AABB a, AABB b)
        {
            return a.Union(b);
        }
        
        public static float Area(AABB other)
        {
            return other.Area();
        }

        public static bool Intersects(AABB other, Ray ray, float range)
        {
            return other.Intersects(ray, range);
        }
        public float Area()
        {
            Vector3 d = Max - Min;
            return 2.0f * (d.x * d.y + d.y * d.z + d.z * d.x);
        }
        public bool Intersects(Ray ray, float range = 0)
        {
            float tmin = float.MinValue;
            float tmax = float.MaxValue;
            if (range != 0)
                tmax = range;

            for (int i = 0; i < 3; i++)
            {
                if (Math.Abs(ray.direction[i]) < float.Epsilon)
                {
                    if (ray.origin[i] < Min[i] || ray.origin[i] > Max[i])
                    {
                        return false;
                    }
                }
                else
                {
                    float t1 = (Min[i] - ray.origin[i]) / ray.direction[i];
                    float t2 = (Max[i] - ray.origin[i]) / ray.direction[i];

                    tmin = Math.Max(tmin, Math.Min(t1, t2));
                    tmax = Math.Min(tmax, Math.Max(t1, t2));

                    if (tmax < 0 || tmin > tmax)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }