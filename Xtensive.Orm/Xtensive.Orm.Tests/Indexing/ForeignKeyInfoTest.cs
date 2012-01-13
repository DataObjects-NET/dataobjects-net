// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.23

using Xtensive.Core;
using Xtensive.Orm;
using Xtensive.Orm.Upgrade.Model;
using NUnit.Framework;
using Xtensive.Testing;

namespace Xtensive.Orm.Tests.Indexing
{
  [TestFixture]
  public class ForeignKeyInfoTest
  {
    private StorageInfo storage;
    private TableInfo referencingTable;
    private SecondaryIndexInfo referencingIndex;
    private SecondaryIndexInfo invalideReferencingIndex;

    private TableInfo referencedTable;
    private PrimaryIndexInfo foreignPrimary;

    [SetUp]
    public void BuildStorageModel()
    {
      storage = new StorageInfo("storage");
      referencingTable = new TableInfo(storage, "referencingTable");
      var pkColumn = new ColumnInfo(referencingTable, "Id", new TypeInfo(typeof (int), null));
      var fkColumn = new ColumnInfo(referencingTable, "foreignId", new TypeInfo(typeof (int?), null));
      var fkColumn2 = new ColumnInfo(referencingTable, "invalideForeignId", new TypeInfo(typeof (string), null));
      var primaryKey = new PrimaryIndexInfo(referencingTable, "PK1");
      new KeyColumnRef(primaryKey, pkColumn);
      referencingIndex = new SecondaryIndexInfo(referencingTable, "FK");
      new KeyColumnRef(referencingIndex, fkColumn);
      referencingIndex.PopulatePrimaryKeyColumns();
      invalideReferencingIndex = new SecondaryIndexInfo(referencingTable, "FK2");
      invalideReferencingIndex.PopulatePrimaryKeyColumns();
      new KeyColumnRef(invalideReferencingIndex, fkColumn2);
      primaryKey.PopulateValueColumns();

      referencedTable = new TableInfo(storage, "referencedTable");
      pkColumn = new ColumnInfo(referencedTable, "Id", new TypeInfo(typeof (int), null));
      foreignPrimary = new PrimaryIndexInfo(referencedTable, "Id");
      new KeyColumnRef(foreignPrimary, pkColumn);
    }

    [Test]
    public void ForeignKeyValidationTest()
    {
      storage.Validate();

      var foreignKey = new ForeignKeyInfo(referencingTable, "ForeignKey");
      AssertEx.Throws<AggregateException>(foreignKey.Validate);
      foreignKey.PrimaryKey = foreignPrimary;
      AssertEx.Throws<AggregateException>(foreignKey.Validate);

      foreignKey.ForeignKeyColumns.Set(invalideReferencingIndex);
      AssertEx.Throws<AggregateException>(foreignKey.Validate);
      foreignKey.ForeignKeyColumns.Set(referencingIndex);
      foreignKey.Validate();
    }
  }
}