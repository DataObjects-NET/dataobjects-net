// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.05.28

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Xtensive.Core.Collections;

namespace Xtensive.Core.Tests.Collections
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
  public sealed class EnumerableUtilsTest
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
      return EnumerableUtils.Flatten(EnumerableUtils.One(root),
        n => n != null ? n.Children : null, exitAction, rootFirst);
    }
  }
}