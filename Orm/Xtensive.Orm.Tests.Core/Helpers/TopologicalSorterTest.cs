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
using Xtensive.Sorting;
using Xtensive.Orm.Tests;

namespace Xtensive.Orm.Tests.Core.Helpers
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

    private static void InternalPerformanceTest(int nodeCount, int averageConnectionCount, bool allowLoops)
    {
      TestLog.Info("Building graph: {0} nodes, {1} connections/node in average.", nodeCount, averageConnectionCount);
      var rnd = new Random();
      var nodes = new List<Node<int, int>>();
      for (var i = 0; i < nodeCount; i++) {
        nodes.Add(new Node<int, int>(i));
      }

      int connectionCount = 0;
      foreach (var from in nodes) {
        var outgoingConnectionCount = rnd.Next(averageConnectionCount);
        for (var i = 0; i < outgoingConnectionCount; i++) {
          var to = nodes[rnd.Next(allowLoops ? nodeCount : @from.Item)];
          if (from == to)
            continue;
          var c = new NodeConnection<int, int>(@from, to, connectionCount++);
          c.BindToNodes();
        }
      }

      _ = GC.GetTotalMemory(true);
      using (new Measurement("Sorting", nodeCount + connectionCount)) {
        var result = TopologicalSorter.Sort(nodes, out var removedEdges);
        if (!allowLoops)
          Assert.AreEqual(nodeCount, result.Count);
      }
      _ = GC.GetTotalMemory(true);
    }

    [Test]
    public void SelfReferenceTest()
    {
      var node = new Node<int, string>(1);
      var connection = new NodeConnection<int, string>(node, node, "ConnectionItem");
      connection.BindToNodes();

      var result = TopologicalSorter.Sort(EnumerableUtils.One(node), out var removedEdges);
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual(node.Item, result[0]);
      Assert.AreEqual(1, removedEdges.Count);
      Assert.AreEqual(connection, removedEdges[0]);
    }

    [Test]
    public void NullNodeCollectionTest()
    {
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.Sort((IEnumerable<Node<int, string>>) null, out var removedEdges1));
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.Sort((IEnumerable<Node<int, string>>) null, out var removedEdges2, false));
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.Sort((IEnumerable<Node<int, string>>) null, out var removedEdges3, true));
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.Sort((List<Node<int, int>>) null, out var removedEdges4));
    }

    [Test]
    public void EmptyNodeCollectionTest()
    {
      _ = TopologicalSorter.Sort(Enumerable.Empty<Node<int, string>>(), out var removedEdges1);
      _ = TopologicalSorter.Sort(Enumerable.Empty<Node<int, string>>(), out var removedEdges2, false);
      _ = TopologicalSorter.Sort(Enumerable.Empty<Node<int, string>>(), out var removedEdges3, true);
      _ = TopologicalSorter.Sort(new List<Node<int, int>>(), out var removedEdges4);
    }

    [Test]
    public void FullCircleTest()
    {
      var nodes = new List<Node<int, int>>();
      for (var i = 0; i < 3; i++) {
        nodes.Add(new Node<int, int>(i));
      }

      var c = new NodeConnection<int, int>(nodes[0], nodes[1], 1);
      c.BindToNodes();
      c = new NodeConnection<int, int>(nodes[1], nodes[2], 2);
      c.BindToNodes();
      c = new NodeConnection<int, int>(nodes[2], nodes[0], 3);
      c.BindToNodes();

      var result = TopologicalSorter.Sort(nodes, out var removedEdges);
      Assert.That(result, Is.Null);
    }

    [Test]
    public void RemoveWholeNodeTest()
    {
      var node1 = new Node<int, string>(1);
      var node2 = new Node<int, string>(2);
      var connection12_1 = new NodeConnection<int, string>(node1, node2, "ConnectionItem 1->2 1");
      connection12_1.BindToNodes();
      var connection12_2 = new NodeConnection<int, string>(node1, node2, "ConnectionItem 1->2 2");
      connection12_2.BindToNodes();
      var connection21_1 = new NodeConnection<int, string>(node2, node1, "ConnectionItem 2->1 1");
      connection21_1.BindToNodes();

      // Remove edge by edge.

      var result = TopologicalSorter.Sort(new[] { node2, node1 }, out var removedEdges);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(node1.Item, result[0]);
      Assert.AreEqual(node2.Item, result[1]);

      Assert.AreEqual(1, removedEdges.Count);
      Assert.AreEqual(connection21_1, removedEdges[0]);

      // Remove whole node
      connection12_1.BindToNodes();
      connection12_2.BindToNodes();
      connection21_1.BindToNodes();

      result = TopologicalSorter.Sort(new[] { node2, node1 }, out removedEdges, true);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(node1.Item, result[1]);
      Assert.AreEqual(node2.Item, result[0]);

      Assert.AreEqual(2, removedEdges.Count);
      Assert.AreEqual(0, removedEdges.Except(new[] { connection12_1, connection12_2 }).Count());
    }

    [Test]
    public void CombinedTest()
    {
      TestSortLoopsCheck(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), null, new[] { 4, 2, 1 });
      TestSortLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, null);
      TestSortLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => true, null, new[] { 1, 2, 3 });
      TestSortLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, null);
      TestSortLoopsCheck(Array.Empty<int>(), (i1, i2) => true, Array.Empty<int>(), null);
      TestSortLoopsCheck(Array.Empty<int>(), (i1, i2) => false, Array.Empty<int>(), null);
      _ = Assert.Throws<ArgumentNullException>(() => TestSortLoopsCheck<int>(null, (i1, i2) => true, null, null));
      _ = Assert.Throws<ArgumentNullException>(() => TestSortLoopsCheck<int>(null, (i1, i2) => false, null, null));

      TestSortNoLoopsCheck(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), null);
      TestSortNoLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 });
      TestSortNoLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => true, null);
      TestSortNoLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 });
      TestSortNoLoopsCheck(Array.Empty<int>(), (i1, i2) => true, Array.Empty<int>());
      TestSortNoLoopsCheck(Array.Empty<int>(), (i1, i2) => false, Array.Empty<int>());
      _ = Assert.Throws<ArgumentNullException>(() => TestSortNoLoopsCheck<int>(null, (i1, i2) => true, null));
      _ = Assert.Throws<ArgumentNullException>(() => TestSortNoLoopsCheck<int>(null, (i1, i2) => false, null));

      TestEdgeRemoval(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), new[] { 3, 1, 2, 4 }, new[] { (4, 2), (4, 1), (2, 1) });
      TestEdgeRemoval(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, null);
      TestEdgeRemoval(new[] { 3, 2, 1 }, (i1, i2) => true, new[] { 1, 2, 3 }, new[] { (3, 2), (2, 1), (3, 1) });
      TestEdgeRemoval(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, null);

      TestEdgeRemovalWithNode(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), new[] { 3, 1, 2, 4 }, new[] { (4, 2), (4, 1), (2, 1) });
      TestEdgeRemovalWithNode(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, null);
      TestEdgeRemovalWithNode(new[] { 3, 2, 1 }, (i1, i2) => true, new[] { 1, 2, 3 }, new[] { (3, 2), (2, 1), (3, 1) });
      TestEdgeRemovalWithNode(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, null);
    }

    private void TestSortLoopsCheck<T>(T[] data, Predicate<T, T> connector, T[] expected, T[] loops)
    {
      var actual = TopologicalSorter.Sort(data, connector, out List<Node<T, object>> actualLoopNodes);

      if (expected == null) {
        Assert.That(actual, Is.Null);
      }
      else if(data.Length==0) {
        Assert.That(actual, Is.Empty);
      }
      else {
        Assert.That(expected.SequenceEqual(actual));
      }

      var actualLoops = actualLoopNodes != null
        ? actualLoopNodes
            .Where(n => n.OutgoingConnectionCount != 0)
            .Select(n => n.Item)
            .ToArray()
        : null;

      AssertEx.HasSameElements(loops, actualLoops);

      var sortWithRemove = TopologicalSorter.Sort(data, connector, out List<NodeConnection<T, object>> removedEdges);
      Assert.AreEqual(sortWithRemove.Count, data.Length);

      if (loops == null) {
        Assert.AreEqual(sortWithRemove.Count, actual.Count);
        for (var i = 0; i < actual.Count; i++) {
          Assert.AreEqual(sortWithRemove[i], actual[i]);
        }
      }
      else {
        TestLog.Debug("Loops detected");
      }
    }

    private void TestSortNoLoopsCheck<T>(T[] data, Predicate<T, T> connector, T[] expected)
    {
      List<Node<T, object>> actualLoopNodes;
      var actual = TopologicalSorter.Sort(data, connector);

      if (expected == null) {
        Assert.That(actual, Is.Null);
      }
      else if (data.Length == 0) {
        Assert.That(actual, Is.Empty);
      }
      else {
        Assert.That(expected.SequenceEqual(actual));
      }

      var sortWithRemove = TopologicalSorter.Sort(data, connector, out List<NodeConnection<T, object>> _);
      Assert.AreEqual(sortWithRemove.Count, data.Length);
    }

    private void TestEdgeRemoval<T>(T[] data, Predicate<T, T> connector, T[] expected, (T source, T target)[] expectedRemovedEdges)
    {
      var sortWithRemove = TopologicalSorter.Sort(data, connector, out List<NodeConnection<T, object>> removedEdges);
      Assert.That(sortWithRemove, Is.Not.Null);
      Assert.AreEqual(sortWithRemove.Count, data.Length);
      Assert.That(sortWithRemove.SequenceEqual(expected), Is.True);

      if (expectedRemovedEdges == null) {
        Assert.That(removedEdges, Is.Empty);
      }

      foreach (var removedEdge in removedEdges) {
        var s = removedEdge.Source.Item;
        var t = removedEdge.Destination.Item;
        (T source, T target) expectedTuple = (s, t);
        Assert.That(expectedRemovedEdges.Contains(expectedTuple), Is.True, $"({s} -> {t}) is not represented in expected edges");
      }
    }

    private void TestEdgeRemovalWithNode<T>(T[] data, Predicate<T, T> connector, T[] expected, (T source, T target)[] expectedRemovedEdges)
    {
      var sortWithRemove = TopologicalSorter.Sort(data, connector, out var removedEdges, true);
      Assert.That(sortWithRemove, Is.Not.Null);
      Assert.AreEqual(sortWithRemove.Count, data.Length);
      Assert.That(sortWithRemove.SequenceEqual(expected), Is.True);

      if (expectedRemovedEdges == null) {
        Assert.That(removedEdges, Is.Empty);
      }

      foreach (var removedEdge in removedEdges) {
        var s = removedEdge.Source.Item;
        var t = removedEdge.Destination.Item;
        (T source, T target) expectedTuple = (s, t);
        Assert.That(expectedRemovedEdges.Contains(expectedTuple), Is.True, $"({s} -> {t}) is not represented in expected edges");
      }
    }
  }
}