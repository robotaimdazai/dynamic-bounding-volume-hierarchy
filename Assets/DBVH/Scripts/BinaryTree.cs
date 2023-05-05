using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Schema;
using Unity.VisualScripting;
using UnityEngine;

namespace DBVH
{
    public class BinaryTree
    {
        private  Dictionary<int,Node> _nodes = new();
        private  Dictionary<int,DBVHBase> _components = new();
        private int _rootIndex;
        private readonly int _nullIndex = -1;
        public Dictionary<int,Node> Nodes => _nodes;
        public int RootIndex => _rootIndex;
        private bool RayCast(Ray ray) 
        {
            if (_rootIndex == _nullIndex) return false;
            Stack<int> stack = new();
            stack.Push(_rootIndex);
            
            //This code is only for debugging, either remove in production or set Debug to false
            if (DBVHBase.DebugMode)
            {
                Dictionary<int, Node> allNodes = new Dictionary<int, Node>(_nodes);
                foreach (var index in _nodes.Keys)
                {
                    _nodes[index].IsHit = false;
                }
                _nodes = allNodes;
            }
            
            while (stack.Count >0)
            {
                var index = stack.Pop();
                if (!AABB.Intersects(_nodes[index].Box, ray))
                {
                    continue;
                }

                if (_nodes[index].IsLeaf && !_components[index].gameObject.activeSelf)
                {
                    continue;
                }

                if (DBVHBase.DebugMode)
                {
                    _nodes[index].IsHit = true;
                }
                
                if (_nodes[index].IsLeaf)
                {
                    int objectIndex = _nodes[index].ObjectIndex;
                    //Insert your Raycast here
                    return true;
                }
                else
                {
                    stack.Push(_nodes[index].Child1);
                    stack.Push(_nodes[index].Child2);
                }
            }
            return false; 
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
    
        private int PickBestOld(int leaf)
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
        
        private int PickBest(int leaf)
        {
            //this algorithm is working on branch and bound pruning
            var sBest = _rootIndex;
            var cBest = _nodes[_rootIndex].Box.Union(_nodes[leaf].Box).Area(); // SA(1 U L)
            var priorityQueue = new PriorityQueue<int>();
            priorityQueue.Enqueue(_rootIndex, cBest);
            
            while (priorityQueue.Count > 0)
            {
                var (index, cost) = priorityQueue.Dequeue();
                if (index == _nullIndex) continue;

                if (cost > cBest) break; // Stop exploring this branch

                var node = _nodes[index];
                var child1 = node.Child1;
                var child2 = node.Child2;
                //get direct cost for this node
                float directCost = AABB.Area(AABB.Union(_nodes[leaf].Box, node.Box));
                //calculate inherited cost for the node
                float inheritedCost = 0f;
                int currentIndex = _nodes[index].ParentIndex;
                while (currentIndex != _nullIndex)
                {
                    inheritedCost += AABB.Area(AABB.Union(_nodes[leaf].Box, _nodes[currentIndex].Box)) - 
                                     _nodes[currentIndex].Box.Area();
                    currentIndex = _nodes[currentIndex].ParentIndex;
                }
                float finalCost = directCost + inheritedCost;
                if (finalCost < cBest)
                {
                    cBest = finalCost;
                    sBest = index;
                }

                if (child1 != _nullIndex) priorityQueue.Enqueue(child1, -finalCost);
                if (child2 != _nullIndex) priorityQueue.Enqueue(child2, -finalCost);
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
        public static void InsertLeaf(BinaryTree binaryTree, int objectIndex, AABB box, DBVHBase component)
        {
            binaryTree.InsertLeaf(objectIndex, box, component);
        }
        public static void Remove(BinaryTree binaryTree, int index)
        {
            binaryTree.Remove(index);
        }
    
        public static bool Raycast(BinaryTree binaryTree, Ray ray)
        {
            return binaryTree.RayCast(ray);
        }
    
        public static float ComputeCost(BinaryTree binaryTree)
        {
            return binaryTree.ComputeCost();
        }
    
        public void InsertLeaf(int objectIndex, AABB box,DBVHBase component)
        {
            int leafIndex = AllocateLeafNode(objectIndex,box);
            _components[objectIndex] = component;
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

                _nodes[newParent].Child1 = sibling;
                _nodes[newParent].Child2 = leafIndex;
                _nodes[sibling].ParentIndex = newParent;
                _nodes[leafIndex].ParentIndex = newParent;
                _nodes[oldParent] = oldParentNode;
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
                if (parent == _nodes[grandParent].Child1)
                {
                    _nodes[grandParent].Child1 = _nodes[sibling].ObjectIndex;
                }
                else if (parent == _nodes[grandParent].Child2)
                {
                    _nodes[grandParent].Child2 = _nodes[sibling].ObjectIndex;
                }
                _nodes[grandParent] = _nodes[grandParent];
            }
            _nodes.Remove(parent);
            _nodes.Remove(index);
            _components.Remove(index);
            _nodes[sibling].ParentIndex = grandParent;
            if (grandParent == -1)
            {
                //of the grandParent was root then, parent should be null, as sibling is the new root
                _nodes[sibling].ParentIndex = _nullIndex;
            }
        
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

    public class Node
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

        public static bool Intersects(AABB other, Ray ray)
        {
            return other.Intersects(ray);
        }
        public float Area()
        {
            Vector3 d = Max - Min;
            return 2.0f * (d.x * d.y + d.y * d.z + d.z * d.x);
        }
        public bool Intersects(Ray ray)
        {
            var origin = ray.origin;
            var direction = ray.direction;
            
            float tmin = (Min.x - origin.x) / direction.x;
            float tmax = (Max.x - origin.x) / direction.x;

            if (tmin > tmax)
            {
                (tmin, tmax) = (tmax, tmin);
            }

            float tymin = (Min.y - origin.y) / direction.y;
            float tymax = (Max.y - origin.y) / direction.y;

            if (tymin > tymax)
            {
                (tymin, tymax) = (tymax, tymin);
            }

            if ((tmin > tymax) || (tymin > tmax))
            {
                return false;
            }

            if (tymin > tmin)
            {
                tmin = tymin;
            }

            if (tymax < tmax)
            {
                tmax = tymax;
            }

            float tzmin = (Min.z - origin.z) / direction.z;
            float tzmax = (Max.z - origin.z) / direction.z;

            if (tzmin > tzmax)
            {
                (tzmin, tzmax) = (tzmax, tzmin);
            }

            if ((tmin > tzmax) || (tzmin > tmax))
            {
                return false;
            }
            return true;
        }
    }
}

public class PriorityQueue<T>
{
    private List<(T, float)> _items = new List<(T, float)>();
    private readonly IComparer<(T, float)> _comparer;

    public PriorityQueue() : this(Comparer<(T, float)>.Default) { }

    public PriorityQueue(IComparer<(T, float)> comparer)
    {
        _comparer = comparer;
    }

    public int Count => _items.Count;

    public void Enqueue(T item, float priority)
    {
        _items.Add((item, priority));
        int i = _items.Count - 1;
        while (i > 0)
        {
            int parent = (i - 1) / 2;
            if (_comparer.Compare(_items[parent], _items[i]) <= 0)
                break;

            (T, float) tmp = _items[parent];
            _items[parent] = _items[i];
            _items[i] = tmp;
            i = parent;
        }
    }

    public (T, float) Dequeue()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Queue is empty.");

        (T, float) result = _items[0];
        int lastIndex = _items.Count - 1;
        _items[0] = _items[lastIndex];
        _items.RemoveAt(lastIndex);
        lastIndex--;

        int i = 0;
        while (true)
        {
            int leftChild = i * 2 + 1;
            int rightChild = i * 2 + 2;
            if (leftChild > lastIndex)
                break;

            int j = leftChild;
            if (rightChild <= lastIndex && _comparer.Compare(_items[rightChild], _items[leftChild]) < 0)
                j = rightChild;

            if (_comparer.Compare(_items[j], _items[i]) >= 0)
                break;

            (T, float) tmp = _items[j];
            _items[j] = _items[i];
            _items[i] = tmp;
            i = j;
        }

        return result;
    }

    public (T, float) Peek()
    {
        if (_items.Count == 0)
            throw new InvalidOperationException("Queue is empty.");

        return _items[0];
    }
}