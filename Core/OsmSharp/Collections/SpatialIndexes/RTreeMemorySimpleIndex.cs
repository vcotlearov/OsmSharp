﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using OsmSharp.Math.Primitives;

namespace OsmSharp.Collections.SpatialIndexes
{
    /// <summary>
    /// R-tree implementation of a spatial index.
    /// http://en.wikipedia.org/wiki/R-tree
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RTreeMemorySimpleIndex<T> : ISpatialIndex<T>
    {
        /// <summary>
        /// Holds the root node.
        /// </summary>
        private Node _root;

        /// <summary>
        /// Holds the maximum leaf size M.
        /// </summary>
        private readonly int _maxLeafSize = 50;

        /// <summary>
        /// Holds the minimum leaf size m.
        /// </summary>
        private readonly int _minLeafSize = 20;

        /// <summary>
        /// Creates a new index.
        /// </summary>
        public RTreeMemorySimpleIndex()
        {
            
        }

        /// <summary>
        /// Creates a new index.
        /// </summary>
        /// <param name="minLeafSize"></param>
        /// <param name="maxLeafSize"></param>
        public RTreeMemorySimpleIndex(int minLeafSize, int maxLeafSize)
        {
            _minLeafSize = minLeafSize;
            _maxLeafSize = maxLeafSize;
        }

        /// <summary>
        /// Adds a new item with the corresponding box.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="item"></param>
        public void Add(RectangleF2D box, T item)
        {
            if (_root == null)
            { // create the root.
                _root = new Node();
                _root.Boxes = new List<RectangleF2D>();
                _root.Children = new List<T>();
            }

            // add new data.
            Node leaf = RTreeMemorySimpleIndex<T>.ChooseLeaf(_root, box);
            Node newRoot = RTreeMemorySimpleIndex<T>.Add(leaf, box, item, _minLeafSize, _maxLeafSize);
            if (newRoot != null)
            { // there should be a new root.
                _root = newRoot;
            }
        }

        /// <summary>
        /// Removes the given item.
        /// </summary>
        /// <param name="item"></param>
        public void Remove(T item)
        {

        }

        /// <summary>
        /// Queries this index and returns all objects with overlapping bounding boxes.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public IEnumerable<T> Get(RectangleF2D box)
        {
            var result = new HashSet<T>();
            RTreeMemorySimpleIndex<T>.Get(_root, box, result);
            return result;
        }

        #region Tree Structure 

        /// <summary>
        /// Represents a simple node.
        /// </summary>
        private class Node
        {
            /// <summary>
            /// Gets or sets boxes.
            /// </summary>
            public List<RectangleF2D> Boxes { get; set; }

            /// <summary>
            /// Gets or sets the children.
            /// </summary>
            public IList Children { get; set; }

            /// <summary>
            /// Gets or sets the parent.
            /// </summary>
            public Node Parent { get; set; }

            /// <summary>
            /// Returns the bounding box for this node.
            /// </summary>
            /// <returns></returns>
            public RectangleF2D GetBox()
            {
                RectangleF2D box = this.Boxes[0];
                for (int idx = 1; idx < this.Boxes.Count; idx++)
                {
                    box = box.Union(this.Boxes[idx]);
                }
                return box;
            }
        }

        #region Tree Operations

        /// <summary>
        /// Fills the collection with data.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="box"></param>
        /// <param name="result"></param>
        private static void Get(Node node, RectangleF2D box, HashSet<T> result)
        {
            if (node.Children is List<Node>)
            {
                var children = (node.Children as List<Node>);
                for (int idx = 0; idx < children.Count; idx++)
                {
                    if (box.Overlaps(node.Boxes[idx]))
                    {
                        if (box.IsInside(node.Boxes[idx]))
                        { // add all the data from the child.
                            RTreeMemorySimpleIndex<T>.GetAll(children[idx],
                                result);
                        }
                        else
                        { // add the data from the child.
                            RTreeMemorySimpleIndex<T>.Get(children[idx],
                                box, result);
                        }
                    }
                }
            }
            else
            {
                var children = (node.Children as List<T>);
                if (children != null)
                { // the children are of the data type.
                    for (int idx = 0; idx < node.Children.Count; idx++)
                    {
                        if (node.Boxes[idx].Overlaps(box))
                        {
                            result.Add(children[idx]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Fills the collection with data.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="result"></param>
        private static void GetAll(Node node, HashSet<T> result)
        {
            if (node.Children is List<Node>)
            {
                var children = (node.Children as List<Node>);
                for (int idx = 0; idx < children.Count; idx++)
                {
                    // add all the data from the child.
                    RTreeMemorySimpleIndex<T>.GetAll(children[idx],
                                                     result);
                }
            }
            else
            {
                var children = (node.Children as List<T>);
                if (children != null)
                { // the children are of the data type.
                    for (int idx = 0; idx < node.Children.Count; idx++)
                    {
                        result.Add(children[idx]);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the given item to the given box.
        /// </summary>
        /// <param name="leaf"></param>
        /// <param name="box"></param>
        /// <param name="item"></param>
        /// <param name="minimumSize"></param>
        /// <param name="maximumSize"></param>
        private static Node Add(Node leaf, RectangleF2D box, T item, int minimumSize, int maximumSize)
        {
            if (box == null) throw new ArgumentNullException("box");
            if (leaf == null) throw new ArgumentNullException("leaf");

            Node ll = null;
            if (leaf.Boxes.Count == maximumSize)
            { // split the node.
                // add the child.
                leaf.Boxes.Add(box);
                leaf.Children.Add(item);

                Node[] split = RTreeMemorySimpleIndex<T>.SplitNode(leaf, minimumSize);
                leaf.Boxes = split[0].Boxes;
                leaf.Children = split[0].Children;
                RTreeMemorySimpleIndex<T>.SetParents(leaf);
                ll = split[1];
            }
            else
            {
                // add the child.
                leaf.Boxes.Add(box);
                leaf.Children.Add(item);
            }

            // adjust the tree.
            Node n = leaf;
            Node nn = ll;
            while (n.Parent != null)
            { // keep going until the root is reached.
                Node p = n.Parent;
                RTreeMemorySimpleIndex<T>.TightenFor(p, n); // tighten the parent box around n.

                if (nn != null)
                { // propagate split if needed.
                    if (p.Boxes.Count == maximumSize)
                    { // parent needs to be split.
                        p.Boxes.Add(nn.GetBox());
                        p.Children.Add(nn);
                        Node[] split = RTreeMemorySimpleIndex<T>.SplitNode(
                            p, minimumSize);
                        p.Boxes = split[0].Boxes;
                        p.Children = split[0].Children;
                        RTreeMemorySimpleIndex<T>.SetParents(p);
                        nn = split[1];
                    }
                    else
                    { // add the other 'split' node.
                        p.Boxes.Add(nn.GetBox());
                        p.Children.Add(nn);
                        nn.Parent = p;
                        nn = null;
                    }
                }
                n = p;
            }
            if (nn != null)
            { // create a new root node and 
                var root = new Node();
                root.Boxes = new List<RectangleF2D>();
                root.Boxes.Add(n.GetBox());
                root.Boxes.Add(nn.GetBox());
                root.Children = new List<Node>();
                root.Children.Add(n);
                n.Parent = root;
                root.Children.Add(nn);
                nn.Parent = root;
                return root;
            }
            return null; // no new root node needed.
        }

        /// <summary>
        /// Tightens the box for the given node in the given parent.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        private static void TightenFor(Node parent, Node child)
        {
            for (int idx = 0; idx < parent.Children.Count; idx++)
            {
                if (parent.Children[idx] == child)
                {
                    parent.Boxes[idx] = child.GetBox();
                }
            }
        }

        /// <summary>
        /// Choose the child to best place the given box.
        /// </summary>
        /// <param name="box"></param>
        /// <param name="node"></param>
        /// <returns></returns>
        private static Node ChooseLeaf(Node node, RectangleF2D box)
        {
            if (box == null) throw new ArgumentNullException("box");
            if (node == null) throw new ArgumentNullException("node");

            // keep looping until a leaf is found.
            while (node.Children is List<Node>)
            { // choose the best leaf.
                Node bestChild = null;
                RectangleF2D bestBox = null;
                double bestIncrease = double.MaxValue;
                var children = node.Children as List<Node>; // cast just once.
                for (int idx = 0; idx < node.Boxes.Count; idx++)
                {
                    RectangleF2D union = node.Boxes[idx].Union(box);
                    double increase = union.Surface - node.Boxes[idx].Surface; // calculates the increase.
                    if (bestIncrease > increase)
                    {
                        // the increase for this child is smaller.
                        bestIncrease = increase;
                        bestChild = children[idx];
                        bestBox = node.Boxes[idx];
                    }
                    else if (bestBox != null &&
                             bestIncrease == increase)
                    {
                        // the increase is indentical, choose the smalles child.
                        if (node.Boxes[idx].Surface < bestBox.Surface)
                        {
                            bestChild = children[idx];
                            bestBox = node.Boxes[idx];
                        }
                    }
                }
                if (bestChild == null)
                {
                    throw new Exception("Finding best child failed!");
                }
                node = bestChild;
            }
            return node;
        }

        /// <summary>
        /// Sets all the parent properties of the children of the given node.
        /// </summary>
        /// <param name="node"></param>
        private static void SetParents(Node node)
        {
            if (node.Children is List<Node>)
            {
                var children = (node.Children as List<Node>);
                for (int idx = 0; idx < node.Boxes.Count; idx++)
                {
                    children[idx].Parent = node;
                }
            }
        }

        /// <summary>
        /// Splits the given node in two other nodes.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="minimumSize"></param>
        /// <returns></returns>
        private static Node[] SplitNode(Node node, int minimumSize)
        {
            bool leaf = (node.Children is List<T>);

            // create the target nodes.
            var nodes = new Node[2];
            nodes[0] = new Node();
            nodes[0].Boxes = new List<RectangleF2D>();
            if (leaf)
            {
                nodes[0].Children = new List<T>();
            }
            else
            {
                nodes[0].Children = new List<Node>();
            }
            nodes[1] = new Node();
            nodes[1].Boxes = new List<RectangleF2D>();
            if (leaf)
            {
                nodes[1].Children = new List<T>();
            }
            else
            {
                nodes[1].Children = new List<Node>();
            }

            // select the seed boxes.
            int[] seeds = RTreeMemorySimpleIndex<T>.SelectSeeds(node.Boxes);

            // add the boxes.
            nodes[0].Boxes.Add(node.Boxes[seeds[0]]);
            nodes[1].Boxes.Add(node.Boxes[seeds[1]]);
            nodes[0].Children.Add(node.Children[seeds[0]]);
            nodes[1].Children.Add(node.Children[seeds[1]]);

            // create the boxes.
            var boxes = new RectangleF2D[2]
                            {
                                node.Boxes[seeds[0]], node.Boxes[seeds[1]]
                            };
            node.Boxes.RemoveAt(seeds[0]); // seeds[1] is always < seeds[0].
            node.Boxes.RemoveAt(seeds[1]);
            node.Children.RemoveAt(seeds[0]);
            node.Children.RemoveAt(seeds[1]);

            while (node.Boxes.Count > 0)
            {
                // check if one of them needs em all!
                if (nodes[0].Boxes.Count + node.Boxes.Count == minimumSize)
                { // all remaining boxes need te be assigned here.
                    for (int idx = 0; node.Boxes.Count > 0; idx++)
                    {
                        boxes[0] = boxes[0].Union(node.Boxes[0]);
                        nodes[0].Boxes.Add(node.Boxes[0]);
                        nodes[0].Children.Add(node.Children[0]);

                        node.Boxes.RemoveAt(0);
                        node.Children.RemoveAt(0);
                    }
                }
                else if (nodes[1].Boxes.Count + node.Boxes.Count == minimumSize)
                { // all remaining boxes need te be assigned here.
                    for (int idx = 0; node.Boxes.Count > 0; idx++)
                    {
                        boxes[1] = boxes[1].Union(node.Boxes[0]);
                        nodes[1].Boxes.Add(node.Boxes[0]);
                        nodes[1].Children.Add(node.Children[0]);

                        node.Boxes.RemoveAt(0);
                        node.Children.RemoveAt(0);
                    }
                }
                else
                { // choose one of the leaves.
                    int leafIdx;
                    int nextId = RTreeMemorySimpleIndex<T>.PickNext(boxes, node.Boxes, out leafIdx);

                    boxes[leafIdx] = boxes[leafIdx].Union(node.Boxes[nextId]);

                    nodes[leafIdx].Boxes.Add(node.Boxes[nextId]);
                    nodes[leafIdx].Children.Add(node.Children[nextId]);

                    node.Boxes.RemoveAt(nextId);
                    node.Children.RemoveAt(nextId);
                }
            }

            RTreeMemorySimpleIndex<T>.SetParents(nodes[0]);
            RTreeMemorySimpleIndex<T>.SetParents(nodes[1]);

            return nodes;
        }

        /// <summary>
        /// Picks the next best box to add to one of the given nodes.
        /// </summary>
        /// <param name="nodeBoxes"></param>
        /// <param name="boxes"></param>
        /// <param name="nodeBoxIndex"></param>
        /// <returns></returns>
        protected static int PickNext(RectangleF2D[] nodeBoxes, IList<RectangleF2D> boxes, out int nodeBoxIndex)
        {
            double difference = double.MinValue;
            nodeBoxIndex = 0;
            int pickedIdx = -1;
            for (int idx = 0; idx < boxes.Count; idx++)
            {
                RectangleF2D item = boxes[idx];
                double d1 = item.Union(nodeBoxes[0]).Surface -
                            item.Surface;
                double d2 = item.Union(nodeBoxes[1]).Surface -
                            item.Surface;

                double localDifference = System.Math.Abs(d1 - d2);
                if (difference < localDifference)
                {
                    difference = localDifference;
                    if (d1 == d2)
                    {
                        nodeBoxIndex = (nodeBoxes[0].Surface < nodeBoxes[1].Surface) ? 0 : 1;
                    }
                    else
                    {
                        nodeBoxIndex = (d1 < d2) ? 0 : 1;
                    }
                    pickedIdx = idx;
                }
            }
            return pickedIdx;
        }

        /// <summary>
        /// Selects the best seed boxes to start splitting a node.
        /// </summary>
        /// <param name="boxes"></param>
        /// <returns></returns>
        private static int[] SelectSeeds(List<RectangleF2D> boxes)
        {
            if (boxes == null) throw new ArgumentNullException("boxes");
            if (boxes.Count < 2) throw new ArgumentException("Cannot select seeds from a list with less than two items.");

            // the Quadratic Split version: selecting the two items that waste the most space
            // if put together.

            var seeds = new int[2];
            double loss = double.MinValue;
            for (int idx1 = 0; idx1 < boxes.Count; idx1++)
            {
                for (int idx2 = 0; idx2 < idx1; idx2++)
                {
                    double localLoss = System.Math.Max(boxes[idx1].Union(boxes[idx2]).Surface -
                        boxes[idx1].Surface - boxes[idx2].Surface, 0);
                    if (localLoss > loss)
                    {
                        loss = localLoss;
                        seeds[0] = idx1;
                        seeds[1] = idx2;
                    }
                }
            }

            return seeds;
        }

        #endregion

        #endregion
    }
}