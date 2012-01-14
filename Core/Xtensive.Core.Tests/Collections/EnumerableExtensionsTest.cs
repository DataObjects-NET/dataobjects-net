// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.06.04

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Testing;

namespace Xtensive.Tests.Collections
{
  internal class Node
  {
    public List<Node> Children { get; set; }
    public readonly string Name;

    public Node(string name)
    {
      Name = name;
    }
  }

  [TestFixture]
  public sealed class EnumerableExtensionsTest
  {
    [Test]
    public void RootFirstTest()
    {
      var root = CreateHierarchy();
      var result = Flatten(root, true);
      var expected = new List<Node>{root};
      expected.Add(root.Children[0]);
      expected.AddRange(root.Children[0].Children);
      expected.Add(root.Children[1]);
      expected.AddRange(root.Children[1].Children);
      expected.Add(root.Children[2]);
      expected.Add(root.Children[3]);
      expected.Add(root.Children[4]);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void RootLastTest()
    {
      var root = CreateHierarchy();
      var result = Flatten(root, false);
      var expected = new List<Node>();
      expected.AddRange(root.Children[0].Children);
      expected.Add(root.Children[0]);
      expected.AddRange(root.Children[1].Children);
      expected.Add(root.Children[1]);
      expected.Add(root.Children[2]);
      expected.Add(root.Children[3]);
      expected.Add(root.Children[4]);
      expected.Add(root);
      Assert.IsTrue(expected.SequenceEqual(result));
    }

    [Test]
    public void ExitActionTest()
    {
      var hierarchy = CreateHierarchy();
      var exitActionResult = new List<Node>();
      Console.WriteLine("test");
      var result = Flatten(hierarchy, false, exitActionResult.Add);
      var expected = new List<Node>{hierarchy};
      expected.Add(hierarchy.Children[0]);
      expected.AddRange(hierarchy.Children[0].Children);
      expected.Add(hierarchy.Children[1]);
      expected.AddRange(hierarchy.Children[1].Children);
      expected.Add(hierarchy.Children[2]);
      expected.Add(hierarchy.Children[3]);
      expected.Add(hierarchy.Children[4]);
      var cachedResult = result.ToList();
      Assert.IsTrue(cachedResult.SequenceEqual(exitActionResult));
    }

    private static Node CreateHierarchy()
    {
      return new Node("0") {
        Children = new List<Node> {
          new Node("0.0") {
            Children = new List<Node> {
              new Node("0.0.0") {Children = null},
              new Node("0.0.1") {Children = new List<Node>()},
              null
             }
          },
          new Node("0.1") {
            Children = new List<Node> {
              new Node("0.1.0") {Children = new List<Node>()},
              new Node("0.1.1") {Children = new List<Node>()},
              new Node("0.1.2") {Children = new List<Node>()}
             }
          },
          null,
          new Node("0.3") {Children = null},
          new Node("0.4") {Children = new List<Node>()},
        }
     };
    }

    private static IEnumerable<Node> Flatten(Node root, bool rootFirst)
    {
      return Flatten(root, rootFirst, null);
    }

    private static IEnumerable<Node> Flatten(Node root, bool rootFirst, Action<Node> exitAction)
    {
      return EnumerableUtils.One(root).Flatten(n => n != null ? n.Children : null, exitAction, rootFirst);
    }

    [Test]
    public void BatchTest()
    {
      var source = InstanceGenerationUtils<int>.GetInstances(new Random(), 0.1).Take(258);
      const int initialBatchSize = 8;
      const int maximalBatchSize = 32;
      const int fastFirstCount = 10;
      var result = source.Batch(fastFirstCount, initialBatchSize, maximalBatchSize);
      Assert.AreEqual(fastFirstCount, result.TakeWhile(e => !(e is List<int>)).Count());
      var batchSize = initialBatchSize;
      Assert.IsTrue(result.Skip(fastFirstCount).All(e => {
        var r = ((List<int>) e).Count==batchSize;
        if(batchSize < maximalBatchSize)
          batchSize *= 2;
        return r;
      }));
      Assert.AreEqual(batchSize, 32);
    }

    [Test]
    public void ApplyBeforeAndAfter()
    {
      const int totalCount = 256;
      const int batchSize = 32;
      var source = InstanceGenerationUtils<int>.GetInstances(new Random(), 0.1).Take(totalCount);
      var batches = source.Batch(0, batchSize, batchSize);
      var count = 0;
      Assert.AreEqual(totalCount / batchSize, batches.ApplyBeforeAndAfter(() => count++, null).Count());
      Assert.AreEqual(totalCount / batchSize + 1, count);
      count = 0;
      Assert.AreEqual(totalCount / batchSize, batches.ApplyBeforeAndAfter(null, () => count++).Count());
      Assert.AreEqual(totalCount / batchSize + 1, count);
    }

    [Test]
    public void AtLeastAtMostTest()
    {
      Assert.AreEqual(true, new[] {1, 2, 3, 4}.AtLeast(3));
      Assert.AreEqual(true, new[] {1, 2, 3}.AtLeast(3));
      Assert.AreEqual(false, new[] {1}.AtLeast(2));
      Assert.AreEqual(true, new[] {1, 2}.AtLeast(0));
      Assert.AreEqual(true, new int[0].AtLeast(-1));

      Assert.AreEqual(false, new[] {1, 2, 3, 4}.AtMost(3));
      Assert.AreEqual(true, new[] {1, 2, 3}.AtMost(3));
      Assert.AreEqual(true, new[] {1}.AtMost(2));
      Assert.AreEqual(false, new[] {1, 2}.AtMost(0));
      Assert.AreEqual(false, new int[0].AtMost(-1));
    }
  }
}