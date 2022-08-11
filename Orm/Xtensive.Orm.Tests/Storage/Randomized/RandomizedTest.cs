// Copyright (C) 2009-2022 Xtensive LLC.
// This code is distributed under MIT license terms.
// See the License.txt file in the project root for more information.
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
using Xtensive.Orm.Providers;

namespace Xtensive.Orm.Tests.Storage.Randomized
{
  [TestFixture]
  public sealed class RandomizedTest : AutoBuildTest
  {
    private const int IterationCount = 1000;
    private const int InitialNodeCount = 5000;
    private const int InitialTreeCount = 10;
    private const int ConstSeed = 1439675735;

    private readonly List<Key> entitySetCache = new();

    private List<Pair<Key, int>> nodesData;
    private List<Action<Session>> actions;
    private Random randomProvider;
    private bool isSettingUp;
    

    protected override DomainConfiguration BuildConfiguration()
    {
      var config = base.BuildConfiguration();
      config.UpgradeMode = DomainUpgradeMode.Recreate;
      config.Types.Register(typeof(Tree).Assembly, typeof(Tree).Namespace);
      return config;
    }

    [SetUp]
    public void SetUp()
    {
      var seed = GetSeed(useConst:true);
      Console.WriteLine("Seed: {0}", seed);
      randomProvider = new Random(seed);
      actions = new List<Action<Session>> {AddNode, RemoveNode, TransferNode, AddTree, /*RemoveTree*/};
      nodesData = new List<Pair<Key, int>>();

      using (var session = Domain.OpenSession())
      using (var tx = session.OpenTransaction(IsolationLevel.ReadCommitted)) {
        isSettingUp = true;
        for (var i = 0; i < InitialTreeCount; i++) {
          AddTree(session);
        }

        for (var j = 0; j < InitialNodeCount - InitialTreeCount; j++) {
          AddNode(session);
        }

        isSettingUp = false;
        tx.Complete();
      }
    }

    [Test]
    public void CombinedTest()
    {
      Require.AnyFeatureSupported(ProviderFeatures.RowNumber | ProviderFeatures.NativePaging);

      using (var session = Domain.OpenSession()) {
        for (var i = 0; i < IterationCount; i++) {
          GetAction().Invoke(session);
        }
      }

      using (var session = Domain.OpenSession())
      using (session.OpenTransaction()) {
        var trees = session.Query.All<Tree>().ToList();
        var totalCount = 0L;
        foreach (var tree in trees) {
          totalCount += ValidateNodes(tree.Root) + 1;
        }

        Assert.AreEqual(nodesData.Count, totalCount);
      }
    }

    private long ValidateNodes(TreeNode current)
    {
      if (current.Parent == null) {
        Assert.IsNotNull(current.Tree);
      }
      else {
        Assert.IsNull(current.Tree);
      }

      var nodePair = nodesData.Where(pair => pair.First == current.Key).First();
      Assert.AreEqual(current.Children.Count, nodePair.Second);

      var result = current.Children.Count;
      if (current.Parent != null) {
        Assert.IsTrue(current.Parent.Children.Contains(current));
      }
      foreach (var node in current.Children) {
        result += ValidateNodes(node);
      }

      return result;
    }

    private void AddNode(Session session)
    {
      Key newNodeKey;
      Key parentNodeKey;
      try {
        using (var tx = isSettingUp ? null : session.OpenTransaction()) {
          parentNodeKey = nodesData[GetNodeIndex()].First;
          var parentNode = session.Query.Single<TreeNode>(parentNodeKey);
          var newNode = new TreeNode();
          _ = parentNode.Children.Add(newNode);
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

    private void RemoveNode(Session session)
    {
      Key removedNodeKey;
      Key parentNodeKey;
      long removedNodeChildCount;
      int removedNodeIndex;
      try {
        using (var tx = session.OpenTransaction()) {
          removedNodeIndex = GetNodeIndex();
          removedNodeKey = nodesData[removedNodeIndex].First;
          var removedNode = session.Query.Single<TreeNode>(removedNodeKey);
          if (removedNode.Parent == null) {
            return;
          }

          parentNodeKey = removedNode.Parent.Key;
          removedNodeChildCount = removedNode.Children.Count;
          entitySetCache.Clear();
          entitySetCache.AddRange(removedNode.Children.Select(n => n.Key));
          foreach (var key in entitySetCache) {
            _ = removedNode.Parent.Children.Add(session.Query.Single<TreeNode>(key));
          }

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

    private void TransferNode(Session session)
    {
      Key oldParentKey;
      Key newParentKey;
      try {
        using (var tx = session.OpenTransaction()) {
          var treeCount = session.Query.All<Tree>().Count();
          if (nodesData.Count == 1 || treeCount == 1) {
            return;
          }

          var nodeIndex = GetNodeIndex();
          var nodeKey = nodesData[nodeIndex].First;
          var node = session.Query.Single<TreeNode>(nodeKey);
          if (node.Parent == null) {
            return;
          }

          var root = node;
          while (root.Tree == null) {
            root = root.Parent;
          }

          var newParentNode =  session.Query.All<Tree>().Where(t => t != root.Tree)
            .Skip(randomProvider.Next(0, treeCount))
            .First()
            .Root;
          newParentKey = newParentNode.Key;
          oldParentKey = node.Parent.Key;
          _ = node.Parent.Children.Remove(node);
          _ = newParentNode.Children.Add(node);
          ThrowOrCompleteTransaction(tx);
        }
      }
      catch(InvalidOperationException) {
        return;
      }
      UpdateChildrenCount(newParentKey, true);
      UpdateChildrenCount(oldParentKey, false);
    }

    private void AddTree(Session session)
    {
      Key key;
      try {
        using (var tx = isSettingUp ? null : session.OpenTransaction()) {
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

#pragma warning disable IDE0051 // Remove unused private members
    private void RemoveTree(Session session)
#pragma warning restore IDE0051 // Remove unused private members
    {
      var treeNodeKeys = new List<Key>();
      try {
        using (var tx = session.OpenTransaction()) {
          if (session.Query.All<Tree>().Count() == 1) {
            return;
          }

          var nodeIndex = GetNodeIndex();
          var nodeKey = nodesData[nodeIndex].First;
          var node = session.Query.Single<TreeNode>(nodeKey);
          while (node.Tree == null) {
            node = node.Parent;
          }

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
      foreach (var key in treeNodeKeys) {
        nodesData.RemoveAt(FindNodeIndex(key));
      }
    }

    private void UpdateChildrenCount(Key parentNodeKey, bool increment) =>
      UpdateChildrenCount(parentNodeKey, increment ? 1 : -1);

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
        if (pair.First == key) {
          return i;
        }
      }
      throw new Exception();
    }

    private Action<Session> GetAction()
    {
      var index = randomProvider.Next(0, actions.Count);
      /*if(!isSettingUp)
        Console.WriteLine(actions[index].Method.Name);*/
      return actions[index];
    }

    private int GetNodeIndex() => randomProvider.Next(0, nodesData.Count);

    private void ThrowOrCompleteTransaction(TransactionScope tx)
    {
      if (isSettingUp) {
        return;
      }
      if (randomProvider.Next(0, 2) == 1) {
        throw new InvalidOperationException();
      }
      tx.Complete();
    }

    private static int GetSeed(bool useConst)
    {
      if (useConst) {
        return ConstSeed;
      }

      var bytes = new byte[sizeof(int)];

      using var seedProvider = RandomNumberGenerator.Create();
      seedProvider.GetNonZeroBytes(bytes);
      return BitConverter.ToInt32(bytes, 0);
    }
  }
}