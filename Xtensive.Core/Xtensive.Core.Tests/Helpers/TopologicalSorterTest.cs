// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.08

using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Collections;
using System.Linq;
using Xtensive.Core;
using Xtensive.Sorting;
using Xtensive.Testing;

namespace Xtensive.Tests.Helpers
{
  [TestFixture]
  public class TopologicalSorterTest
  {
    [Test]
    public void SelfReferenceTest()
    {
      var node = new Node<int, string>(1);
      var connection = new NodeConnection<int, string>(node, node, "ConnectionItem");
      connection.BindToNodes();

      List<NodeConnection<int, string>> removedEdges;
      var result = TopologicalSorter.Sort(EnumerableUtils.One(node), out removedEdges);
      Assert.AreEqual(1, result.Count);
      Assert.AreEqual(node.Item, result[0]);
      Assert.AreEqual(1, removedEdges.Count);
      Assert.AreEqual(connection, removedEdges[0]);
      
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

      List<NodeConnection<int, string>> removedEdges;
      var result = TopologicalSorter.Sort(new[]{node2, node1}, out removedEdges);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(node1.Item, result[0]);
      Assert.AreEqual(node2.Item, result[1]);

      Assert.AreEqual(1, removedEdges.Count);
      Assert.AreEqual(connection21_1, removedEdges[0]);

      // Remove whole node
      connection12_1.BindToNodes();
      connection12_2.BindToNodes();
      connection21_1.BindToNodes();

      result = TopologicalSorter.Sort(new[]{node2, node1}, out removedEdges, true);
      Assert.AreEqual(2, result.Count);
      Assert.AreEqual(node1.Item, result[1]);
      Assert.AreEqual(node2.Item, result[0]);

      Assert.AreEqual(2, removedEdges.Count);
      Assert.AreEqual(0, removedEdges.Except(new[]{connection12_1, connection12_2} ).Count());
    }

    [Test]
    public void CombinedTest()
    {
      TestSort(new[] {4, 3, 2, 1}, (i1, i2) => !(i1==3 || i2==3), null, new[] {4, 2, 1});
      TestSort(new[] {3, 2, 1}, (i1, i2) => i1 >= i2, new[] {1, 2, 3}, null);
      TestSort(new[] {3, 2, 1}, (i1, i2) => true, null, new[] {1, 2, 3});
      TestSort(new[] {3, 2, 1}, (i1, i2) => false, new[] {3 ,2, 1}, null);
    }

    private void TestSort<T>(T[] data, Predicate<T, T> connector, T[] expected, T[] loops)
    {
      List<Node<T, object>> actualLoopNodes;
      var actual = TopologicalSorter.Sort(data, connector, out actualLoopNodes);
      T[] actualLoops = null;
      if (actualLoopNodes!=null)
        actualLoops = actualLoopNodes
          .Where(n => n.OutgoingConnectionCount!=0)
          .Select(n => n.Item)
          .ToArray();

      AssertEx.AreEqual(expected, actual);
      AssertEx.AreEqual(loops, actualLoops);

      List<NodeConnection<T, object>> removedEdges;
      var sortWithRemove = TopologicalSorter.Sort(data, connector, out removedEdges);
      Assert.AreEqual(sortWithRemove.Count, data.Length);
      if (loops == null)
      {
        Assert.AreEqual(sortWithRemove.Count, actual.Count);
        for (int i = 0; i < actual.Count; i++) {
          Assert.AreEqual(sortWithRemove[i], actual[i]);
        }
      }
      else {
        Log.Debug("Loops detected");
      }
    }
  }
}