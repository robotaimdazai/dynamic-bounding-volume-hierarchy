using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

public class Tree
{
    private  Dictionary<int,Node> _nodes = new();
    private  Dictionary<int,DBVHBase> _components = new();
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
        Dictionary<int, Node> allNodes = new Dictionary<int, Node>(_nodes);
        foreach (var index in _nodes.Keys)
        {
            var thisNode = _nodes[index];
            thisNode.IsHit = false;
            allNodes[index] = thisNode;
        }
        _nodes = allNodes;
        //--
        
        while (stack.Count >0)
        {
            var index = stack.Pop();
            if (!AABB.Intersects(_nodes[index].Box, ray, range))
            {
                continue;
            }

            if (_nodes[index].IsLeaf && !_components[index].gameObject.activeSelf)
            {
                continue;
            }

            //TODO remove this, only for debugging
            Debug.Log(index);
            var thisNode = _nodes[index];
            thisNode.IsHit = true;
            _nodes[index] = thisNode;
            //--
            
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
            _nodes[objectIndex] = new Node
            {
                ObjectIndex = objectIndex,
                Box = box,
                ParentIndex = -1,
                Child1 = -1,
                Child2 = -1,
                IsLeaf = true,
                IsHit =  false
            };
            return objectIndex;
        }

        var foundNode = _nodes[objectIndex];
        foundNode.Box = box;
        foundNode.ObjectIndex = objectIndex;
        foundNode.IsLeaf = true;
        _nodes[objectIndex] = foundNode;
        return objectIndex;
    }

    private int AllocateInternalNode()
    {
        int objectIndex = Guid.NewGuid().GetHashCode();
        Node node = new Node
        {
            ObjectIndex = objectIndex,
            ParentIndex = -1,
            Child1 = -1,
            Child2 = -1,
            IsLeaf = false,
            IsHit =  false
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
    
    private void RefitAncestors(int index)
    {
        index = _nodes[index].ParentIndex;
        while (index != _nullIndex)
        {
            var thisNode = _nodes[index];
            int child1 = _nodes[index].Child1;
            int child2 = _nodes[index].Child2;
            thisNode.Box = _nodes[child1].Box.Union(_nodes[child2].Box);
            _nodes[index] = thisNode;
            index = _nodes[index].ParentIndex;
        }
    }

    public static void InsertLeaf(Tree tree, int objectIndex, AABB box, DBVHBase component)
    {
        tree.InsertLeaf(objectIndex, box, component);
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
    
    public void InsertLeaf(int objectIndex, AABB box,DBVHBase component)
    {
        int leafIndex = AllocateLeafNode(objectIndex,box);
        _components[objectIndex] = component;
        var leafNode = _nodes[leafIndex];
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
        var siblingNode = _nodes[sibling];
        int oldParent = _nodes[sibling].ParentIndex;
        int newParent = AllocateInternalNode();
        var newParentNode = _nodes[newParent];
        newParentNode.ParentIndex = oldParent;
        newParentNode.Box = box.Union(_nodes[sibling].Box);

        //_nodes[newParent].ParentIndex = oldParent;
        //_nodes[newParent].Box = box.Union(_nodes[sibling].Box);
        if (oldParent != _nullIndex)
        {
            var oldParentNode = _nodes[oldParent];
            // sibling was not root
            if (oldParentNode.Child1 == sibling)
            {
                oldParentNode.Child1 = newParent;
            }
            else
            {
                oldParentNode.Child2 = newParent;
            }

            newParentNode.Child1 = sibling;
            newParentNode.Child2 = leafIndex;
            siblingNode.ParentIndex = newParent;
            leafNode.ParentIndex = newParent;
            _nodes[oldParent] = oldParentNode;
        }
        else
        {
            //the sibling was root
            newParentNode.Child1 = sibling;
            newParentNode.Child2 = leafIndex;
            siblingNode.ParentIndex = newParent;
            leafNode.ParentIndex = newParent;
            _rootIndex = newParent;
        }

        _nodes[newParent] = newParentNode;
        _nodes[sibling] = siblingNode;
        _nodes[leafIndex] = leafNode;

        //3, Traverse the tree upwards refitting AABBs
        RefitAncestors(leafIndex);
        
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
            var grandParentNode = _nodes[grandParent];
            if (parent == _nodes[grandParent].Child1)
            {
                grandParentNode.Child1 = _nodes[sibling].ObjectIndex;
            }
            else if (parent == _nodes[grandParent].Child2)
            {
                grandParentNode.Child2 = _nodes[sibling].ObjectIndex;
            }
            _nodes[grandParent] = grandParentNode;
        }
        
        _nodes.Remove(parent);
        _nodes.Remove(index);
        _components.Remove(index);

        var siblingNode = _nodes[sibling];
        siblingNode.ParentIndex = grandParent;
        if (grandParent == -1)
        {
            //of the grandParent was root then, parent should be null, as sibling is the new root
            siblingNode.ParentIndex = _nullIndex;
        }

        _nodes[sibling] = siblingNode;

        RefitAncestors(sibling);
        
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

    public struct Node
    {
        public AABB Box;
        public int ObjectIndex;
        public int ParentIndex;
        public int Child1;
        public int Child2;
        public bool IsLeaf;
        public bool IsHit;
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