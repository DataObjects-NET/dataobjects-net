// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.17

using System;
using NUnit.Framework;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Tests.IndexingModel;

namespace Xtensive.Modelling.Tests
{
  [TestFixture]
  public class IndexingModelTest
  {
    [Test]
    public void CombinedTest()
    {
      var storage = CreateSimpleStorageModel();
    }

    #region Private methods

    private static StorageInfo CreateSimpleStorageModel()
    {
      var storage = new StorageInfo("Storage");
      var t = new TableInfo(storage, "Types");
      var tId = new ColumnInfo(t, "Id") {
        Type = new TypeInfo(typeof (int))
      };
      var tValue = new ColumnInfo(t, "Value") {
        Type = new TypeInfo(typeof (string), 1024)
      };
      return storage;
    }

    private static void TestUpdate(StorageInfo origin, Action<StorageInfo, StorageInfo, HintSet> update)
    {
      TestUpdate(origin, update, true);
      TestUpdate(origin, update, false);
    }

    private static void TestUpdate(StorageInfo origin, Action<StorageInfo, StorageInfo, HintSet> update, bool useHints)
    {
      var s1 = Clone(origin);
      var s2 = Clone(origin);
      var hints = new HintSet(s1, s2);
      update.Invoke(s1, s2, hints);
      Log.Info("Update test ({0} hints)", useHints ? "with" : "without");
      s1.Dump();
      s2.Dump();

      // Comparing different models
      Log.Info("Comparing models:");
      var c = new Comparison.Comparer<StorageInfo>(s1, s2);
      if (useHints)
        foreach (var hint in hints)
          c.Hints.Add(hint);
      var diff = c.Difference;
      Log.Info("\r\nDifference:\r\n{0}", diff);
      var actions = new ActionSequence() { diff.ToActions() };
      Log.Info("\r\nActions:\r\n{0}", actions);
      actions.Apply(s1);
      s1.Dump();
      s2.Dump();

      // Comparing action applicaiton result & target model
      Log.Info("Comparing synchronization result:");
      c = new Comparison.Comparer<StorageInfo>(s1, s2);
      diff = c.Difference;
      Log.Info("\r\nDifference:\r\n{0}", diff);
      Assert.IsNull(diff);
    }

    private static StorageInfo Clone(StorageInfo server)
    {
      return (StorageInfo) LegacyBinarySerializer.Instance.Clone(server);
    }

    #endregion
  }
}