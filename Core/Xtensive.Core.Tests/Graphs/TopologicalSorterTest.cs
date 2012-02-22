// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.08

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Diagnostics;
using Xtensive.Graphs;
using Xtensive.Testing;

namespace Xtensive.Tests.Graphs
{
    [TestFixture]
    public class TopologicalSorterTest
    {
        [Test, Explicit]
        public void PerformanceTest()
        {
            using (Log.InfoRegion("No loops")) {
                InternalPerformanceTest(100, 10, false);
                InternalPerformanceTest(1000, 10, false);
                InternalPerformanceTest(10000, 10, false);
                InternalPerformanceTest(100000, 10, false);
            }
            Log.Info("");
            using (Log.InfoRegion("With loop removal")) {
                InternalPerformanceTest(100, 10, true);
                InternalPerformanceTest(1000, 10, true);
                InternalPerformanceTest(10000, 10, true);
                InternalPerformanceTest(100000, 10, true);
            }
        }

        private static void InternalPerformanceTest(int nodeCount, int averageEdgeCount, bool allowLoops)
        {
            Log.Info("Building graph: {0} nodes, {1} edges/node in average.", nodeCount, averageEdgeCount);
            var rnd = new Random();
            var nodes = new List<Node<int>>();
            for (int i = 0; i < nodeCount; i++)
                nodes.Add(new Node<int>(i));
            int edgeCount = 0;
            foreach (var from in nodes) {
                int outgoingEdgeCount = rnd.Next(averageEdgeCount);
                for (int i = 0; i < outgoingEdgeCount; i++) {
                    var to = nodes[rnd.Next(allowLoops ? nodeCount : @from.Value)];
                    if (from==to)
                        continue;
                    var c = new Edge<int>(@from, to, edgeCount++);
                }
            }
            var graph = new Graph<Node<int>, Edge<int>>(nodes);

            GC.GetTotalMemory(true);
            using (new Measurement("Sorting", nodeCount + edgeCount)) {
                var result = TopologicalSorter.Sort(graph, e => true);
                Assert.IsFalse(result.HasLoops);
            }
        }

        [Test]
        public void SelfReferenceTest()
        {
            var node = new Node<int>(1);
            var edge = new Edge<int>(node, node, 1);
            var graph = new Graph<Node<int>, Edge<int>>(EnumerableUtils.One(node));

            List<Edge<int>> removedEdges;
            var result = TopologicalSorter.Sort(graph, e => e.Source==e.Target);
            Assert.AreEqual(1, result.SortedNodes.Count);
            Assert.AreSame(node, result.SortedNodes[0]);
            Assert.AreEqual(1, result.BrokenEdges.Count);
            Assert.AreSame(edge, result.BrokenEdges[0]);
            Assert.AreEqual(0, node.Edges.Count());
        }

        [Test]
        public void RemoveEdgeTest1()
        {
            var node1 = new Node<int>(1);
            var node2 = new Node<int>(2);
            var connection12_1 = new Edge<int>(node1, node2, 1);
            var connection12_2 = new Edge<int>(node1, node2, 2);
            var connection21_1 = new Edge<int>(node2, node1, 3);
            var graph = new Graph<Node<int>, Edge<int>>(new[] {node2, node1});

            var result = TopologicalSorter.Sort(graph, e => e.Value==3);
            Assert.AreEqual(2, result.SortedNodes.Count);
            Assert.AreEqual(1, result.BrokenEdges.Count);
        }

        [Test]
        public void RemoveEdgeTest2()
        {
            var node1 = new Node<int>(1);
            var node2 = new Node<int>(2);
            var connection12_1 = new Edge<int>(node1, node2, 1);
            var connection12_2 = new Edge<int>(node1, node2, 2);
            var connection21_1 = new Edge<int>(node2, node1, 3);
            var graph = new Graph<Node<int>, Edge<int>>(new[] {node2, node1});

            var result = TopologicalSorter.Sort(graph, e => e.Value!=3);
            Assert.AreEqual(2, result.SortedNodes.Count);
            Assert.AreEqual(2, result.BrokenEdges.Count);
        }

        [Test]
        public void CombinedTest()
        {
            TestSort(new[] {4, 3, 2, 1}, 
                (i1, i2) => !(i1 == 3 || i2 == 3), 
                new [] {3}, 
                new[] {4, 2, 1});
            TestSort(new[] {3, 2, 1}, 
                (i1, i2) => i1 >= i2, 
                new[] {1, 2, 3}, 
                new int[0]);
            TestSort(new[] {3, 2, 1}, 
                (i1, i2) => true, 
                new int[0], 
                new[] {1, 2, 3});
            TestSort(new[] {3, 2, 1}, 
                (i1, i2) => false, 
                new[] {3, 2, 1}, 
                new int[0]);
        }

        private void TestSort<T>(T[] nodeValues, Predicate<T, T> connector, T[] expected, T[] loops)
        {
            // WIth edge breaker, that breaks only self-referencing edges
            var nodes = nodeValues.Select(v => new Node<T>(v)).ToList();
            var graph = new Graph<Node<T>, Edge>(nodes);
            graph.AddEdges((s,t) => connector(s.Value, t.Value) ? new Edge(s,t) : null);
            int selfReferencingEdgeCount = graph.Edges.Count(e => e.Source == e.Target);

            var result = TopologicalSorter.Sort(graph, e => e.Source==e.Target);
            var actual = result.SortedNodes.Select(n => n.Value).ToArray();
            var actualLoops = result.LoopNodes.Select(n => n.Value).ToArray();

            AssertEx.AreEqual(expected, actual);
            AssertEx.AreEqual(loops, actualLoops);
            Assert.AreEqual(selfReferencingEdgeCount, result.BrokenEdges.Count);

            // With edge breaker (any edge can be broken)
            nodes = nodeValues.Select(v => new Node<T>(v)).ToList();
            graph = new Graph<Node<T>, Edge>(nodes);
            graph.AddEdges((s,t) => connector(s.Value, t.Value) ? new Edge(s,t) : null);
            
            result = TopologicalSorter.Sort(graph, e => true);
            actual = result.SortedNodes.Select(n => n.Value).ToArray();
            actualLoops = result.LoopNodes.Select(n => n.Value).ToArray();

            Assert.AreEqual(nodeValues.Length, actual.Length);
            Assert.AreEqual(0, actualLoops.Length);
            if (loops.Length==0) 
                Assert.AreEqual(selfReferencingEdgeCount, result.BrokenEdges.Count);
            else 
                Assert.AreNotEqual(selfReferencingEdgeCount, result.BrokenEdges.Count);
        }
    }
}