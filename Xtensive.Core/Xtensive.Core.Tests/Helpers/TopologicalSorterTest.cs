// Copyright (C) 2008 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2008.08.08

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core.Helpers;
using Xtensive.Core.Testing;
using System.Linq;

namespace Xtensive.Core.Tests.Helpers
{
  [TestFixture]
  public class TopologicalSorterTest
  {
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
      List<TopologicalSorter<T>.Node> actualLoopNodes;
      var actual = TopologicalSorter<T>.Sort(data, connector, out actualLoopNodes);
      T[] actualLoops = null;
      if (actualLoopNodes!=null)
        actualLoops = actualLoopNodes
          .Where(n => n.GetConnectionCount(true)!=0)
          .Select(n => n.Item)
          .ToArray();

      AssertEx.AreEqual(expected, actual);
      AssertEx.AreEqual(loops, actualLoops);

      List<Pair<TopologicalSorter<T>.Node>> removedEdges;
      var sortWithRemove = TopologicalSorter<T>.Sort(data, connector, out removedEdges);
      Assert.AreEqual(sortWithRemove.Count, data.Length);
      if (loops==null) {
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