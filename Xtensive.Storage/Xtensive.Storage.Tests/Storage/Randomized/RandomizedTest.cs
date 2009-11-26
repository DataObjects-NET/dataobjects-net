// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.11.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using Xtensive.Core;

namespace Xtensive.Storage.Tests.Storage.Randomized
{
  [TestFixture]
  public sealed class RandomizedTest : AutoBuildTest
  {
    private const int iterationCount = 1000;
    private const int initialNodeCount = 5000;
    private List<Pair<Key, int>> nodesData;
    private List<Action> actions;
    private RNGCryptoServiceProvider randomProvider;
    private readonly byte[] actionBytes = new byte[1];
    private readonly byte[] nodeIndexBytes = new byte[sizeof (int)];
    private readonly byte[] txBytes = new byte[1];
    private bool isSettingUp;
    private readonly List<Key> entitySetCache = new List<Key>();

    protected override Xtensive.Storage.Configuration.DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Tree).Assembly, typeof(Tree).Namespace);
      return config;
    }

    [SetUp]
    public void SetUp()
    {
      randomProvider = new RNGCryptoServiceProvider();
      actions = new List<Action> {AddNode, RemoveNode, TransferNode};
      nodesData = new List<Pair<Key, int>>();
      using (var session = Session.Open(Domain))
      using (var tx = Transaction.Open()) {
        var tree = new Tree();
        tree.Root = new TreeNode(tree);
        nodesData.Add(new Pair<Key, int>(tree.Root.Key, 0));
        isSettingUp = true;
        for (var i = 0; i < initialNodeCount; i++)
          AddNode();
        isSettingUp = false;
        tx.Complete();
      }
    }

    [Test]
    public void CombinedTest()
    {
      using (Session.Open(Domain))
        for (int i = 0; i < iterationCount; i++)
          GetAction().Invoke();

      using (Session.Open(Domain))
      using (Transaction.Open()) {
        var tree = Query<Tree>.All.Single();
        ValidateNodes(tree.Root);
      }
    }

    private void ValidateNodes(TreeNode current)
    {
      Assert.IsNotNull(current.Tree);
      var nodePair = nodesData.Where(pair => pair.First == current.Key).First();
      Assert.AreEqual(current.Children.Count, nodePair.Second);
      if (current.Tree.Root != current) {
        Assert.IsNotNull(current.Parent);
        Assert.IsTrue(current.Parent.Children.Contains(current));
      }
      foreach (var node in current.Children)
        ValidateNodes(node);
    }

    private void AddNode()
    {
      Key newNodeKey;
      Key parentNodeKey;
      try {
        using (var tx = isSettingUp ? null : Transaction.Open()) {
          parentNodeKey = nodesData[GetNodeIndex()].First;
          var parentNode = Query<TreeNode>.Single(parentNodeKey);
          var newNode = new TreeNode (parentNode.Tree);
          parentNode.Children.Add(newNode);
          newNodeKey = newNode.Key;
          ThrowOrCompleteTransaction(tx);
        }
      }
      catch(InvalidOperationException) {
        return;
      }
      nodesData.Add(new Pair<Key, int>(newNodeKey, 0));
      UpdateChildrenCount(parentNodeKey, true);
    }

    private void RemoveNode()
    {
      Key removedNodeKey;
      Key parentNodeKey;
      long removedNodeChildCount;
      int removedNodeIndex;
      try {
        using (var tx = Transaction.Open()) {
          removedNodeIndex = GetNodeIndex();
          removedNodeKey = nodesData[removedNodeIndex].First;
          var removedNode = Query<TreeNode>.Single(removedNodeKey);
          if (removedNode.Parent == null)
            return;
          parentNodeKey = removedNode.Parent.Key;
          removedNodeChildCount = removedNode.Children.Count;
          entitySetCache.Clear();
          entitySetCache.AddRange(removedNode.Children.Select(n => n.Key));
          foreach (var key in entitySetCache)
            removedNode.Parent.Children.Add(Query<TreeNode>.Single(key));
          removedNode.Children.Clear();
          removedNode.Remove();
          ThrowOrCompleteTransaction(tx);
        }
      }
      catch(InvalidOperationException) {
        return;
      }
      nodesData.RemoveAt(removedNodeIndex);
      UpdateChildrenCount(parentNodeKey, (int) (removedNodeChildCount - 1));
    }

    private void TransferNode()
    {
      Key oldParentKey;
      Key newParentKey;
      try {
        using (var tx = Transaction.Open()) {
          var nodeIndex = GetNodeIndex();
          var nodeKey = nodesData[nodeIndex].First;
          var newParentIndex = GetNodeIndex();
          if (newParentIndex == nodeIndex)
            newParentIndex = nodeIndex <= nodesData.Count - 1 ? nodeIndex + 1 : 0;
          newParentKey = nodesData[newParentIndex].First;
          var node = Query<TreeNode>.Single(nodeKey);
          if (node.Parent == null)
            return;
          var newParentNode = Query<TreeNode>.Single(newParentKey);
          oldParentKey = node.Parent.Key;
          node.Parent.Children.Remove(node);
          newParentNode.Children.Add(node);
          ThrowOrCompleteTransaction(tx);
        }
      }
      catch(InvalidOperationException) {
        return;
      }
      UpdateChildrenCount(newParentKey, true);
      UpdateChildrenCount(oldParentKey, false);
    }

    private void UpdateChildrenCount(Key parentNodeKey, bool increment)
    {
      UpdateChildrenCount(parentNodeKey, increment ? 1 : -1);
    }

    private void UpdateChildrenCount(Key parentNodeKey, int increment)
    {
      for (int i = 0; i < nodesData.Count; i++) {
        var pair = nodesData[i];
        if (pair.First == parentNodeKey) {
          nodesData[i] = new Pair<Key, int>(parentNodeKey, pair.Second + increment);
          return;
        }
      }
    }

    private Action GetAction()
    {
      randomProvider.GetBytes(actionBytes);
      var index = (int) Math.Truncate(actions.Count * (actionBytes[0] / 255d));
      if (index == actions.Count)
        index--;
      return actions[index];
    }

    private int GetNodeIndex()
    {
      randomProvider.GetBytes(nodeIndexBytes);
      var t = Math.Abs(BitConverter.ToInt32(nodeIndexBytes, 0) / (double) (int.MaxValue));
      return (int) Math.Truncate((nodesData.Count - 1)
        * t);
    }

    private void ThrowOrCompleteTransaction(TransactionScope tx)
    {
      if (isSettingUp)
        return;
      randomProvider.GetBytes(txBytes);
      if (txBytes[0] > 127)
        throw new InvalidOperationException();
      tx.Complete();
    }
  }
}