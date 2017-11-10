// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Alex Yakunin
// Created:    2009.04.17

using System;
using System.Collections.Generic;
using NUnit.Framework;
using Xtensive.Collections;
using Xtensive.Core;
using Xtensive.Modelling;
using Xtensive.Modelling.Actions;
using Xtensive.Modelling.Comparison;
using Xtensive.Modelling.Comparison.Hints;
using System.Linq;
using Xtensive.Orm.Tests.Core.Modelling.IndexingModel;

namespace Xtensive.Orm.Tests.Core.Modelling
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
        new ColumnInfo(s1.Tables["Types"], "Temp_Data", new TypeInfo(typeof (int)));
        RepopulateValueColumns(s1, "Types");
        new ColumnInfo(s2.Tables["Types"], "Temp_Data", new TypeInfo(typeof (int)));
        RepopulateValueColumns(s2, "Types");
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
    public void MutualRenameTest2()
    {
      var storage = CreateSimpleStorageModel();
      TestUpdate(storage, (s1, s2, hs) => {
        var types = s2.Resolve("Tables/Types") as TableInfo;
        var objects= s2.Resolve("Tables/Objects") as TableInfo;
        types.Name = "Temp";
        objects.Name = "Types";
        types.Name = "Objects";
        types.Columns["Value"].Name = "NewValue";
        RepopulateValueColumns(s2, "Objects");
        hs.Add(new RenameHint("Tables/Types", "Tables/Objects"));
        hs.Add(new RenameHint("Tables/Objects", "Tables/Types"));
        hs.Add(new RenameHint("Tables/Types/Columns/Value", "Tables/Objects/Columns/NewValue"));
      },
        (diff, actions) => { });
    }

    [Test]
    public void RemoveDependentPropertyTest1()
    {
      var storage = CreateSimpleStorageModel();

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

      TestUpdate(storage, (s1, s2, hs) => {
        var t1 = (TableInfo) s2.Resolve("Tables/Types");
        t1.Columns["Value"].Name = "NewValue";
        RepopulateValueColumns(s2, "Types");
      },
        (diff, actions) => Assert.IsTrue(
          actions
            .Flatten()
            .OfType<RemoveNodeAction>()
            .Any(a => a.Path == "Tables/Types/Columns/Value")));
    }

    [Test]
    public void RemoveDependentPropertyTest4()
    {
      var storage = CreateSimpleStorageModel();

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

      TestUpdate(storage, (s1, s2, hs) => {
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

      TestUpdate(storage, (source, target, hs) => {
        var type = (TableInfo)target.Resolve("Tables/Types");
        var typeOldPath = type.Path;
        var pkType = type.PrimaryIndex;
        var pkOldPath = pkType.Path;

        pkType.Name = "PK_NewTypes";
        type.Name = "NewTypes";

        hs.Add(new RenameHint(typeOldPath, type.Path));
      },
      (diff, actions) => {
        var flatten = actions.Flatten().ToList();
        Assert.AreEqual(11, flatten.Count);
      });
    }

    [Test]
    public void RenameTest3()
    {
      var storage = CreateSimpleStorageModel();
      storage.Tables["Types"].Index = storage.Tables.Count - 1;

      TestUpdate(storage, (s1, s2, hs) => {
        var t2 = (TableInfo) s2.Resolve("Tables/Types");
        string t2OldPath = t2.Path;
        t2.Name = "NewTypes";
        hs.Add(new RenameHint(t2OldPath, t2.Path));
      },
      (diff, actions) => {
        var flatten = actions.Flatten().ToList();
        Assert.AreEqual(1, flatten.Count);
        Assert.IsTrue(flatten.All(a => a is MoveNodeAction));
      });
    }
    
    [Test]
    public void RenameTest4()
    {
      var storage = new StorageInfo(".");
      new TableInfo(storage, "Employee");
      
      TestUpdate(storage, (s1, s2, hs) => {
        s2.Tables.Clear();
        new TableInfo(s2, "Employee");
        new TableInfo(s2, "RcEmployee");
        
        hs.Add(new RenameHint("Tables/Employee", "Tables/RcEmployee"));
      },
      (diff, actions) => {
      });
    }

    [Test]
    public void ChangeFullTextIndexTest()
    {
      var storage = CreateSimpleStorageModel();

      TestUpdate(storage, (source, target, hs) => {
        var type = (TableInfo) target.Resolve("Tables/Types");
        var ftIndex = type.FullTextIndex;
        var valueColumn = type.Columns["Value"];
        var oldPath = valueColumn.Path;
        valueColumn.Name = "StringValue";
        var dataColumn = type.Columns["Data"];
        new FullTextColumnRef(ftIndex, dataColumn);
        hs.Add(new RenameHint(oldPath, valueColumn.Path));
      },
      (diff, actions) => {
//        var flatten = actions.Flatten().ToList();
//        Assert.AreEqual(1, flatten.Count);
//        Assert.IsTrue(flatten.All(a => a is MoveNodeAction));
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

        var copyDataHint = new CopyDataHint(
          "Tables/Types",
          new List<IdentityPair> {new IdentityPair("Tables/Types/Columns/Id", "Tables/Objects/Columns/TypeId", false)},
          new List<Pair<string>> {new Pair<string>("Tables/Types/Columns/Data", "Tables/Objects/Columns/Data")});
        
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

        var copyDataHint = new CopyDataHint(
          "Tables/Types",
          new List<IdentityPair> {new IdentityPair("Tables/Types/Columns/Id", "Tables/Objects/Columns/TypeId", false)},
          new List<Pair<string>> {new Pair<string>("Tables/Types/Columns/Data", "Tables/Objects/Columns/Data")});

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
        var copyDataHint = new CopyDataHint(
          "Tables/Types",
          new List<IdentityPair> {new IdentityPair("Tables/Types/Columns/Id", "Tables/NewObjects/Columns/TypeId", false)},
          new List<Pair<string>> {new Pair<string>("Tables/Types/Columns/Data", "Tables/NewObjects/Columns/NewData")});

        hs.Add(copyDataHint);
      },
        (diff, actions) => { });
    }

    [Test]
    public void ChangePrimaryColumnTypeTest()
    {
      var storage = CreateSimpleStorageModel();
      TestUpdate(storage, (s, t, hs) => {
          var pk = t.Tables["Types"].PrimaryIndex;
          var id = t.Tables["Types"].Columns["Id"];
          id.Type = new TypeInfo(typeof(Guid));
          var fk = t.Tables["Objects"].ForeignKeys["FK_TypeId"];
          fk.ForeignKeyColumns.Set(pk);
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
        var copyDataHint = new CopyDataHint(
          "Tables/Types",
          new List<IdentityPair> {new IdentityPair("Tables/Types/Columns/Id", "Tables/NewObjects/Columns/TypeId", false)},
          new List<Pair<string>> {new Pair<string>("Tables/Types/Columns/Data", "Tables/NewObjects/Columns/NewData")});
        
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

      var tiFt = new FullTextIndexInfo(t, "FT_Index");
      new FullTextColumnRef(tiFt, tValue);

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
      // Классное я слово придумал - мутатор ;)
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
      TestLog.Info("Update test ({0} hints)", useHints ? "with" : "without");
      s1.Validate();
      s2.Validate();

      // Comparing different models
      TestLog.Info("Comparing models:");
      var comparer = new Comparer();
      var diff = comparer.Compare(s1, s2, hints);
      TestLog.Info("\r\nDifference:\r\n{0}", diff);
      var actions = new ActionSequence() {
        new Upgrader().GetUpgradeSequence(diff, hints, comparer)
      };
      TestLog.Info("\r\nActions:\r\n{0}", actions);
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