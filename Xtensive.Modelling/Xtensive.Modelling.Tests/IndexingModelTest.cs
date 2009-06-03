// Copyright (C) 2009 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.17

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Core.Collections;
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
    public void RenameNodeTest()
    {
      var storage = CreateSimpleStorageModel();
      var tTypes = storage.Tables["Types"];
      tTypes.SecondaryIndexes["IX_Value"].Name = "IX_Value_New";
      tTypes.PrimaryIndex.Name = "PK_Types_New";
    }

    [Test]
    public void SimpleTest()
    {
      var storage = CreateSimpleStorageModel();
      TestUpdate(storage, (s1, s2, hs) => { },
        (diff, actions) => Assert.IsNull(diff));
    }

    [Test]
    public void MutualRenameTest()
    {
      var storage = CreateSimpleStorageModel();
      TestUpdate(storage, (s1, s2, hs) => {
        var oldTTypes = s1.Tables["Types"];
        var oldCValue = oldTTypes.Columns["Value"];
        var oldCData = oldTTypes.Columns["Data"];
        var oldTObjects = s1.Tables["Objects"];
        var newTTypes = s2.Tables["Types"];
        var newCValue = newTTypes.Columns["Value"];
        var newCData = newTTypes.Columns["Data"];
        var newTObjects = s2.Tables["Objects"];
        oldTTypes.PrimaryIndex.Name = "PK_Types_Old";
        newTTypes.PrimaryIndex.Name = "PK_Types_New";
        newCValue.Name = "Temp";
        newCData.Name = "Value";
        newCValue.Name = "Data";
        newTTypes.Name = "Temp";
        newTObjects.Name = "Types";
        newTTypes.Name = "Objects";
        hs.Add(new RenameHint(oldCData.Path, newCData.Path));
        hs.Add(new RenameHint(oldCValue.Path, newCValue.Path));
        hs.Add(new RenameHint(oldTTypes.Path, newTTypes.Path));
        hs.Add(new RenameHint(oldTObjects.Path, newTObjects.Path));
      },
        (diff, actions) => { });
    }

    [Test]
    public void RemoveDependentPropertyTest1()
    {
      var storage = CreateSimpleStorageModel();
      storage.Dump();

      TestUpdate(storage, (s1, s2, hs) => {
        var o = (TableInfo) s2.Resolve("Tables/Objects");
        o.Remove();
      },
        (diff, actions) => Assert.IsTrue(
          actions
            .Flatten()
            .OfType<RemoveNodeAction>()
            .Any(a => a.Path=="Tables/Objects/ForeignKeys/FK_TypeId")));
    }

    [Test]
    public void RemoveDependentPropertyTest2()
    {
      var storage = CreateSimpleStorageModel();
      storage.Dump();

      TestUpdate(storage, (s1, s2, hs) => {
        var t1 = (TableInfo) s2.Resolve("Tables/Types");
        var t2 = (TableInfo) s2.Resolve("Tables/Objects");
        t1.Remove();
        t2.Remove();
      },
        (diff, actions) => Assert.IsTrue(
          actions
            .Flatten()
            .OfType<RemoveNodeAction>()
            .Any(a => a.Path=="Tables/Objects/ForeignKeys/FK_TypeId")));
    }

    [Test]
    public void RemoveDependentPropertyTest3()
    {
      var storage = CreateSimpleStorageModel();
      storage.Dump();

      TestUpdate(storage, (s1, s2, hs) => {
        var t1 = (TableInfo) s2.Resolve("Tables/Types");
        t1.Columns["Value"].Name = "NewValue";
        RepopulateValueColumns(s2, "Types");
      },
        (diff, actions) => Assert.IsTrue(
          actions
            .Flatten()
            .OfType<RemoveNodeAction>()
            .Any(a => a.Path=="Tables/Objects/ForeignKeys/FK_TypeId")));
    }

    [Test]
    public void RemoveDependentPropertyTest4()
    {
      var storage = CreateSimpleStorageModel();
      storage.Dump();

      TestUpdate(storage, (s1, s2, hs) => {
        var t1 = (TableInfo) s2.Resolve("Tables/Objects");
        var c1 = t1.Columns["TypeId"];
        var oldPath = c1.Path;
        c1.Name = "NewTypeId";
        hs.Add(new RenameHint(oldPath, c1.Path));
      },
      (diff, actions) => { });
    }

    [Test]
    public void IgnoreHintTest()
    {
      var storage = CreateSimpleStorageModel();
      storage.Dump();

      TestUpdate(storage, (s1, s2, hs) => {
        var o = s1.Resolve("Tables/Objects") as TableInfo;
        new ColumnInfo(o, "IgnoredColumn", new TypeInfo(typeof (int)));
        RepopulateValueColumns(s1, "Objects");
        hs.Add(new IgnoreHint("Tables/Objects/Columns/IgnoredColumn"));
      },
      (diff, actions) => 
        Assert.IsNull(diff));
    }

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
          let cna = a as CreateNodeAction
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
          from a in actions.Flatten()
          let cna = a as CreateNodeAction
          where cna!=null && cna.Name=="FK_TypeId"
          select a;
        Assert.IsTrue(query.Any());
      });
    }

    [Test]
    public void MoveColumnTest()
    {
      var storage = CreateSimpleStorageModel();
      TestUpdate(storage, (s1, s2, hs) => {
        new ColumnInfo(s1.Tables["Types"], "ColumnToRemove", new TypeInfo(typeof (int)));
        var oldColumn = s1.Tables["Types"].Columns["Data"];
        ((Node) s2.Resolve(oldColumn.Path)).Remove();
        var newColumn = new ColumnInfo(s2.Tables["Objects"], "Data", oldColumn.Type);
        RepopulateValueColumns(s1, "Types");
        RepopulateValueColumns(s2, "Types");
        RepopulateValueColumns(s2, "Objects");

        var copyDataHint = new CopyDataHint();
        copyDataHint.SourceTablePath = "Tables/Types";
        copyDataHint.CopiedColumns.Add(new Pair<string>("Tables/Types/Columns/Data", "Tables/Objects/Columns/Data"));
        copyDataHint.Identities.Add(new IdentityPair("Tables/Types/Columns/Id", "Tables/Objects/Columns/TypeId", false));
        hs.Add(copyDataHint);
      },
        (diff, actions)=> { });
    }

    [Test]
    public void MoveColumnFromRemovedTableTest()
    {
      var storage = CreateSimpleStorageModel();
      TestUpdate(storage, (s1, s2, hs) => {
        var oldColumn = s1.Tables["Types"].Columns["Data"];
        s2.Tables["Objects"].ForeignKeys.Clear();
        s2.Tables["Types"].Remove();
        var newColumn = new ColumnInfo(s2.Tables["Objects"], "Data", oldColumn.Type);
        RepopulateValueColumns(s1, "Types");
        RepopulateValueColumns(s2, "Objects");

        var copyDataHint = new CopyDataHint();
        copyDataHint.SourceTablePath = "Tables/Types";
        copyDataHint.CopiedColumns.Add(new Pair<string>("Tables/Types/Columns/Data", "Tables/Objects/Columns/Data"));
        copyDataHint.Identities.Add(new IdentityPair("Tables/Types/Columns/Id", "Tables/Objects/Columns/TypeId", false));
        hs.Add(copyDataHint);
      },
        (diff, actions) => { });
    }

    [Test]
    public void MoveColumnFromRemovedTableToRenamedTableTest()
    {
      var storage = CreateSimpleStorageModel();
      TestUpdate(storage, (s1, s2, hs) => {
        var oldColumn = s1.Tables["Types"].Columns["Data"];
        s2.Tables["Objects"].ForeignKeys.Clear();
        s2.Tables["Types"].Remove();
        s2.Tables["Objects"].Name = "NewObjects";
        var newColumn = new ColumnInfo(s2.Tables["NewObjects"], "NewData", oldColumn.Type);
        RepopulateValueColumns(s1, "Types");
        RepopulateValueColumns(s2, "NewObjects");

        hs.Add(new RenameHint(s1.Tables["Objects"].Path, s2.Tables["NewObjects"].Path));
        var copyDataHint = new CopyDataHint();
        copyDataHint.SourceTablePath = "Tables/Types";
        copyDataHint.CopiedColumns.Add(new Pair<string>("Tables/Types/Columns/Data", "Tables/NewObjects/Columns/NewData"));
        copyDataHint.Identities.Add(new IdentityPair("Tables/Types/Columns/Id", "Tables/NewObjects/Columns/TypeId", false));
        hs.Add(copyDataHint);
      },
        (diff, actions) => { });
    }

    [Test]
    public void ChangeColumnTypeTest()
    {
      var storage = CreateSimpleStorageModel();
      TestUpdate(storage, (s1, s2, hs) => {
        var column = s2.Tables["Types"].Columns["Data"];
        column.Type = new TypeInfo(typeof (int));
      },
        (diff, actions) => { });
    }

    [Test]
    public void RemoveReferencedTable()
    {
      var storage = CreateSimpleStorageModel();
      storage.Dump();

      TestUpdate(storage, (s1, s2, hs) => {
        var types = (TableInfo) s2.Resolve("Tables/Types");
        var objects = (TableInfo) s2.Resolve("Tables/Objects");
        objects.ForeignKeys.Clear();
        types.Remove();
      },
        (diff, actions) => Assert.IsTrue(
          actions
            .Flatten()
            .OfType<RemoveNodeAction>()
            .Any(a => a.Path=="Tables/Objects/ForeignKeys/FK_TypeId")));
    }

    [Test]
    public void ComplexTest()
    {
      var storage = CreateSimpleStorageModel();
      TestUpdate(storage, (s1, s2, hs) => {
        var oldColumn = s1.Tables["Types"].Columns["Data"];
        s2.Tables["Types"].Remove();
        s2.Tables["Objects"].ForeignKeys[0].Remove();
        s2.Tables["Objects"].Name = "NewObjects";
        s2.Tables["NewObjects"].Columns["Id"].Name = "NewId";
        var newColumn = new ColumnInfo(s2.Tables["NewObjects"], "NewData", oldColumn.Type);
        s2.Tables["NewObjects"].PrimaryIndex.ValueColumns.Clear();
        s2.Tables["NewObjects"].PrimaryIndex.PopulateValueColumns();

        hs.Add(new RenameHint(s1.Tables["Objects"].Path,
          s2.Tables["NewObjects"].Path));
        hs.Add(new RenameHint(s1.Tables["Objects"].Columns["Id"].Path,
          s2.Tables["NewObjects"].Columns["NewId"].Path));
        var copyDataHint = new CopyDataHint();
        copyDataHint.SourceTablePath = "Tables/Types";
        copyDataHint.CopiedColumns.Add(new Pair<string>("Tables/Types/Columns/Data", "Tables/NewObjects/Columns/NewData"));
        copyDataHint.Identities.Add(new IdentityPair("Tables/Types/Columns/Id", "Tables/NewObjects/Columns/NewId", false));
        hs.Add(copyDataHint);
      },
        (diff, actions) => { });
    }

    private static StorageInfo CreateSimpleStorageModel()
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

    private static void RepopulateValueColumns(StorageInfo model, string tableName)
    {
      var pk = model.Tables[tableName].PrimaryIndex;
      pk.ValueColumns.Clear();
      pk.PopulateValueColumns();
    }

    private static void TestUpdate(StorageInfo origin, Action<StorageInfo, StorageInfo, HintSet> mutator, Action<Difference, ActionSequence> validator)
    {
      // �������� � ����� �������� - ������� ;)
      TestUpdate(origin, mutator, validator, true);
      // TestUpdate(origin, mutator, null, false);
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
      return (StorageInfo) server.Clone(null, server.Name);
      // return (StorageInfo) LegacyBinarySerializer.Instance.Clone(server);
    }

    #endregion
  }
}