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
  public class DataUpgradeTest
  {
    [Test]
    public void CreateModelTest()
    {
      var storage = CreateSimpleStorageModel();
    }

    [Test]
    public void RemoveTypeTest()
    {
      var storage = CreateSimpleStorageModel();
      TestUpdate(storage, (s1, s2, hs) => {
        var fkCRefB = s2.Resolve("Tables/C/ForeignKeys/FK_CRefB") as ForeignKeyInfo;
        var tB = s2.Resolve("Tables/B") as TableInfo;
        fkCRefB.Remove();
        tB.Remove();

        var deleteDataHint = new DeleteDataHint();
        deleteDataHint.SourceTablePath = "Tables/A";
        deleteDataHint.Identities.Add(new IdentityPair("Tables/A/Columns/Id", "102", true));
        hs.Add(deleteDataHint);
        var updateDataHint1 = new UpdateDataHint();
        updateDataHint1.SourceTablePath = "Tables/C";
        updateDataHint1.UpdateParameter.Add(new Pair<string, object>("Tables/C/Columns/RefA", "null"));
        updateDataHint1.Identities.Add(new IdentityPair("Tables/C/Columns/RefA", "Tables/B/Columns/Id", false));
        var updateDataHint2 = new UpdateDataHint();
        updateDataHint2.SourceTablePath = "Tables/C";
        updateDataHint2.UpdateParameter.Add(new Pair<string, object>("Tables/C/Columns/RefB", "null"));
        updateDataHint2.Identities.Add(new IdentityPair("Tables/C/Columns/RefB", "Tables/B/Columns/Id", false));
        hs.Add(updateDataHint1);
        hs.Add(updateDataHint2);
      },
        (diff, actions) => { });
    }

    private static StorageInfo CreateSimpleStorageModel()
    {
      var storage = new StorageInfo("Storage");

      var tA = new TableInfo(storage, "A");
      var tB = new TableInfo(storage, "B");
      var tC = new TableInfo(storage, "C");

      var idA = new ColumnInfo(tA, "Id", new TypeInfo(typeof (int)));
      var idB = new ColumnInfo(tB, "Id", new TypeInfo(typeof (int)));
      var idC = new ColumnInfo(tC, "Id", new TypeInfo(typeof (int)));
      var refCA = new ColumnInfo(tC, "RefA", new TypeInfo(typeof (int)));
      var refCB = new ColumnInfo(tC, "RefB", new TypeInfo(typeof (int)));
      var refCC = new ColumnInfo(tC, "RefC", new TypeInfo(typeof (int)));

      var pkA = new PrimaryIndexInfo(tA, "PK_A");
      var pkB = new PrimaryIndexInfo(tB, "PK_B");
      var pkC = new PrimaryIndexInfo(tC, "PK_C");
      new KeyColumnRef(pkA, idA);
      new KeyColumnRef(pkB, idB);
      new KeyColumnRef(pkC, idC);
      pkC.PopulateValueColumns();
      var ixRefCA = new SecondaryIndexInfo(tC, "FK_CRefA");
      new KeyColumnRef(ixRefCA, refCA);
      ixRefCA.PopulatePrimaryKeyColumns();
      var ixRefCB = new SecondaryIndexInfo(tC, "FK_CRefB");
      new KeyColumnRef(ixRefCB, refCB);
      ixRefCB.PopulatePrimaryKeyColumns();
      var ixRefCC = new SecondaryIndexInfo(tC, "FK_CRefC");
      new KeyColumnRef(ixRefCC, refCC);
      ixRefCC.PopulatePrimaryKeyColumns();

      var fkCRefA = new ForeignKeyInfo(tC, "FK_CRefA");
      fkCRefA.PrimaryKey = pkA;
      fkCRefA.ForeignKeyColumns.Set(ixRefCA);

      var fkCRefB = new ForeignKeyInfo(tC, "FK_CRefB");
      fkCRefB.PrimaryKey = pkB;
      fkCRefB.ForeignKeyColumns.Set(ixRefCB);

      var fkCRefC = new ForeignKeyInfo(tC, "FK_CRefC");
      fkCRefC.PrimaryKey = pkC;
      fkCRefC.ForeignKeyColumns.Set(ixRefCC);

      var fkCA = new ForeignKeyInfo(tC, "FK_CA");
      fkCA.PrimaryKey = pkA;
      fkCA.ForeignKeyColumns.Set(pkC);

      var fkBA = new ForeignKeyInfo(tB, "FK_BA");
      fkBA.PrimaryKey = pkA;
      fkBA.ForeignKeyColumns.Set(pkB);

      storage.Validate();

      return storage;
    }

    private static void AddClearHints(HintSet hintSet)
    {
      //var clearTypeIdHint = new ClearDataHint("Tables/Objects/Columns/TypeId", ClearAction.SetToDefault);
      //clearTypeIdHint.Dependences.Add("Tables/Types/Columns/Id");
      //hintSet.Add(clearTypeIdHint);
     
      //var removeTypesRecords = new ClearDataHint("Tables/Objects", ClearAction.Remove);
      //removeTypesRecords.Dependences.Add("Tables/Types/Columns/Id");
      //removeTypesRecords.IdentityParameters.Add(new IdentityParameter("Tables/Types/Columns/Id", "Tables/Objects/Columns/TypeId"));

      //hintSet.Add(removeTypesRecords);
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