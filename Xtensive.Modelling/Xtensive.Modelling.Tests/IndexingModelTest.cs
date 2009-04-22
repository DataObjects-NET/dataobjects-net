// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.17

using System;
using NUnit.Framework;
using Xtensive.Core.Serialization.Binary;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using Xtensive.Modelling.Tests.IndexingModel;
using System.Linq;

namespace Xtensive.Modelling.Tests
{
  [TestFixture]
  public class IndexingModelTest
  {
    [Test]
    public void RenameTest1()
    {
      var storage = CreateSimpleStorageModel();
      storage.Dump();

      TestUpdate(storage, (s1, s2, hs) => {
        var t1 = (TableInfo) s1.Resolve("Tables/Types");
        t1.Remove();
        var fk1 = (ForeignKeyInfo) s1.Resolve("Tables/Objects/ForeignKeys/FK_TypeId");
        fk1.Remove();
        var o2 = (TableInfo) s2.Resolve("Tables/Objects");
        string o2OldPath = o2.Path;
        o2.Name = "NewObjects";
        hs.Add(new RenameHint(o2OldPath, o2.Path));
      },
      (diff, actions) => { });
    }

    [Test]
    public void RenameTest2()
    {
      var storage = CreateSimpleStorageModel();
      storage.Dump();

      TestUpdate(storage, (s1, s2, hs) => {
        var t2 = (TableInfo) s2.Resolve("Tables/Types");
        string t2OldPath = t2.Path;
        t2.Name = "NewTypes";
        hs.Add(new RenameHint(t2OldPath, t2.Path));
      },
      (diff, actions) => {
        var query =
          from a in actions.Flatten()
          let cna = a as CopyNodeAction
          where cna!=null && cna.Name=="FK_TypeId"
          select a;
        Assert.IsTrue(query.Any());
      });
    }

    [Test]
    public void RenameTest3()
    {
      var storage = CreateSimpleStorageModel();
      storage.Tables["Types"].Index = storage.Tables.Count - 1;
      storage.Dump();

      TestUpdate(storage, (s1, s2, hs) => {
        var t2 = (TableInfo) s2.Resolve("Tables/Types");
        string t2OldPath = t2.Path;
        t2.Name = "NewTypes";
        hs.Add(new RenameHint(t2OldPath, t2.Path));
      },
      (diff, actions) => {
        var query =
          from a in actions
          let cna = a as CopyNodeAction
          where cna!=null && cna.Name=="FK_TypeId"
          select a;
        Assert.IsTrue(query.Any());
      });
    }

    public static StorageInfo CreateSimpleStorageModel()
    {
      var storage = new StorageInfo("Storage");
      
      // Types table
      var t = new TableInfo(storage, "Types");
      var tId = new ColumnInfo(t, "Id") {
        Type = new TypeInfo(typeof (int), false)
      };
      var tValue = new ColumnInfo(t, "Value") {
        Type = new TypeInfo(typeof (string), 1024)
      };
      var tData = new ColumnInfo(t, "Data") {
        Type = new TypeInfo(typeof (byte[]), 1024*1024)
      };

      var tiPk = new PrimaryIndexInfo(t, "PK_Types");
      new KeyColumnRef(tiPk, tId);
      tiPk.PopulateValueColumns();

      var tiValue = new SecondaryIndexInfo(t, "IX_Value");
      new KeyColumnRef(tiValue, tValue);
      tiValue.PopulatePrimaryKeyColumns();

      // Objects table
      var o = new TableInfo(storage, "Objects");
      var oId = new ColumnInfo(o, "Id") {
        Type = new TypeInfo(typeof (long), false)
      };
      var oTypeId = new ColumnInfo(o, "TypeId") {
        Type = new TypeInfo(typeof (int), false)
      };
      var oValue = new ColumnInfo(o, "Value") {
        Type = new TypeInfo(typeof (string), 1024)
      };

      var oiPk = new PrimaryIndexInfo(o, "PK_Objects");
      new KeyColumnRef(oiPk, oId);
      oiPk.PopulateValueColumns();

      var oiTypeId = new SecondaryIndexInfo(o, "IX_TypeId");
      new KeyColumnRef(oiTypeId, oTypeId);
      oiTypeId.PopulatePrimaryKeyColumns();

      var oiValue = new SecondaryIndexInfo(o, "IX_Value");
      new KeyColumnRef(oiValue, oValue);
      new IncludedColumnRef(oiValue, oTypeId);
      oiValue.PopulatePrimaryKeyColumns();

      var ofkTypeId = new ForeignKeyInfo(o, "FK_TypeId") {
        PrimaryKey = tiPk, 
      };
      ofkTypeId.ForeignKeyColumns.Set(oiTypeId);

      storage.Validate();
      return storage;
    }

    #region Private methods

    private static void TestUpdate(StorageInfo origin, Action<StorageInfo, StorageInfo, HintSet> mutator, Action<Difference, ActionSequence> validator)
    {
      // Классное я слово придумал - мутатор ;)
      TestUpdate(origin, mutator, validator, true);
      TestUpdate(origin, mutator, null, false);
    }

    private static void TestUpdate(StorageInfo origin, Action<StorageInfo, StorageInfo, HintSet> mutator, Action<Difference, ActionSequence> validator, bool useHints)
    {
      var s1 = Clone(origin);
      var s2 = Clone(origin);
      var hints = new HintSet(s1, s2);
      mutator.Invoke(s1, s2, hints);
      if (!useHints)
        hints = new HintSet(s1, s2);
      Log.Info("Update test ({0} hints)", useHints ? "with" : "without");
      s1.Dump();
      s2.Dump();
      s1.Validate();
      s2.Validate();

      // Comparing different models
      Log.Info("Comparing models:");
      var comparer = new Comparer();
      var diff = comparer.Compare(s1, s2, hints);
      Log.Info("\r\nDifference:\r\n{0}", diff);
      var actions = new ActionSequence() {
        new Upgrader().GetUpgradeSequence(diff, hints, comparer)
      };
      Log.Info("\r\nActions:\r\n{0}", actions);
      if (validator!=null)
        validator.Invoke(diff, actions);
    }

    private static StorageInfo Clone(StorageInfo server)
    {
      return (StorageInfo) LegacyBinarySerializer.Instance.Clone(server);
    }

    #endregion
  }
}