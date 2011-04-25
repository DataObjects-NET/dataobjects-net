// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alexander Nikolaev
// Created:    2009.11.26

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Transactions;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Orm.Configuration;
using Xtensive.Storage.Providers;

namespace Xtensive.Orm.Tests.Storage.Randomized
{
  [TestFixture]
  public sealed class RandomizedTest : AutoBuildTest
  {
    private const int iterationCount = 1000;
    private const int initialNodeCount = 5000;
    private const int initialTreeCount = 10;
    private List<Pair<Key, int>> nodesData;
    private List<Action> actions;
    private Random randomProvider;
    private bool isSettingUp;
    private readonly List<Key> entitySetCache = new List<Key>();
    private bool isProtocolMemory;
    
    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      isProtocolMemory = config.ConnectionInfo.Provider==WellKnown.Provider.Memory;
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Tree).Assembly, typeof(Tree).Namespace);
      return config;
    }

    [SetUp]
    public void SetUp()
    {
      var seed = 1439675735;//GetSeed();
      Console.WriteLine("Seed: {0}", seed);
      randomProvider = new Random(seed);
      actions = new List<Action> {AddNode, RemoveNode, TransferNode, AddTree};
      nodesData = new List<Pair<Key, int>>();
      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction(IsolationLevel.ReadCommitted)) {
        isSettingUp = true;
        for (int i = 0; i < initialTreeCount; i++)
          AddTree();
        for (var j = 0; j < initialNodeCount - initialTreeCount; j++)
          AddNode();
        isSettingUp = false;
        tx.Complete();
      }
    }

    [Test]
    public void CombinedTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);
      using (var session = Domain.OpenSession())
        for (int i = 0; i < iterationCount; i++)
          GetAction().Invoke();

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var trees = Session.Demand().Query.All<Tree>().ToList();
        long totalCount = 0;
        foreach (var tree in trees)
          totalCount += ValidateNodes(tree.Root) + 1;
        Assert.AreEqual(nodesData.Count, totalCount);
      }
    }

    private long ValidateNodes(TreeNode current)
    {
      if (current.Parent == null)
        Assert.IsNotNull(current.Tree);
      else
        Assert.IsNull(current.Tree);
      var nodePair = nodesData.Where(pair => pair.First == current.Key).First();
      Assert.AreEqual(current.Children.Count, nodePair.Second);
      var result = current.Children.Count;
      if (current.Parent != null)
        Assert.IsTrue(current.Parent.Children.Contains(current));
      foreach (var node in current.Children)
        result += ValidateNodes(node);
      return result;
    }

    private void AddNode()
    {
      Key newNodeKey;
      Key parentNodeKey;
      try {
        using (var tx = isSettingUp ? null : Session.Demand().OpenTransaction()) {
          parentNodeKey = nodesData[GetNodeIndex()].First;
          var parentNode = Session.Demand().Query.Single<TreeNode>(parentNodeKey);
          var newNode = new TreeNode();
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
        using (var tx = Session.Demand().OpenTransaction()) {
          removedNodeIndex = GetNodeIndex();
          removedNodeKey = nodesData[removedNodeIndex].First;
          var removedNode = Session.Demand().Query.Single<TreeNode>(removedNodeKey);
          if (removedNode.Parent == null)
            return;
          parentNodeKey = removedNode.Parent.Key;
          removedNodeChildCount = removedNode.Children.Count;
          entitySetCache.Clear();
          entitySetCache.AddRange(removedNode.Children.Select(n => n.Key));
          foreach (var key in entitySetCache)
            removedNode.Parent.Children.Add(Session.Demand().Query.Single<TreeNode>(key));
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
        using (var tx = Session.Demand().OpenTransaction()) {
          var treeCount = Session.Demand().Query.All<Tree>().Count();
          if (nodesData.Count == 1 || treeCount == 1)
            return;
          var nodeIndex = GetNodeIndex();
          var nodeKey = nodesData[nodeIndex].First;
          var node = Session.Demand().Query.Single<TreeNode>(nodeKey);
          if (node.Parent == null)
            return;
          var root = node;
          while (root.Tree == null)
            root = root.Parent;
          var newParentNode =  Session.Demand().Query.All<Tree>().Where(t => t != root.Tree)
            .Skip(randomProvider.Next(0, treeCount)).First().Root;
          newParentKey = newParentNode.Key;
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

    private void AddTree()
    {
      Key key;
      try {
        using (var tx = isSettingUp ? null : Session.Demand().OpenTransaction()) {
          var tree = new Tree();
          tree.Root = new TreeNode {Tree = tree};
          key = tree.Root.Key;
          ThrowOrCompleteTransaction(tx);
        }
      }
      catch(InvalidOperationException) {
        return;
      }
      nodesData.Add(new Pair<Key, int>(key, 0));
    }

    private void RemoveTree()
    {
      var treeNodeKeys = new List<Key>();
      try {
        using (var tx = Session.Demand().OpenTransaction()) {
          if (Session.Demand().Query.All<Tree>().Count()==1)
            return;
          var nodeIndex = GetNodeIndex();
          var nodeKey = nodesData[nodeIndex].First;
          var node = Session.Demand().Query.Single<TreeNode>(nodeKey);
          while (node.Tree==null)
            node = node.Parent;
          treeNodeKeys.AddRange(node.Children.Flatten(n => n.Children, null, true).Select(n => n.Key));
          treeNodeKeys.Add(node.Key);
          node.Tree.Remove();
          ThrowOrCompleteTransaction(tx);
        }
      }
      catch (InvalidOperationException) {
        return;
      }

      // TODO: It's very slow. Probably it should be optimized.
      foreach (var key in treeNodeKeys)
        nodesData.RemoveAt(FindNodeIndex(key));
    }

    private void UpdateChildrenCount(Key parentNodeKey, bool increment)
    {
      UpdateChildrenCount(parentNodeKey, increment ? 1 : -1);
    }

    private void UpdateChildrenCount(Key parentNodeKey, int increment)
    {
      var index = FindNodeIndex(parentNodeKey);
      var pair = nodesData[index];
      nodesData[index] = new Pair<Key, int>(parentNodeKey, pair.Second + increment);
    }

    private int FindNodeIndex(Key key)
    {
      for (var i = 0; i < nodesData.Count; i++) {
        var pair = nodesData[i];
        if (pair.First == key)
          return i;
      }
      throw new Exception();
    }

    private Action GetAction()
    {
      var index = randomProvider.Next(0, actions.Count);
      /*if(!isSettingUp)
        Console.WriteLine(actions[index].Method.Name);*/
      return actions[index];
    }

    private int GetNodeIndex()
    {
      return randomProvider.Next(0, nodesData.Count);
    }

    private void ThrowOrCompleteTransaction(TransactionScope tx)
    {
      if (isSettingUp)
        return;
      if (!isProtocolMemory && randomProvider.Next(0, 2) == 1)
        throw new InvalidOperationException();
      tx.Complete();
    }

    private static int GetSeed()
    {
      var bytes = new byte[sizeof (int)];
      var seedProvider = new RNGCryptoServiceProvider();
      seedProvider.GetNonZeroBytes(bytes);
      return BitConverter.ToInt32(bytes, 0);
    }
  }
}