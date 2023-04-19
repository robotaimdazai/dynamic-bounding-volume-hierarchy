using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using UnityEngine;

public class Tree
{
    private List<Node> _nodes;
    private int _nodeCount = 0;
    private int _rootIndex;
    private readonly int _nullIndex = -1;

    public void InsertLeaf(AABB box)
    {
        int leafIndex = AllocateLeafNode(box);
        if (_nodeCount == 1)
        {
            _rootIndex = leafIndex;
            return;
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
            // sibling is not root
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
        }

        //3, Traverse the tree upwards refitting AABBs

        int index = _nodes[leafIndex].ParentIndex;
        while (index != _nullIndex)
        {
            int child1 = _nodes[index].Child1;
            int child2 = _nodes[index].Child2;
            _nodes[index].Box = _nodes[child1].Box.Union(_nodes[child2].Box);
            index = _nodes[index].ParentIndex;
        }

    }

    private int AllocateLeafNode(AABB box)
    {
        Node node = new Node();
        _nodeCount++;
        _nodes.Add(node);
        node.ObjectIndex = _nodeCount;
        node.IsLeaf = true;
        node.Box = box;
        return _nodeCount;
    }

    private int AllocateInternalNode()
    {
        Node node = new Node();
        _nodes.Add(node);
        node.ObjectIndex = _nodeCount;
        node.IsLeaf = false;
        _nodeCount++;
        return _nodeCount;
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

    public float ComputeCost()
    {
        float cost = 0f;
        for (int i = 0; i < _nodeCount; i++)
        {
            if (_nodes[i].IsLeaf == false)
            {
                cost += _nodes[i].Box.Area();
            }
        }

        return cost;
    }

    public static float ComputeCost(Tree tree)
    {
        return tree.ComputeCost();
    }
}

    public class Node
    {
        public AABB Box;
        public int ObjectIndex;
        public int ParentIndex;
        public int Child1;
        public int Child2;
        public bool IsLeaf;
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

        public float Area()
        {
            Vector3 d = Max - Min;
            return 2.0f * (d.x * d.y + d.y * d.z + d.z * d.x);
        }

        public static float Area(AABB other)
        {
            return other.Area();
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