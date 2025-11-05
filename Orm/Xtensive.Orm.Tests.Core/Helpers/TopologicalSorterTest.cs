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
      using (TestLog.InfoRegion("No loops - SortToList")) {
        InternalListPerformanceTest(10000, 10, false);
        InternalListPerformanceTest(100, 10, false);
        InternalListPerformanceTest(1000, 10, false);
        InternalListPerformanceTest(10000, 10, false);
        InternalListPerformanceTest(100000, 10, false);
      }
      TestLog.Info("");
      using (TestLog.InfoRegion("With loop removal - SortToList")) {
        InternalListPerformanceTest(10000, 10, true);
        InternalListPerformanceTest(100, 10, true);
        InternalListPerformanceTest(1000, 10, true);
        InternalListPerformanceTest(10000, 10, true);
        InternalListPerformanceTest(100000, 10, true);
      }
      TestLog.Info("");
      using (TestLog.InfoRegion("No loops - Sort")) {
        InternalEnumerablePerformanceTest(10000, 10, false);
        InternalEnumerablePerformanceTest(100, 10, false);
        InternalEnumerablePerformanceTest(1000, 10, false);
        InternalEnumerablePerformanceTest(10000, 10, false);
        InternalEnumerablePerformanceTest(100000, 10, false);
      }
      TestLog.Info("");
      using (TestLog.InfoRegion("With loop removal - Sort")) {
        InternalEnumerablePerformanceTest(10000, 10, true);
        InternalEnumerablePerformanceTest(100, 10, true);
        InternalEnumerablePerformanceTest(1000, 10, true);
        InternalEnumerablePerformanceTest(10000, 10, true);
        InternalEnumerablePerformanceTest(100000, 10, true);
      }
    }

    private static void InternalListPerformanceTest(int nodeCount, int averageConnectionCount, bool allowLoops)
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
        var result = TopologicalSorter.SortToList(nodes, out var removedEdges);
        if (!allowLoops)
          Assert.AreEqual(nodeCount, result.Count);
      }
      _ = GC.GetTotalMemory(true);
    }

    private static void InternalEnumerablePerformanceTest(int nodeCount, int averageConnectionCount, bool allowLoops)
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
        var result = TopologicalSorter.Sort(nodes, out _).ToList(nodeCount);
        if (!allowLoops)
          Assert.AreEqual(nodeCount, result.Count);
      }
      _ = GC.GetTotalMemory(true);
    }

    [Test]
    public void SelfReferenceTest()
    {
      var node1 = new Node<int, string>(1);
      var connection1 = new NodeConnection<int, string>(node1, node1, "ConnectionItem");
      connection1.BindToNodes();

      var node2 = new Node<int, string>(1);
      var connection2 = new NodeConnection<int, string>(node2, node2, "ConnectionItem");
      connection2.BindToNodes();

      var result = TopologicalSorter.SortToList(EnumerableUtils.One(node1), out var removedEdges1);
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual(node1.Item, result[0]);
      Assert.AreEqual(1, removedEdges1.Count);
      Assert.AreEqual(connection1, removedEdges1[0]);

      result = TopologicalSorter.Sort(EnumerableUtils.One(node2), out var removedEdges2).ToList(1);
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual(node2.Item, result[0]);
      Assert.AreEqual(1, removedEdges2.Count);
      Assert.AreEqual(connection2, removedEdges2[0]);
    }

    [Test]
    public void NullNodeCollectionTest()
    {
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.Sort((IEnumerable<Node<int, string>>) null, out _));
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.Sort((IEnumerable<Node<int, string>>) null, out _, false));
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.Sort((IEnumerable<Node<int, string>>) null, out _, true));
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.Sort((List<Node<int, int>>) null, out _));

      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.SortToList((IEnumerable<Node<int, string>>) null, out _));
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.SortToList((IEnumerable<Node<int, string>>) null, out _, false));
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.SortToList((IEnumerable<Node<int, string>>) null, out _, true));
      _ = Assert.Throws<ArgumentNullException>(() => TopologicalSorter.SortToList((List<Node<int, int>>) null, out _));
    }

    [Test]
    public void EmptyNodeCollectionTest()
    {
      _ = TopologicalSorter.Sort(Enumerable.Empty<Node<int, string>>(), out _);
      _ = TopologicalSorter.Sort(Enumerable.Empty<Node<int, string>>(), out _, false);
      _ = TopologicalSorter.Sort(Enumerable.Empty<Node<int, string>>(), out _, true);
      _ = TopologicalSorter.Sort(new List<Node<int, int>>(), out _);

      _ = TopologicalSorter.SortToList(Enumerable.Empty<Node<int, string>>(), out _);
      _ = TopologicalSorter.SortToList(Enumerable.Empty<Node<int, string>>(), out _, false);
      _ = TopologicalSorter.SortToList(Enumerable.Empty<Node<int, string>>(), out _, true);
      _ = TopologicalSorter.SortToList(new List<Node<int, int>>(), out _);
    }

    [Test]
    public void FullCircleTest()
    {
      var nodes1 = new List<Node<int, int>>(3);
      var nodes2 = new List<Node<int, int>>(3);
      for (var i = 0; i < 3; i++) {
        nodes1.Add(new Node<int, int>(i));
        nodes2.Add(new Node<int, int>(i));
      }

      var c = new NodeConnection<int, int>(nodes1[0], nodes1[1], 1);
      c.BindToNodes();
      c = new NodeConnection<int, int>(nodes1[1], nodes1[2], 2);
      c.BindToNodes();
      c = new NodeConnection<int, int>(nodes1[2], nodes1[0], 3);
      c.BindToNodes();

      c = new NodeConnection<int, int>(nodes2[0], nodes2[1], 1);
      c.BindToNodes();
      c = new NodeConnection<int, int>(nodes2[1], nodes2[2], 2);
      c.BindToNodes();
      c = new NodeConnection<int, int>(nodes2[2], nodes2[0], 3);
      c.BindToNodes();

      var result1 = TopologicalSorter.Sort(nodes1, out _);
      Assert.That(result1, Is.Null);

      var result2 = TopologicalSorter.Sort(nodes2, out _);
      Assert.That(result1, Is.Null);
    }

    [Test]
    public void RemoveWholeNodeEnumerableTest()
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
      var result = TopologicalSorter.Sort(new[] { node2, node1 }, out var removedEdges).ToList();
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(node1.Item, result[0]);
      Assert.AreEqual(node2.Item, result[1]);

      Assert.AreEqual(1, removedEdges.Count);
      Assert.AreEqual(connection21_1, removedEdges[0]);

      // Remove whole node
      connection12_1.BindToNodes();
      connection12_2.BindToNodes();
      connection21_1.BindToNodes();

      result = TopologicalSorter.Sort(new[] { node2, node1 }, out removedEdges, true).ToList();
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(node1.Item, result[1]);
      Assert.AreEqual(node2.Item, result[0]);

      Assert.AreEqual(2, removedEdges.Count);
      Assert.AreEqual(0, removedEdges.Except(new[] { connection12_1, connection12_2 }).Count());
    }

    [Test]
    public void RemoveWholeNodeToListTest()
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
      var result = TopologicalSorter.SortToList(new[] { node2, node1 }, out var removedEdges);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(node1.Item, result[0]);
      Assert.AreEqual(node2.Item, result[1]);

      Assert.AreEqual(1, removedEdges.Count);
      Assert.AreEqual(connection21_1, removedEdges[0]);

      // Remove whole node
      connection12_1.BindToNodes();
      connection12_2.BindToNodes();
      connection21_1.BindToNodes();

      result = TopologicalSorter.SortToList(new[] { node2, node1 }, out removedEdges, true);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(node1.Item, result[1]);
      Assert.AreEqual(node2.Item, result[0]);

      Assert.AreEqual(2, removedEdges.Count);
      Assert.AreEqual(0, removedEdges.Except(new[] { connection12_1, connection12_2 }).Count());
    }

    [Test]
    public void CombinedTest()
    {
      TestSortLoopsCheck(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), null, new[] { 4, 2, 1 } , true);
      TestSortLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, null, true);
      TestSortLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => true, null, new[] { 1, 2, 3 }, true);
      TestSortLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, null, true);
      TestSortLoopsCheck(Array.Empty<int>(), (i1, i2) => true, Array.Empty<int>(), null, true);
      TestSortLoopsCheck(Array.Empty<int>(), (i1, i2) => false, Array.Empty<int>(), null, true);
      _ = Assert.Throws<ArgumentNullException>(() => TestSortLoopsCheck<int>(null, (i1, i2) => true, null, null, true));
      _ = Assert.Throws<ArgumentNullException>(() => TestSortLoopsCheck<int>(null, (i1, i2) => false, null, null, true));

      TestSortLoopsCheck(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), null, new[] { 4, 2, 1 }, false);
      TestSortLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, null, false);
      TestSortLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => true, null, new[] { 1, 2, 3 }, false);
      TestSortLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, null, false);
      TestSortLoopsCheck(Array.Empty<int>(), (i1, i2) => true, Array.Empty<int>(), null, false);
      TestSortLoopsCheck(Array.Empty<int>(), (i1, i2) => false, Array.Empty<int>(), null, false);
      _ = Assert.Throws<ArgumentNullException>(() => TestSortLoopsCheck<int>(null, (i1, i2) => true, null, null, false));
      _ = Assert.Throws<ArgumentNullException>(() => TestSortLoopsCheck<int>(null, (i1, i2) => false, null, null, false));

      TestSortNoLoopsCheck(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), null, true);
      TestSortNoLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, true);
      TestSortNoLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => true, null, true);
      TestSortNoLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, true);
      TestSortNoLoopsCheck(Array.Empty<int>(), (i1, i2) => true, Array.Empty<int>(), true);
      TestSortNoLoopsCheck(Array.Empty<int>(), (i1, i2) => false, Array.Empty<int>(), true);
      _ = Assert.Throws<ArgumentNullException>(() => TestSortNoLoopsCheck<int>(null, (i1, i2) => true, null, true));
      _ = Assert.Throws<ArgumentNullException>(() => TestSortNoLoopsCheck<int>(null, (i1, i2) => false, null, true));

      TestSortNoLoopsCheck(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), null, true);
      TestSortNoLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, false);
      TestSortNoLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => true, null, false);
      TestSortNoLoopsCheck(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, false);
      TestSortNoLoopsCheck(Array.Empty<int>(), (i1, i2) => true, Array.Empty<int>(), false);
      TestSortNoLoopsCheck(Array.Empty<int>(), (i1, i2) => false, Array.Empty<int>(), false);
      _ = Assert.Throws<ArgumentNullException>(() => TestSortNoLoopsCheck<int>(null, (i1, i2) => true, null, false));
      _ = Assert.Throws<ArgumentNullException>(() => TestSortNoLoopsCheck<int>(null, (i1, i2) => false, null, false));

      TestEdgeRemoval(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), new[] { 3, 1, 2, 4 }, new[] { (4, 2), (4, 1), (2, 1) }, true);
      TestEdgeRemoval(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, null, true);
      TestEdgeRemoval(new[] { 3, 2, 1 }, (i1, i2) => true, new[] { 1, 2, 3 }, new[] { (3, 2), (2, 1), (3, 1) }, true);
      TestEdgeRemoval(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, null, true);

      TestEdgeRemoval(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), new[] { 3, 1, 2, 4 }, new[] { (4, 2), (4, 1), (2, 1) }, false);
      TestEdgeRemoval(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, null, false);
      TestEdgeRemoval(new[] { 3, 2, 1 }, (i1, i2) => true, new[] { 1, 2, 3 }, new[] { (3, 2), (2, 1), (3, 1) }, false);
      TestEdgeRemoval(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, null, false);

      TestEdgeRemovalWithNode(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), new[] { 3, 1, 2, 4 }, new[] { (4, 2), (4, 1), (2, 1) }, true);
      TestEdgeRemovalWithNode(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, null, true);
      TestEdgeRemovalWithNode(new[] { 3, 2, 1 }, (i1, i2) => true, new[] { 1, 2, 3 }, new[] { (3, 2), (2, 1), (3, 1) }, true);
      TestEdgeRemovalWithNode(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, null, true);

      TestEdgeRemovalWithNode(new[] { 4, 3, 2, 1 }, (i1, i2) => !(i1 == 3 || i2 == 3), new[] { 3, 1, 2, 4 }, new[] { (4, 2), (4, 1), (2, 1) }, false);
      TestEdgeRemovalWithNode(new[] { 3, 2, 1 }, (i1, i2) => i1 >= i2, new[] { 3, 2, 1 }, null, false);
      TestEdgeRemovalWithNode(new[] { 3, 2, 1 }, (i1, i2) => true, new[] { 1, 2, 3 }, new[] { (3, 2), (2, 1), (3, 1) }, false);
      TestEdgeRemovalWithNode(new[] { 3, 2, 1 }, (i1, i2) => false, new[] { 3, 2, 1 }, null, false);
    }

    private void TestSortLoopsCheck<T>(T[] data, Predicate<T, T> connector, T[] expected, T[] loops, bool toList)
    {
      List<Node<T, object>> actualLoopNodes;
      var actual = (!toList)
        ? TopologicalSorter.Sort(data, connector, out actualLoopNodes)
        : TopologicalSorter.SortToList(data, connector, out actualLoopNodes);

      if (expected == null)
        Assert.That(actual, Is.Null);
      else if (data.Length == 0)
        Assert.That(actual, Is.Empty);
      else
        Assert.That(expected.SequenceEqual(actual));

      var actualLoops = actualLoopNodes != null
          ? actualLoopNodes
              .Where(n => n.OutgoingConnectionCount != 0)
              .Select(n => n.Item)
              .ToArray()
          : null;

      AssertEx.HasSameElements(loops, actualLoops);

      var sortWithRemove = (!toList)
        ? TopologicalSorter.Sort(data, connector, out List<NodeConnection<T, object>> _).ToList()
        : TopologicalSorter.SortToList(data, connector, out List<NodeConnection<T, object>> _);
      Assert.AreEqual(sortWithRemove.Count, data.Length);

      if (loops == null) {
        var actualAsList = toList
          ? (IReadOnlyList<T>) actual
          : actual.ToList();

        Assert.AreEqual(sortWithRemove.Count, actualAsList.Count);
        for (var i = 0; i < actualAsList.Count; i++) {
          Assert.AreEqual(sortWithRemove[i], actualAsList[i]);
        }
      }
      else {
        TestLog.Debug("Loops detected");
      }
    }

    private void TestSortNoLoopsCheck<T>(T[] data, Predicate<T, T> connector, T[] expected, bool toList)
    {
      var actual = (toList)
        ? TopologicalSorter.Sort(data, connector)
        : TopologicalSorter.SortToList(data, connector);

      if (expected == null)
        Assert.That(actual, Is.Null);
      else if (data.Length == 0)
        Assert.That(actual, Is.Empty);
      else
        Assert.That(expected.SequenceEqual(actual));

      var sortWithRemove = toList
        ? TopologicalSorter.Sort(data, connector, out List<NodeConnection<T, object>> _).ToList()
        : TopologicalSorter.SortToList(data, connector, out List<NodeConnection<T, object>> _);
      Assert.AreEqual(sortWithRemove.Count, data.Length);
    }

    private void TestEdgeRemoval<T>(T[] data, Predicate<T, T> connector, T[] expected, (T source, T target)[] expectedRemovedEdges, bool toList)
    {
      List<NodeConnection<T, object>> removedEdges;

      var sortWithRemove = (!toList)
        ? TopologicalSorter.Sort(data, connector, out removedEdges)
        : TopologicalSorter.SortToList(data, connector, out removedEdges);
      Assert.That(sortWithRemove, Is.Not.Null);
      Assert.AreEqual(sortWithRemove.Count(), data.Length);
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

    private void TestEdgeRemovalWithNode<T>(T[] data, Predicate<T, T> connector, T[] expected, (T source, T target)[] expectedRemovedEdges, bool toList)
    {
      List<NodeConnection<T, object>> removedEdges;

      var sortWithRemove = (!toList)
        ? TopologicalSorter.Sort(data, connector, out removedEdges, true)
        : TopologicalSorter.SortToList(data, connector, out removedEdges, true);
      Assert.That(sortWithRemove, Is.Not.Null);
      Assert.AreEqual(sortWithRemove.Count(), data.Length);
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