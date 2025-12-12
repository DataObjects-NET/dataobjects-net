// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.08

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Collections.Graphs;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Collections
{
    [TestFixture]
    public class TopologicalSorterTest
    {
        [Test, Explicit]
        public void PerformanceTest()
        {
            using (TestLog.InfoRegion("No loops")) {
                InternalPerformanceTest(10000, 10, false);
                InternalPerformanceTest(100, 10, false);
                InternalPerformanceTest(1000, 10, false);
                InternalPerformanceTest(10000, 10, false);
                InternalPerformanceTest(100000, 10, false);
            }
            TestLog.Info("");
            using (TestLog.InfoRegion("With loop removal")) {
                InternalPerformanceTest(10000, 10, true);
                InternalPerformanceTest(100, 10, true);
                InternalPerformanceTest(1000, 10, true);
                InternalPerformanceTest(10000, 10, true);
                InternalPerformanceTest(100000, 10, true);
            }
        }

        private static void InternalPerformanceTest(int nodeCount, int averageEdgeCount, bool allowLoops)
        {
            TestLog.Info($"Building graph: {nodeCount} nodes, {averageEdgeCount} edges/node in average.");
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
        Assert.That(result.HasLoops, Is.False);
            }
            GC.GetTotalMemory(true);
        }

        [Test]
        public void SelfReferenceTest()
        {
            var node = new Node<int>(1);
            var edge = new Edge<int>(node, node, 1);
            var graph = new Graph<Node<int>, Edge<int>>(EnumerableUtils.One(node));

            var result = TopologicalSorter.Sort(graph, e => e.Source==e.Target);
      Assert.That(result.SortedNodes.Count, Is.EqualTo(1));
      Assert.That(result.SortedNodes[0], Is.SameAs(node));
      Assert.That(result.BrokenEdges.Count, Is.EqualTo(1));
      Assert.That(result.BrokenEdges[0], Is.SameAs(edge));
      Assert.That(node.Edges.Count(), Is.EqualTo(0));
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
      Assert.That(result.SortedNodes.Count, Is.EqualTo(2));
      Assert.That(result.BrokenEdges.Count, Is.EqualTo(1));
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
      Assert.That(result.SortedNodes.Count, Is.EqualTo(2));
      Assert.That(result.BrokenEdges.Count, Is.EqualTo(2));
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

            AssertEx.HasSameElements(expected, actual);
            AssertEx.HasSameElements(loops, actualLoops);
      Assert.That(result.BrokenEdges.Count, Is.EqualTo(selfReferencingEdgeCount));

            // With edge breaker (any edge can be broken)
            nodes = nodeValues.Select(v => new Node<T>(v)).ToList();
            graph = new Graph<Node<T>, Edge>(nodes);
            graph.AddEdges((s,t) => connector(s.Value, t.Value) ? new Edge(s,t) : null);
            
            result = TopologicalSorter.Sort(graph, e => true);
            actual = result.SortedNodes.Select(n => n.Value).ToArray();
            actualLoops = result.LoopNodes.Select(n => n.Value).ToArray();

      Assert.That(actual.Length, Is.EqualTo(nodeValues.Length));
      Assert.That(actualLoops.Length, Is.EqualTo(0));
            if (loops.Length==0)
        Assert.That(result.BrokenEdges.Count, Is.EqualTo(selfReferencingEdgeCount));
            else
        Assert.That(result.BrokenEdges.Count, Is.Not.EqualTo(selfReferencingEdgeCount));
        }
    }
}