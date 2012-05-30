// Copyright (C) 2003-2010 Xtensive LLC.
// All rights reserved.
// For conditions of distribution and use, see license.
// Created by: Ivan Galkin
// Created:    2009.03.17

using System;
using NUnit.Framework;
using Xtensive.Core;
using Xtensive.Testing;
using Xtensive.Modelling.Actions;
using Xtensive.Orm.Upgrade.Model;
using AggregateException = Xtensive.Core.AggregateException;

namespace Xtensive.Orm.Tests.Indexing
{
  [TestFixture]
  public class StorageInfoTest
  {
    protected StorageModel storage;
    protected TableInfo table1;
    protected TableInfo table2;
    protected PrimaryIndexInfo pi1;
    protected PrimaryIndexInfo pi2;
    protected SecondaryIndexInfo si1;
    protected SecondaryIndexInfo si2;
    protected ForeignKeyInfo fk1;
    protected StorageColumnInfo column1;
    protected StorageColumnInfo column2;
    protected StorageColumnInfo column3;
    protected StorageColumnInfo column4;
    protected StorageColumnInfo column5;

    [SetUp]
    public void CreateModel()
    {
      storage = new StorageModel("storage") { Actions = new ActionSequence() };
      // Table 1
      table1 = new TableInfo(storage, "table1");
      pi1 = new PrimaryIndexInfo(table1, "pk1");
      column1 = new StorageColumnInfo(table1, "col1", new StorageTypeInfo(typeof(string), null, false));
      column2 = new StorageColumnInfo(table1, "col2", new StorageTypeInfo(typeof(string), null));
      column3 = new StorageColumnInfo(table1, "col3", new StorageTypeInfo(typeof(string), null));
      new KeyColumnRef(pi1, column1, Direction.Positive);
      pi1.PopulateValueColumns();
      si1 = new SecondaryIndexInfo(table1, "ix1");
      new KeyColumnRef(si1, column2, Direction.Positive);
      si1.PopulatePrimaryKeyColumns();

      // Table 2
      table2 = new TableInfo(storage, "table2");
      pi2 = new PrimaryIndexInfo(table2, "pk2");
      column4 = new StorageColumnInfo(table2, "col4", new StorageTypeInfo(typeof(int), null));
      column5 = new StorageColumnInfo(table2, "col5", new StorageTypeInfo(typeof(string), null));
      new KeyColumnRef(pi2, column4, Direction.Negative);
      pi2.PopulateValueColumns();
      si2 = new SecondaryIndexInfo(table2, "ix2");
      new KeyColumnRef(si2, column5, Direction.Positive);
      si2.PopulatePrimaryKeyColumns();

      // Foreign keys
      fk1 = new ForeignKeyInfo(table2, "fk1")
        {
          PrimaryKey = pi1
        };
      fk1.ForeignKeyColumns.Set(si2);
    }

    [Test]
    public void ValidateModel()
    {
      storage.Validate();
    }

    [Test]
    public void StorageLogTest()
    {
      Log.Info("Actions:");
      Log.Info("{0}", storage.Actions);
    }

    [Test]
    public void RemoveReferencedColumnTest()
    {
      column5.Remove();
      AssertEx.Throws<AggregateException>(storage.Validate);
    }

    [Test]
    public void RemoveReferencedSecondaryIndexTest()
    {
      column5.Remove();
      si2.Remove();
      AssertEx.Throws<AggregateException>(storage.Validate);
    }

    [TearDown]
    public void Dump()
    {
      storage.Dump();
    }
  }
}