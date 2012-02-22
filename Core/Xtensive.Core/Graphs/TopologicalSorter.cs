﻿// Copyright (C) 2003-2012 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2012.02.22

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Xtensive.Collections;

namespace Xtensive.Graphs
{
    /// <summary>
    /// Topological sorter.
    /// </summary>
    public static class TopologicalSorter
    {
        #region Nested type: OrderedSet<T>

        private sealed class OrderedSet<T> : IEnumerable<T>
        {
            private readonly System.Collections.Generic.LinkedList<T> list;
            private readonly Dictionary<T, LinkedListNode<T>> map;

            public int Count
            {
                get { return list.Count; }
            }

            public T Pop()
            {
                var node = list.First;
                if (node == null)
                    return default(T);
                list.RemoveFirst();
                map.Remove(node.Value);
                return node.Value;
            }

            public void Remove(T item)
            {
                LinkedListNode<T> node;
                if (map.TryGetValue(item, out node)) {
                    list.Remove(node);
                    map.Remove(item);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return list.GetEnumerator();
            }

            public OrderedSet(IEnumerable<T> source)
            {
                list = new System.Collections.Generic.LinkedList<T>();
                map = new Dictionary<T, LinkedListNode<T>>();
                foreach (var item in source) {
                    var node = list.AddLast(item);
                    map.Add(item, node);
                }
            }
        }

        #endregion

        /// <summary>
        /// Sorts the <see cref="graph"/> in topological order (nodes without incoming edges go first).
        /// <note>
        /// This method modifies the <paramref name="graph"/> by removing all non-loop edges from it!
        /// </note>
        /// </summary>
        /// <returns>Sorting result.</returns>
        public static TopologicalSortResult<TNode, TEdge> Sort<TNode, TEdge>(Graph<TNode, TEdge> graph, Predicate<TEdge> edgeBreaker = null)
            where TNode: Node
            where TEdge: Edge
        {
            bool breakEdges = edgeBreaker != null;
            var tmpEdges = new List<TEdge>();
            var result = new TopologicalSortResult<TNode, TEdge>();

            var unsortedNodes = new OrderedSet<TNode>(graph.Nodes);
            var breakableEdges = new OrderedSet<TEdge>(breakEdges ? graph.Edges : EnumerableUtils<TEdge>.Empty);
            
            var nodesWithoutIncomingEdges = new Queue<TNode>(unsortedNodes.Where(n => !n.HasIncomingEdges));

        restart:
            // Sorting
            while (nodesWithoutIncomingEdges.Count!=0) {
                var node = nodesWithoutIncomingEdges.Dequeue();
                unsortedNodes.Remove(node);
                result.SortedNodes.Add(node);
                if (!node.HasOutgoingEdges)
                    continue;
                tmpEdges.AddRange(node.OutgoingEdges.Cast<TEdge>());
                foreach (var edge in tmpEdges) {
                    edge.Detach();
                    if (breakEdges)
                        breakableEdges.Remove(edge);
                    var target = (TNode) edge.Target;
                    if (!target.HasIncomingEdges)
                        nodesWithoutIncomingEdges.Enqueue(target);
                }
                tmpEdges.Clear();
            }

            if (unsortedNodes.Count != 0) {
                // Trying to break edges (collection is always empty when breakEdges==false)
                TEdge edge;
                while ((edge = breakableEdges.Pop())!=null) {
                    if (!edgeBreaker(edge))
                        continue;
                    result.BrokenEdges.Add(edge);
                    edge.Detach();
                    var target = (TNode) edge.Target;
                    if (!target.HasIncomingEdges) {
                        nodesWithoutIncomingEdges.Enqueue(target);
                        goto restart;
                    }
                }
                result.LoopNodes.AddRange(unsortedNodes);
            }

            return result;
        }
    }
}